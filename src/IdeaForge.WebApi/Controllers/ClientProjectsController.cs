using IdeaForge.Application.ClientProjects;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage client-owned systems/projects and the repositories owned by those systems.
/// </summary>
[ApiController]
[Route("api/client-projects")]
public sealed class ClientProjectsController : ControllerBase
{
    private readonly IClientProjectService _service;

    public ClientProjectsController(IClientProjectService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get a client-owned system/project by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClientProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientProjectDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetProjectAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Update a client-owned system/project.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClientProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientProjectDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateClientProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdateProjectAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid client system/project request", exception, StatusCodes.Status400BadRequest));
        }
    }

    /// <summary>
    /// List repositories under a client-owned system/project.
    /// </summary>
    [HttpGet("{projectId:guid}/repositories")]
    [ProducesResponseType<IReadOnlyList<ProjectRepositoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectRepositoryDto>>> ListRepositoriesAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await _service.ListRepositoriesAsync(projectId, cancellationToken);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a source repository under a client-owned system/project.
    /// </summary>
    [HttpPost("{projectId:guid}/repositories")]
    [ProducesResponseType<ProjectRepositoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectRepositoryDto>> CreateRepositoryAsync(
        Guid projectId,
        [FromBody] CreateProjectRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateRepositoryAsync(projectId, request, cancellationToken);
            return Created($"/api/project-repositories/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid system repository request", exception, StatusCodes.Status400BadRequest));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem("System repository conflict", exception, StatusCodes.Status409Conflict));
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
