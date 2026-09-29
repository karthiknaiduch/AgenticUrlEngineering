using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Models;

public class AgentContext
{
    public Guid WorkflowId { get; set; }

    public string Requirement { get; set; } = string.Empty;

    public string? Architecture { get; set; }

    public List<string> Artifacts { get; set; } = [];

    public List<string> PreviousFailures { get; set; } = [];

    public List<string> Constraints { get; set; } = [];

    public List<Decision> Decisions { get; set; } = [];

    public List<string> Approvals { get; set; } = [];
    
    public List<Approval> ApprovalRecords { get; set; } = [];

    public Dictionary<string, object> Data { get; set; } = [];
}