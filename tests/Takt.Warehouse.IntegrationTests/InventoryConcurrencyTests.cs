using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Takt.Warehouse.API.Domain;
using Takt.Warehouse.API.Persistence;

namespace Takt.Warehouse.IntegrationTests;

public sealed class InventoryConcurrencyTests : IntegrationTestBase
{
    [SetUp]
    public async Task CleanDatabase()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
              stock_movements,
              stock_balances,
              inventory_members,
              outbox_messages,
              inbox_messages,
              idempotency_records,
              skus,
              warehouses
            RESTART IDENTITY
            CASCADE;
            """);
    }

    [Test]
    public async Task TwoParallelIssues_WithOneItem_OnlyOneSucceeds()
    {
        var warehouseId = await CreateWarehouseAsync();
        var skuId = await CreateSkuAsync();
        await UpsertMemberAsync("M123");
        await ReceiveAsync(warehouseId, skuId, quantity: 1);

        var issuePayload = new
        {
            memberId = "M123",
            skuId,
            quantity = 1
        };

        var issue1 = PostWithIdempotencyAsync($"/warehouses/{warehouseId}/issues", issuePayload, "issue-1");
        var issue2 = PostWithIdempotencyAsync($"/warehouses/{warehouseId}/issues", issuePayload, "issue-2");
        await Task.WhenAll(issue1, issue2);

        var statuses = new[] { issue1.Result.StatusCode, issue2.Result.StatusCode };
        Assert.That(statuses.Count(x => x == HttpStatusCode.Created), Is.EqualTo(1));
        Assert.That(statuses.Count(x => x == HttpStatusCode.Conflict), Is.EqualTo(1));

        var stockResponse = await Fixture.Client.GetAsync($"/warehouses/{warehouseId}/stock/{skuId}");
        Assert.That(stockResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var stockJson = await stockResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(stockJson.GetProperty("quantity").GetInt32(), Is.EqualTo(0));
    }

    private async Task<Guid> CreateWarehouseAsync()
    {
        var response = await Fixture.Client.PostAsJsonAsync("/warehouses", new
        {
            branchId = "B001",
            name = $"Main Warehouse {Guid.NewGuid():N}",
            description = "Main branch warehouse"
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateSkuAsync()
    {
        var response = await Fixture.Client.PostAsJsonAsync("/skus", new
        {
            code = $"ICEAXE-{Guid.NewGuid():N}".Substring(0, 14),
            name = "Ice Axe",
            description = "Petzl",
            category = "Climbing",
            unit = "pcs"
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task UpsertMemberAsync(string memberId)
    {
        var response = await Fixture.Client.PutAsJsonAsync($"/members/{memberId}", new
        {
            branchId = "B001",
            status = InventoryMemberStatus.Active
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    private Task<HttpResponseMessage> PostWithIdempotencyAsync(string uri, object payload, string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Idempotency-Key", key);
        return Fixture.Client.SendAsync(request);
    }

    private async Task ReceiveAsync(Guid warehouseId, Guid skuId, int quantity)
    {
        var response = await PostWithIdempotencyAsync($"/warehouses/{warehouseId}/receipts", new
        {
            skuId,
            quantity,
            reason = "seed"
        }, $"receipt-{Guid.NewGuid():N}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
}
