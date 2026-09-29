using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Domain.Entities;

public class Workflow
{
    public Guid Id { get; set; }

    public string Requirement { get; set; } = string.Empty;

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Created;

    public int Version { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<WorkflowNode> Nodes { get; set; } = [];
}