using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Domain.Entities;

public class WorkflowNode
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public string AgentType { get; set; } = string.Empty;

    public List<Guid> DependsOn { get; set; } = [];

    public NodeStatus Status { get; set; } = NodeStatus.Pending;

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public bool RequiresApproval { get; set; }

    public Guid? ApprovalId { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }
        = ApprovalStatus.Pending;
}