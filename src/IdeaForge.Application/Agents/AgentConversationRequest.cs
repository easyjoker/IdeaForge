namespace IdeaForge.Application.Agents;

/// <summary>
/// Request used to send a prompt to a specific employee-style agent.
/// </summary>
public sealed class AgentConversationRequest
{
    /// <summary>
    /// User prompt to send to the selected employee.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Optional working directory used by providers that support repository-aware execution.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Additional directories the provider is allowed to use when supported.
    /// </summary>
    public IReadOnlyList<string> AdditionalDirectories { get; init; } = Array.Empty<string>();
}
