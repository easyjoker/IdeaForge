namespace IdeaForge.Agents.Codex;

public sealed class CodexAgentOptions
{
    public string CommandPath { get; set; } = "codex";

    public string DefaultModel { get; set; } = "gpt-5.4";

    public string Sandbox { get; set; } = "danger-full-access";

    public bool DangerouslyBypassApprovalsAndSandbox { get; set; } = true;

    public bool SkipGitRepoCheck { get; set; } = true;

    public bool JsonOutput { get; set; }
}
