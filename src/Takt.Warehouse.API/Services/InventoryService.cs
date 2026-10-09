using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Takt.Warehouse.API.Domain;
using Takt.Warehouse.API.Persistence;
using Takt.Warehouse.API.Telemetry;

namespace Takt.Warehouse.API.Services;

public sealed class InventoryService(
    WarehouseDbContext dbContext,
    IInventoryMetrics metrics,
    TimeProvider timeProvider) : IInventoryService
{
    public async Task<SkuResponse> CreateSkuAsync(CreateSkuRequest request, CancellationToken cancellationToken)
    {
        ValidateRequired(request.Code, nameof(request.Code));
        ValidateRequired(request.Name, nameof(request.Name));
        ValidateRequired(request.Unit, nameof(request.Unit));

        var entity = new Sku
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = TrimOptional(request.Description),
            Category = TrimOptional(request.Category),
            Unit = request.Unit.Trim(),
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.Skus.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new BusinessException(
                "inventory.sku_conflict",
                "SKU already exists",
                StatusCodes.Status409Conflict,
                $"SKU with code '{entity.Code}' already exists.");
        }

        return new SkuResponse(entity.Id, entity.Code, entity.Name, entity.Description, entity.Category, entity.Unit, entity.IsActive);
    }

    public async Task<WarehouseResponse> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        ValidateRequired(request.BranchId, nameof(request.BranchId));
        ValidateRequired(request.Name, nameof(request.Name));

        var entity = new WarehouseEntity
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId.Trim(),
            Name = request.Name.Trim(),
            Description = TrimOptional(request.Description),
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.Warehouses.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new WarehouseResponse(entity.Id, entity.BranchId, entity.Name, entity.Description, entity.IsActive);
    }

    public Task<ApiResult> ReceiveAsync(Guid warehouseId, ReceiveStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken)
    {
        ValidatePositive(request.Quantity, nameof(request.Quantity));
        return ExecuteIdempotentMutationAsync($"receipt:{warehouseId}", idempotencyKey, request, async now =>
        {
            var warehouse = await GetWarehouseAsync(warehouseId, cancellationToken);
            var sku = await GetSkuAsync(request.SkuId, cancellationToken);
            EnsureActive(warehouse.IsActive, "inventory.warehouse_inactive", "Warehouse is inactive");
            EnsureActive(sku.IsActive, "inventory.sku_inactive", "SKU is inactive");

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                SkuId = request.SkuId,
                Type = MovementType.Receipt,
                Quantity = request.Quantity,
                Reason = TrimOptional(request.Reason),
                CreatedBy = createdBy,
                OccurredAt = now
            };

            await IncreaseBalanceAsync(warehouseId, request.SkuId, request.Quantity, now, cancellationToken);
            dbContext.StockMovements.Add(movement);
            AddOutbox("StockReceived", new
            {
                eventId = Guid.NewGuid(),
                eventType = "StockReceived",
                occurredAt = now,
                movementId = movement.Id,
                warehouseId,
                request.SkuId,
                request.Quantity
            }, now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdateOutboxMetricAsync(cancellationToken);

            return new ApiResult(StatusCodes.Status201Created, ToMovement(movement));
        }, cancellationToken);
    }

    public Task<ApiResult> IssueAsync(Guid warehouseId, IssueStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken)
    {
        ValidateRequired(request.MemberId, nameof(request.MemberId));
        ValidatePositive(request.Quantity, nameof(request.Quantity));
        return ExecuteIdempotentMutationAsync($"issue:{warehouseId}", idempotencyKey, request, async now =>
        {
            var warehouse = await GetWarehouseAsync(warehouseId, cancellationToken);
            var sku = await GetSkuAsync(request.SkuId, cancellationToken);
            EnsureActive(warehouse.IsActive, "inventory.warehouse_inactive", "Warehouse is inactive");
            EnsureActive(sku.IsActive, "inventory.sku_inactive", "SKU is inactive");

            var member = await dbContext.InventoryMembers
                .SingleOrDefaultAsync(x => x.MemberId == request.MemberId, cancellationToken);
            if (member is null)
            {
                metrics.TrackIssue(false);
                throw new BusinessException(
                    "inventory.member_not_found",
                    "Member is not known by inventory",
                    StatusCodes.Status422UnprocessableEntity,
                    $"Member '{request.MemberId}' is missing in local projection.");
            }

            if (member.Status != InventoryMemberStatus.Active)
            {
                metrics.TrackIssue(false);
                throw new BusinessException(
                    "inventory.member_inactive",
                    "Member is inactive",
                    StatusCodes.Status422UnprocessableEntity,
                    $"Member '{request.MemberId}' is not active.");
            }

            var updated = await TryDecreaseBalanceAsync(warehouseId, request.SkuId, request.Quantity, now, cancellationToken);
            if (!updated)
            {
                metrics.TrackIssue(false);
                throw new BusinessException(
                    "inventory.insufficient_stock",
                    "Insufficient stock",
                    StatusCodes.Status409Conflict,
                    $"Requested {request.Quantity} items, but there is not enough stock available.");
            }

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                SkuId = request.SkuId,
                Type = MovementType.Issue,
                Quantity = request.Quantity,
                MemberId = request.MemberId.Trim(),
                CreatedBy = createdBy,
                OccurredAt = now
            };

            dbContext.StockMovements.Add(movement);
            AddOutbox("StockIssued", new
            {
                eventId = Guid.NewGuid(),
                eventType = "StockIssued",
                occurredAt = now,
                issueId = movement.Id,
                warehouseId,
                request.SkuId,
                memberId = request.MemberId,
                request.Quantity
            }, now);

            await dbContext.SaveChangesAsync(cancellationToken);
            metrics.TrackIssue(true);
            await UpdateOutboxMetricAsync(cancellationToken);
            return new ApiResult(StatusCodes.Status201Created, ToMovement(movement));
        }, cancellationToken);
    }

    public Task<ApiResult> ReturnAsync(Guid warehouseId, ReturnStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken)
    {
        ValidateRequired(request.MemberId, nameof(request.MemberId));
        ValidatePositive(request.Quantity, nameof(request.Quantity));
        if (!request.IssueMovementId.HasValue)
        {
            throw new BusinessException(
                "inventory.issue_reference_required",
                "Issue reference is required",
                StatusCodes.Status422UnprocessableEntity,
                "Return operation must contain issueMovementId.");
        }

        return ExecuteIdempotentMutationAsync($"return:{warehouseId}", idempotencyKey, request, async now =>
        {
            var warehouse = await GetWarehouseAsync(warehouseId, cancellationToken);
            var sku = await GetSkuAsync(request.SkuId, cancellationToken);
            EnsureActive(warehouse.IsActive, "inventory.warehouse_inactive", "Warehouse is inactive");
            EnsureActive(sku.IsActive, "inventory.sku_inactive", "SKU is inactive");

            var issue = await dbContext.StockMovements
                .SingleOrDefaultAsync(x =>
                    x.Id == request.IssueMovementId.Value &&
                    x.Type == MovementType.Issue &&
                    x.WarehouseId == warehouseId &&
                    x.SkuId == request.SkuId &&
                    x.MemberId == request.MemberId,
                    cancellationToken);
            if (issue is null)
            {
                throw new BusinessException(
                    "inventory.issue_not_found",
                    "Issue not found",
                    StatusCodes.Status404NotFound,
                    "Referenced issue movement was not found.");
            }

            var alreadyReturned = await dbContext.StockMovements
                .Where(x => x.Type == MovementType.Return && x.RelatedMovementId == issue.Id)
                .SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;

            if (alreadyReturned + request.Quantity > issue.Quantity)
            {
                throw new BusinessException(
                    "inventory.return_exceeds_issue",
                    "Return exceeds outstanding issue",
                    StatusCodes.Status409Conflict,
                    $"Return quantity exceeds outstanding quantity. Outstanding: {issue.Quantity - alreadyReturned}.");
            }

            await IncreaseBalanceAsync(warehouseId, request.SkuId, request.Quantity, now, cancellationToken);
            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                SkuId = request.SkuId,
                Type = MovementType.Return,
                Quantity = request.Quantity,
                MemberId = request.MemberId.Trim(),
                RelatedMovementId = issue.Id,
                CreatedBy = createdBy,
                OccurredAt = now
            };
            dbContext.StockMovements.Add(movement);

            AddOutbox("StockReturned", new
            {
                eventId = Guid.NewGuid(),
                eventType = "StockReturned",
                occurredAt = now,
                returnId = movement.Id,
                issueId = issue.Id,
                warehouseId,
                request.SkuId,
                memberId = request.MemberId,
                request.Quantity
            }, now);

            await dbContext.SaveChangesAsync(cancellationToken);
            metrics.TrackReturn(true);
            await UpdateOutboxMetricAsync(cancellationToken);
            return new ApiResult(StatusCodes.Status201Created, ToMovement(movement));
        }, cancellationToken);
    }

    public Task<ApiResult> TransferAsync(Guid warehouseId, TransferStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken)
    {
        ValidatePositive(request.Quantity, nameof(request.Quantity));
        if (request.TargetWarehouseId == warehouseId)
        {
            throw new BusinessException(
                "inventory.same_warehouse_transfer",
                "Source and target warehouses must be different",
                StatusCodes.Status422UnprocessableEntity,
                "Cannot transfer to the same warehouse.");
        }

        return ExecuteIdempotentMutationAsync($"transfer:{warehouseId}", idempotencyKey, request, async now =>
        {
            var source = await GetWarehouseAsync(warehouseId, cancellationToken);
            var target = await GetWarehouseAsync(request.TargetWarehouseId, cancellationToken);
            var sku = await GetSkuAsync(request.SkuId, cancellationToken);
            EnsureActive(source.IsActive, "inventory.warehouse_inactive", "Source warehouse is inactive");
            EnsureActive(target.IsActive, "inventory.target_warehouse_inactive", "Target warehouse is inactive");
            EnsureActive(sku.IsActive, "inventory.sku_inactive", "SKU is inactive");

            var decreased = await TryDecreaseBalanceAsync(warehouseId, request.SkuId, request.Quantity, now, cancellationToken);
            if (!decreased)
            {
                throw new BusinessException(
                    "inventory.insufficient_stock",
                    "Insufficient stock",
                    StatusCodes.Status409Conflict,
                    $"Requested {request.Quantity} items, but there is not enough stock available.");
            }

            await IncreaseBalanceAsync(request.TargetWarehouseId, request.SkuId, request.Quantity, now, cancellationToken);

            var transferOut = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                SkuId = request.SkuId,
                Type = MovementType.TransferOut,
                Quantity = request.Quantity,
                SourceWarehouseId = warehouseId,
                TargetWarehouseId = request.TargetWarehouseId,
                Reason = TrimOptional(request.Reason),
                CreatedBy = createdBy,
                OccurredAt = now
            };
            var transferIn = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = request.TargetWarehouseId,
                SkuId = request.SkuId,
                Type = MovementType.TransferIn,
                Quantity = request.Quantity,
                SourceWarehouseId = warehouseId,
                TargetWarehouseId = request.TargetWarehouseId,
                RelatedMovementId = transferOut.Id,
                Reason = TrimOptional(request.Reason),
                CreatedBy = createdBy,
                OccurredAt = now
            };
            dbContext.StockMovements.AddRange(transferOut, transferIn);

            AddOutbox("StockTransferred", new
            {
                eventId = Guid.NewGuid(),
                eventType = "StockTransferred",
                occurredAt = now,
                transferId = transferOut.Id,
                sourceWarehouseId = warehouseId,
                targetWarehouseId = request.TargetWarehouseId,
                request.SkuId,
                request.Quantity
            }, now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdateOutboxMetricAsync(cancellationToken);
            return new ApiResult(StatusCodes.Status201Created, ToMovement(transferOut));
        }, cancellationToken);
    }

    public Task<ApiResult> AdjustAsync(Guid warehouseId, AdjustStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Quantity == 0)
        {
            throw new BusinessException(
                "inventory.invalid_quantity",
                "Quantity must not be zero",
                StatusCodes.Status422UnprocessableEntity,
                "Adjustment quantity must not be zero.");
        }

        ValidateRequired(request.Reason, nameof(request.Reason));
        return ExecuteIdempotentMutationAsync($"adjustment:{warehouseId}", idempotencyKey, request, async now =>
        {
            var warehouse = await GetWarehouseAsync(warehouseId, cancellationToken);
            var sku = await GetSkuAsync(request.SkuId, cancellationToken);
            EnsureActive(warehouse.IsActive, "inventory.warehouse_inactive", "Warehouse is inactive");
            EnsureActive(sku.IsActive, "inventory.sku_inactive", "SKU is inactive");

            if (request.Quantity > 0)
            {
                await IncreaseBalanceAsync(warehouseId, request.SkuId, request.Quantity, now, cancellationToken);
            }
            else
            {
                var decreased = await TryDecreaseBalanceAsync(warehouseId, request.SkuId, Math.Abs(request.Quantity), now, cancellationToken);
                if (!decreased)
                {
                    throw new BusinessException(
                        "inventory.insufficient_stock",
                        "Insufficient stock for adjustment",
                        StatusCodes.Status409Conflict,
                        "Adjustment would result in negative stock.");
                }
            }

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                SkuId = request.SkuId,
                Type = MovementType.Adjustment,
                Quantity = Math.Abs(request.Quantity),
                Reason = request.Reason.Trim(),
                CreatedBy = createdBy,
                OccurredAt = now
            };
            dbContext.StockMovements.Add(movement);

            AddOutbox("StockAdjusted", new
            {
                eventId = Guid.NewGuid(),
                eventType = "StockAdjusted",
                occurredAt = now,
                adjustmentId = movement.Id,
                warehouseId,
                request.SkuId,
                quantityDelta = request.Quantity,
                reason = request.Reason
            }, now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdateOutboxMetricAsync(cancellationToken);
            return new ApiResult(StatusCodes.Status201Created, ToMovement(movement));
        }, cancellationToken);
    }

    public async Task UpsertMemberProjectionAsync(UpsertMemberRequest request, CancellationToken cancellationToken)
    {
        ValidateRequired(request.MemberId, nameof(request.MemberId));
        ValidateRequired(request.BranchId, nameof(request.BranchId));

        var member = await dbContext.InventoryMembers
            .SingleOrDefaultAsync(x => x.MemberId == request.MemberId, cancellationToken);

        if (member is null)
        {
            dbContext.InventoryMembers.Add(new InventoryMember
            {
                MemberId = request.MemberId.Trim(),
                BranchId = request.BranchId.Trim(),
                Status = request.Status,
                UpdatedAt = timeProvider.GetUtcNow()
            });
        }
        else
        {
            member.BranchId = request.BranchId.Trim();
            member.Status = request.Status;
            member.UpdatedAt = timeProvider.GetUtcNow();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessMemberEventAsync(MemberEventEnvelope request, CancellationToken cancellationToken)
    {
        ValidateRequired(request.MessageId, nameof(request.MessageId));
        ValidateRequired(request.EventType, nameof(request.EventType));
        ValidateRequired(request.MemberId, nameof(request.MemberId));
        ValidateRequired(request.BranchId, nameof(request.BranchId));

        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var processed = await dbContext.InboxMessages
                .AsNoTracking()
                .AnyAsync(x => x.MessageId == request.MessageId, cancellationToken);
            if (processed)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var status = request.EventType switch
            {
                "MemberCreated" => InventoryMemberStatus.Suspended,
                "MemberActivated" => InventoryMemberStatus.Active,
                "MemberSuspended" => InventoryMemberStatus.Suspended,
                "MemberLeft" => InventoryMemberStatus.Left,
                "MemberTransferred" => InventoryMemberStatus.Active,
                _ => throw new BusinessException(
                    "inventory.unsupported_member_event",
                    "Unsupported member event",
                    StatusCodes.Status422UnprocessableEntity,
                    $"Event type '{request.EventType}' is not supported.")
            };

            await UpsertMemberProjectionAsync(new UpsertMemberRequest(request.MemberId, request.BranchId, status), cancellationToken);

            dbContext.InboxMessages.Add(new InboxMessage
            {
                MessageId = request.MessageId,
                MessageType = request.EventType,
                ProcessedAt = timeProvider.GetUtcNow()
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            metrics.TrackEventProcessingFailure();
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ApiResult> ExecuteIdempotentMutationAsync<TRequest>(
        string operation,
        string key,
        TRequest request,
        Func<DateTimeOffset, Task<ApiResult>> mutation,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "inventory.idempotency_key_required",
                "Idempotency-Key header is required",
                StatusCodes.Status400BadRequest,
                "Mutating operations require Idempotency-Key header.");
        }

        var requestHash = BuildRequestHash(request);
        var existing = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key && x.Operation == operation, cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new BusinessException(
                    "inventory.idempotency_key_reused",
                    "Idempotency key reuse with different payload",
                    StatusCodes.Status409Conflict,
                    "The same Idempotency-Key was used for a different request payload.");
            }

            var replayBody = JsonSerializer.Deserialize<JsonElement>(existing.ResponseJson);
            return new ApiResult(existing.StatusCode, replayBody);
        }

        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = timeProvider.GetUtcNow();
            var result = await mutation(now);

            dbContext.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                Key = key.Trim(),
                Operation = operation,
                RequestHash = requestHash,
                StatusCode = result.StatusCode,
                ResponseJson = JsonSerializer.Serialize(result.Body),
                CreatedAt = now
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task IncreaseBalanceAsync(Guid warehouseId, Guid skuId, int quantity, DateTimeOffset now, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO stock_balances ("WarehouseId", "SkuId", "Quantity", "UpdatedAt")
            VALUES ({warehouseId}, {skuId}, {quantity}, {now})
            ON CONFLICT ("WarehouseId", "SkuId")
            DO UPDATE SET
                "Quantity" = stock_balances."Quantity" + EXCLUDED."Quantity",
                "UpdatedAt" = EXCLUDED."UpdatedAt"
            """, cancellationToken);
    }

    private async Task<bool> TryDecreaseBalanceAsync(Guid warehouseId, Guid skuId, int quantity, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE stock_balances
            SET "Quantity" = "Quantity" - {quantity},
                "UpdatedAt" = {now}
            WHERE "WarehouseId" = {warehouseId}
              AND "SkuId" = {skuId}
              AND "Quantity" >= {quantity}
            """, cancellationToken);

        return affected == 1;
    }

    private async Task<WarehouseEntity> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses.SingleOrDefaultAsync(x => x.Id == warehouseId, cancellationToken);
        return warehouse ?? throw new BusinessException(
            "inventory.warehouse_not_found",
            "Warehouse not found",
            StatusCodes.Status404NotFound,
            $"Warehouse '{warehouseId}' was not found.");
    }

    private async Task<Sku> GetSkuAsync(Guid skuId, CancellationToken cancellationToken)
    {
        var sku = await dbContext.Skus.SingleOrDefaultAsync(x => x.Id == skuId, cancellationToken);
        return sku ?? throw new BusinessException(
            "inventory.sku_not_found",
            "SKU not found",
            StatusCodes.Status404NotFound,
            $"SKU '{skuId}' was not found.");
    }

    private void AddOutbox(string eventType, object payload, DateTimeOffset occurredAt)
    {
        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = occurredAt
        });
    }

    private static MovementResponse ToMovement(StockMovement movement) =>
        new(
            movement.Id,
            movement.WarehouseId,
            movement.SkuId,
            movement.Type,
            movement.Quantity,
            movement.MemberId,
            movement.SourceWarehouseId,
            movement.TargetWarehouseId,
            movement.Reason,
            movement.RelatedMovementId,
            movement.OccurredAt);

    private static string BuildRequestHash<TRequest>(TRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }

    private static void EnsureActive(bool isActive, string code, string detail)
    {
        if (!isActive)
        {
            throw new BusinessException(code, "Entity is inactive", StatusCodes.Status422UnprocessableEntity, detail);
        }
    }

    private static void ValidateRequired(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessException(
                "inventory.validation_error",
                "Validation failed",
                StatusCodes.Status400BadRequest,
                $"Field '{name}' is required.");
        }
    }

    private static void ValidatePositive(int quantity, string name)
    {
        if (quantity <= 0)
        {
            throw new BusinessException(
                "inventory.invalid_quantity",
                "Quantity must be positive",
                StatusCodes.Status422UnprocessableEntity,
                $"Field '{name}' must be greater than zero.");
        }
    }

    private async Task UpdateOutboxMetricAsync(CancellationToken cancellationToken)
    {
        var pending = await dbContext.OutboxMessages.CountAsync(x => x.PublishedAt == null, cancellationToken);
        metrics.TrackPendingOutbox(pending);
    }

    private static string? TrimOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
