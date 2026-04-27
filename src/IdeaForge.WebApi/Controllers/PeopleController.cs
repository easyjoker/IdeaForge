using IdeaForge.Application.Personnel;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage people in the company directory.
/// </summary>
[ApiController]
[Route("api/people")]
public sealed class PeopleController : ControllerBase
{
    private readonly IPersonnelService _service;

    public PeopleController(IPersonnelService service)
    {
        _service = service;
    }

    /// <summary>
    /// List all people.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PersonDirectoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonDirectoryDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var people = await _service.ListPeopleAsync(cancellationToken);
        return Ok(people);
    }

    /// <summary>
    /// Get a person by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PersonDirectoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDirectoryDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var person = await _service.GetPersonAsync(id, cancellationToken);
        return person is null ? NotFound() : Ok(person);
    }

    /// <summary>
    /// Create a person. Use kind 1 for Human and 2 for AI.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<PersonDirectoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonDirectoryDto>> CreateAsync(
        [FromBody] CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreatePersonAsync(request, cancellationToken);
            return Created($"/api/people/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid person request", exception, StatusCodes.Status400BadRequest));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Person conflict", exception, StatusCodes.Status409Conflict));
        }
    }

    /// <summary>
    /// Update a person. Changing an AI person to Human removes AI agent settings from linked employee records.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PersonDirectoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDirectoryDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdatePersonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdatePersonAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid person request", exception, StatusCodes.Status400BadRequest));
        }
    }

    /// <summary>
    /// Delete a person that is not assigned as an employee.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeletePersonAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Person conflict", exception, StatusCodes.Status409Conflict));
        }
    }

    private static ProblemDetails CreateProblem(string title, Exception exception, int statusCode) =>
        new()
        {
            Title = title,
            Detail = exception.Message,
            Status = statusCode
        };
}
