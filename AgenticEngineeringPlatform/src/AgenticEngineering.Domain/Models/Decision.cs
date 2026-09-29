namespace AgenticEngineering.Domain.Models;

public class Decision
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public string DecisionText { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public List<string> Evidence { get; set; } = [];

    public List<string> Alternatives { get; set; } = [];

    public string SelectedOption { get; set; } = string.Empty;

    public string Actor { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}