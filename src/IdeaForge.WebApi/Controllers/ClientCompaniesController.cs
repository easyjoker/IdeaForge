using IdeaForge.Application.ClientProjects;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage client companies and the systems/projects owned by those companies.
/// </summary>
[ApiController]
[Route("api/client-companies")]
public sealed class ClientCompaniesController : ControllerBase
{
    private readonly IClientProjectService _service;

    public ClientCompaniesController(IClientProjectService service)
    {
        _service = service;
    }

    /// <summary>
    /// List all client companies.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientCompanyDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientCompanyDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await _service.ListCompaniesAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Get a client company by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClientCompanyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCompanyDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetCompanyAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Get a client company by stable key.
    /// </summary>
    [HttpGet("by-key/{key}")]
    [ProducesResponseType<ClientCompanyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCompanyDto>> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        var item = await _service.GetCompanyByKeyAsync(key, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create a client company.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ClientCompanyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientCompanyDto>> CreateAsync(
        [FromBody] CreateClientCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateCompanyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAsync), new { id = created.Id }, created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid client company request", exception, StatusCodes.Status400BadRequest));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Client company conflict", exception, StatusCodes.Status409Conflict));
        }
    }

    /// <summary>
    /// Update a client company.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClientCompanyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCompanyDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateClientCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdateCompanyAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid client company request", exception, StatusCodes.Status400BadRequest));
        }
    }

    /// <summary>
    /// List client-owned systems/projects under a client company.
    /// </summary>
    [HttpGet("{companyId:guid}/projects")]
    [ProducesResponseType<IReadOnlyList<ClientProjectDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ClientProjectDto>>> ListProjectsAsync(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await _service.ListProjectsAsync(companyId, cancellationToken);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a client-owned system/project under a client company.
    /// </summary>
    [HttpPost("{companyId:guid}/projects")]
    [ProducesResponseType<ClientProjectDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientProjectDto>> CreateProjectAsync(
        Guid companyId,
        [FromBody] CreateClientProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateProjectAsync(companyId, request, cancellationToken);
            return Created($"/api/client-projects/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid client system/project request", exception, StatusCodes.Status400BadRequest));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("Client system/project conflict", exception, StatusCodes.Status409Conflict));
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
