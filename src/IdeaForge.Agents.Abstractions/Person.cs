namespace IdeaForge.Agents.Abstractions;

public sealed class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public PersonKind Kind { get; set; } = PersonKind.Human;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = [];

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
