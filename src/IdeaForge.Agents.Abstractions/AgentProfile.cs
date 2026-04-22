namespace IdeaForge.Agents.Abstractions;

public sealed class AgentProfile
{
    public Employee Employee { get; set; } = new();

    public AgentData AgentData { get; set; } = new();
}
