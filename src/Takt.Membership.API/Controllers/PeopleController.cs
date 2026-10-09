using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Takt.People.API.Constants;
using Takt.People.API.Services;

namespace Takt.People.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/people")]
public sealed class PeopleController(IPeopleService peopleService) : ControllerBase
{
    [HttpGet("/api/v{version:apiVersion}/club/roles")]
    public async Task<IActionResult> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await peopleService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("/api/v{version:apiVersion}/club/branches")]
    public async Task<IActionResult> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await peopleService.GetBranchesAsync(cancellationToken);
        return Ok(branches);
    }

    [HttpPost("/api/v{version:apiVersion}/club/branches")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateBranchAsync([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await peopleService.CreateBranchAsync(
                new CreateBranchCommand(request.Name, request.Code),
                cancellationToken);
            return Created($"/api/v1.0/club/branches/{branch.Id}", branch);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Branch with the same name or code already exists." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("/api/v{version:apiVersion}/club/memberships")]
    public async Task<IActionResult> SearchMembershipsAsync(
        [FromQuery] long? personId,
        [FromQuery] long? branchId,
        [FromQuery] int? roleId,
        [FromQuery] DateOnly? activeOn,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (offset < 0)
        {
            return BadRequest(new { message = "Offset cannot be negative." });
        }

        if (limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Limit should be between 1 and 200." });
        }

        var memberships = await peopleService.SearchMembershipsAsync(
            new SearchMembershipsQuery(personId, branchId, roleId, activeOn, offset, limit),
            cancellationToken);
        return Ok(memberships);
    }

    [HttpPost("/api/v{version:apiVersion}/club/memberships")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> AddMembershipAsync(
        [FromBody] AddMembershipRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var membership = await peopleService.AddMembershipAsync(
                new ClubMembershipData(
                    request.PersonId,
                    request.RoleId,
                    request.BranchId,
                    request.StartDate,
                    request.FinishDate),
                cancellationToken);
            return Created($"/api/v1.0/club/memberships/{membership.Id}", membership);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        var person = await peopleService.GetByIdAsync(id, cancellationToken);
        return person is null ? NotFound() : Ok(person);
    }

    [HttpGet]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? query,
        [FromQuery] string? alias,
        [FromQuery] int? birthYear,
        [FromQuery] int? birthMonth,
        [FromQuery] int? birthDay,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (offset < 0)
        {
            return BadRequest(new { message = "Offset cannot be negative." });
        }

        if (limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Limit should be between 1 and 200." });
        }

        var people = await peopleService.SearchAsync(
            new SearchPeopleQuery(query, alias, birthYear, birthMonth, birthDay, offset, limit),
            cancellationToken);
        return Ok(people);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] UpsertPersonRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await peopleService.CreateAsync(
                new PersonData(
                    request.FirstName,
                    request.LastName,
                    request.FathersName,
                    request.Alias,
                    request.BirthYear,
                    request.BirthMonth,
                    request.BirthDay),
                cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id, version = "1.0" }, created);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Person with the same alias already exists." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:long}/self")]
    [Authorize]
    public async Task<IActionResult> UpdateAsPersonAsync(
        [FromRoute] long id,
        [FromBody] UpsertPersonRequest request,
        CancellationToken cancellationToken)
    {
        var selfPersonId = ResolveSelfPersonId(User);
        if (selfPersonId is null || selfPersonId.Value != id)
        {
            return Forbid();
        }

        try
        {
            var updated = await peopleService.UpdateAsPersonAsync(
                id,
                new PersonData(
                    request.FirstName,
                    request.LastName,
                    request.FathersName,
                    request.Alias,
                    request.BirthYear,
                    request.BirthMonth,
                    request.BirthDay),
                cancellationToken);

            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Person with the same alias already exists." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:long}/admin")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> UpdateAsAdminAsync(
        [FromRoute] long id,
        [FromBody] UpsertPersonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await peopleService.UpdateAsAdminAsync(
                id,
                new PersonData(
                    request.FirstName,
                    request.LastName,
                    request.FathersName,
                    request.Alias,
                    request.BirthYear,
                    request.BirthMonth,
                    request.BirthDay),
                cancellationToken);

            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Person with the same alias already exists." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("consents/personal-data-processing")]
    public async Task<IActionResult> AcceptPersonalDataConsentAsync(
        [FromBody] AcceptPersonalDataConsentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await peopleService.AcceptPersonalDataConsentAsync(
                new AcceptConsentCommand(
                    request.PersonId,
                    new PersonData(
                        request.FirstName,
                        request.LastName,
                        request.FathersName,
                        request.Alias,
                        request.BirthYear,
                        request.BirthMonth,
                        request.BirthDay),
                    request.ConsentVersion,
                    request.AcceptedAt ?? DateTimeOffset.UtcNow,
                    HttpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken);
            return Ok(result);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Person with the same alias already exists." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:long}/anonymize")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> AnonymizeAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        var isAnonymized = await peopleService.AnonymizeAsync(id, cancellationToken);
        return isAnonymized ? NoContent() : NotFound();
    }

    private static long? ResolveSelfPersonId(ClaimsPrincipal user)
    {
        var raw =
            user.FindFirstValue("person_id") ??
            user.FindFirstValue("personId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub");
        return long.TryParse(raw, out var parsed) ? parsed : null;
    }
}

public sealed record UpsertPersonRequest(
    string FirstName,
    string LastName,
    string? FathersName,
    string Alias,
    int? BirthYear,
    int? BirthMonth,
    int? BirthDay);

public sealed record AcceptPersonalDataConsentRequest(
    long? PersonId,
    string FirstName,
    string LastName,
    string? FathersName,
    string Alias,
    int? BirthYear,
    int? BirthMonth,
    int? BirthDay,
    string ConsentVersion,
    DateTimeOffset? AcceptedAt);

public sealed record CreateBranchRequest(
    string Name,
    string? Code);

public sealed record AddMembershipRequest(
    long PersonId,
    int RoleId,
    long? BranchId,
    DateOnly StartDate,
    DateOnly? FinishDate);
