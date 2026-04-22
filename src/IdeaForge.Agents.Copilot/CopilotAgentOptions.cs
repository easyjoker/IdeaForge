namespace IdeaForge.Agents.Copilot;

public sealed class CopilotAgentOptions
{
    public string DefaultModel { get; set; } = "gpt-5.4";

    public string? CliPath { get; set; }

    public bool ApproveAllPermissions { get; set; } = true;
}
