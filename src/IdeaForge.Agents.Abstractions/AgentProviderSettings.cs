namespace IdeaForge.Agents.Abstractions;

public static class AgentProviderSettings
{
    public const string CodexReasoning = "codex.reasoning";
    public const string CodexEffort = "codex.effort";
    public const string CodexCompute = "codex.compute";

    public static string? Get(IReadOnlyDictionary<string, string> settings, string key) =>
        settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
}
