using IdeaForge.Application.Personnel;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage employee work assignments and optional AI agent settings.
/// </summary>
[ApiController]
[Route("api/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IPersonnelService _service;

    public EmployeesController(IPersonnelService service)
    {
        _service = service;
    }

    /// <summary>
    /// List all employees.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeDirectoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDirectoryDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var employees = await _service.ListEmployeesAsync(cancellationToken);
        return Ok(employees);
    }

    /// <summary>
    /// Get an employee by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeDirectoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDirectoryDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _service.GetEmployeeAsync(id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    /// <summary>
    /// Create an employee record for a person. Include agentSettings only when the selected person is AI.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<EmployeeDirectoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDirectoryDto>> CreateAsync(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateEmployeeAsync(request, cancellationToken);
            return Created($"/api/employees/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid employee request", exception, StatusCodes.Status400BadRequest));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Employee conflict", exception, StatusCodes.Status409Conflict));
        }
    }

    /// <summary>
    /// Update employee work assignment and optional AI agent settings.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmployeeDirectoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDirectoryDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdateEmployeeAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid employee request", exception, StatusCodes.Status400BadRequest));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Employee conflict", exception, StatusCodes.Status409Conflict));
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
