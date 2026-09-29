using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Domain.Models;

public class Approval
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public Guid NodeId { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public string? ApprovedBy { get; set; }

    public ApprovalStatus Status { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public DateTime? DecisionAtUtc { get; set; }
}