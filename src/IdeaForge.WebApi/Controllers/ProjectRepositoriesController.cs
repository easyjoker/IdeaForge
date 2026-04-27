using IdeaForge.Application.ClientProjects;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage repository settings used by agents.
/// </summary>
[ApiController]
[Route("api/project-repositories")]
public sealed class ProjectRepositoriesController : ControllerBase
{
    private readonly IClientProjectService _service;

    public ProjectRepositoriesController(IClientProjectService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get a repository under a client system/project by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProjectRepositoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectRepositoryDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetRepositoryAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Update a repository under a client system/project.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProjectRepositoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectRepositoryDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateProjectRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdateRepositoryAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem("Invalid system repository request", exception, StatusCodes.Status400BadRequest));
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
