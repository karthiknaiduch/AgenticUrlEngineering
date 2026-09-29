using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Orchestration;

public class DecisionRecorder : IDecisionRecorder
{
    private readonly AgentContextAccessor _contextAccessor;

    public DecisionRecorder(
        AgentContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Record(
        Guid workflowId,
        string decisionText,
        string reason,
        IEnumerable<string> evidence,
        IEnumerable<string> alternatives,
        string selectedOption,
        string actor)
    {
        var context = _contextAccessor.Context;

        if (context == null)
        {
            return;
        }

        context.Decisions.Add(new Decision
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            DecisionText = decisionText,
            Reason = reason,
            Evidence = evidence.ToList(),
            Alternatives = alternatives.ToList(),
            SelectedOption = selectedOption,
            Actor = actor,
            TimestampUtc = DateTime.UtcNow
        });
    }
}