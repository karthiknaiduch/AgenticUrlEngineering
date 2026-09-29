using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Orchestration;

public interface IDecisionRecorder
{
    void Record(
        Guid workflowId,
        string decisionText,
        string reason,
        IEnumerable<string> evidence,
        IEnumerable<string> alternatives,
        string selectedOption,
        string actor);
}