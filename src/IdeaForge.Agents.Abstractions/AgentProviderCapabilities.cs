namespace IdeaForge.Agents.Abstractions;

public sealed record AgentProviderCapabilities(
    bool SupportsModelSelection,
    bool SupportsWorkingDirectory,
    bool SupportsStructuredOutput,
    bool SupportsNamedSessions);
