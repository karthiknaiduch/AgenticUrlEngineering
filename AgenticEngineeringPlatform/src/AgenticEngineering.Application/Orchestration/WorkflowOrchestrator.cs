using AgenticEngineering.Application.Models;

namespace AgenticEngineering.Application.Orchestration;

public class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly WorkflowExecutor _workflowExecutor;
    private readonly IWorkflowPlanner _workflowPlanner;
    private readonly IWorkflowStore _workflowStore;
    private readonly AgentContextAccessor _contextAccessor;

    public WorkflowOrchestrator(
    WorkflowExecutor workflowExecutor,
    IWorkflowPlanner workflowPlanner,
    IWorkflowStore workflowStore,
    AgentContextAccessor contextAccessor)
{
    _workflowExecutor = workflowExecutor;
    _workflowPlanner = workflowPlanner;
    _workflowStore = workflowStore;
    _contextAccessor = contextAccessor;
}

    public async Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        
        var workflow = _workflowPlanner.CreateWorkflow(context);

        await _workflowStore.SaveAsync(
            workflow,
            context,
            cancellationToken);

        var result = await _workflowExecutor.ExecuteAsync(
            workflow,
            context,
            cancellationToken);

        await _workflowStore.SaveAsync(
            workflow,
            context,
            cancellationToken);

        return result;
    }

    public async Task<AgentResult> ResumeAsync(
    Guid workflowId,
    string approvedBy,
    string reason,
    CancellationToken cancellationToken)
{
    var stored = await _workflowStore.GetAsync(
        workflowId,
        cancellationToken);

    if (stored == null)
    {
        return new AgentResult
        {
            Status = AgentExecutionStatus.Failed,
            Message = "Workflow was not found.",
            Errors =
            [
                $"Workflow '{workflowId}' does not exist."
            ]
        };
    }

    var workflow = stored.Value.Workflow;
    var context = stored.Value.Context;

    _contextAccessor.Context = context; 

    var waitingNodes = workflow.Nodes
        .Where(x =>
            x.Status == Domain.Enums.NodeStatus.WaitingForApproval)
        .ToList();

    if (waitingNodes.Count == 0)
    {
        return new AgentResult
        {
            Status = AgentExecutionStatus.Failed,
            Message = "Workflow has no pending approval.",
            Errors =
            [
                "No workflow node is waiting for approval."
            ]
        };
    }

    if (string.IsNullOrWhiteSpace(approvedBy))
    {
        return new AgentResult
        {
            Status = AgentExecutionStatus.Failed,
            FailureType = Domain.Enums.FailureType.Validation,
            Message = "Approver identity is required."
        };
    }

    if (string.IsNullOrWhiteSpace(reason))
    {
        return new AgentResult
        {
            Status = AgentExecutionStatus.Failed,
            FailureType = Domain.Enums.FailureType.Validation,
            Message = "Approval reason is required."
        };
    }

    foreach (var node in waitingNodes)
    {
        var approval = new Domain.Models.Approval
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            NodeId = node.Id,
            RequestedBy = "WorkflowOrchestrator",
            ApprovedBy = approvedBy,
            Status = Domain.Enums.ApprovalStatus.Approved,
            Reason = reason,
            RequestedAtUtc = DateTime.UtcNow,
            DecisionAtUtc = DateTime.UtcNow
        };

        context.ApprovalRecords.Add(approval);

        context.Approvals.Add(
            $"Approved by {approvedBy}: {reason}");

        node.ApprovalId = approval.Id;

        node.ApprovalStatus =
            Domain.Enums.ApprovalStatus.Approved;

        node.Status =
            Domain.Enums.NodeStatus.Pending;
    }

    workflow.Status =
        Domain.Enums.WorkflowStatus.Running;

    await _workflowStore.SaveAsync(
        workflow,
        context,
        cancellationToken);

    var result = await _workflowExecutor.ExecuteAsync(
        workflow,
        context,
        cancellationToken);

    await _workflowStore.SaveAsync(
        workflow,
        context,
        cancellationToken);

    return result;
}
}