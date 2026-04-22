using IdeaForge.Application.Agents;
using Microsoft.AspNetCore.Mvc;

namespace IdeaForge.WebApi.Controllers;

/// <summary>
/// Manage employee-style agents and their execution history.
/// </summary>
[ApiController]
[Route("api/agents")]
public sealed class AgentsController : ControllerBase
{
    private readonly IAgentProfileService _service;
    private readonly IAgentConversationService _conversationService;

    public AgentsController(IAgentProfileService service, IAgentConversationService conversationService)
    {
        _service = service;
        _conversationService = conversationService;
    }

    /// <summary>
    /// List all registered agents.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AgentProfileDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AgentProfileDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var agents = await _service.ListAsync(cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Get an agent by employee identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AgentProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentProfileDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var agent = await _service.GetAsync(id, cancellationToken);
        return agent is null ? NotFound() : Ok(agent);
    }

    /// <summary>
    /// Get an agent by employee key.
    /// </summary>
    [HttpGet("by-key/{key}")]
    [ProducesResponseType<AgentProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentProfileDto>> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        var agent = await _service.GetByKeyAsync(key, cancellationToken);
        return agent is null ? NotFound() : Ok(agent);
    }

    /// <summary>
    /// List execution records for an agent by employee identifier.
    /// </summary>
    [HttpGet("{id:guid}/executions")]
    [ProducesResponseType<IReadOnlyList<AgentExecutionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AgentExecutionDto>>> ListExecutionsAsync(Guid id, CancellationToken cancellationToken)
    {
        var items = await _service.ListExecutionsAsync(id, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// List execution records for an agent by employee key.
    /// </summary>
    [HttpGet("by-key/{key}/executions")]
    [ProducesResponseType<IReadOnlyList<AgentExecutionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AgentExecutionDto>>> ListExecutionsByKeyAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _service.ListExecutionsByKeyAsync(key, cancellationToken);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a new employee-style agent.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<AgentProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgentProfileDto>> CreateAsync(
        [FromBody] CreateAgentProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return Created($"/api/agents/{created.Employee.Id}", created);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Agent key conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Update an existing employee-style agent.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<AgentProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentProfileDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateAgentProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Report a completed execution for an agent by employee identifier.
    /// </summary>
    [HttpPost("{id:guid}/executions")]
    [ProducesResponseType<AgentExecutionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentExecutionDto>> ReportExecutionAsync(
        Guid id,
        [FromBody] ReportAgentExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.ReportExecutionAsync(id, request, cancellationToken);
            return Created($"/api/agents/{id}/executions", created);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Report a completed execution for an agent by employee key.
    /// </summary>
    [HttpPost("by-key/{key}/executions")]
    [ProducesResponseType<AgentExecutionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentExecutionDto>> ReportExecutionByKeyAsync(
        string key,
        [FromBody] ReportAgentExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.ReportExecutionByKeyAsync(key, request, cancellationToken);
            return Created($"/api/agents/by-key/{Uri.EscapeDataString(key)}/executions", created);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Send a prompt to an agent by employee identifier and persist the execution result.
    /// </summary>
    [HttpPost("{id:guid}/chat")]
    [ProducesResponseType<AgentConversationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgentConversationResponse>> ChatAsync(
        Guid id,
        [FromBody] AgentConversationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _conversationService.ChatAsync(id, request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Agent is not available",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Send a prompt to an agent by employee key and persist the execution result.
    /// </summary>
    [HttpPost("by-key/{key}/chat")]
    [ProducesResponseType<AgentConversationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgentConversationResponse>> ChatByKeyAsync(
        string key,
        [FromBody] AgentConversationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _conversationService.ChatByKeyAsync(key, request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Agent is not available",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}
