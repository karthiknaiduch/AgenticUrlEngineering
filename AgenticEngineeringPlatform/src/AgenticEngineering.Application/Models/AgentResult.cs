using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Models;

public class AgentResult
{
    public AgentExecutionStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;

    public FailureType FailureType { get; set; }

    public List<string> Artifacts { get; set; } = [];

    public List<string> Errors { get; set; } = [];

    public Dictionary<string, object> Data { get; set; } = [];
}