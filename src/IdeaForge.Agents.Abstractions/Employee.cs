namespace IdeaForge.Agents.Abstractions;

public sealed class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Role { get; set; }

    public string? Description { get; set; }

    public string? Mission { get; set; }

    public AgentStatus Status { get; set; } = AgentStatus.Active;

    public List<string> Specialties { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = [];

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
