using AgenticEngineering.Api.Models;
using AgenticEngineering.Application.Models;
using AgenticEngineering.Application.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace AgenticEngineering.Api.Controllers;

[ApiController]
[Route("api/v1/workflows")]
public class ApprovalsController : ControllerBase
{
    private readonly IWorkflowOrchestrator _orchestrator;

    public ApprovalsController(
        IWorkflowOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("{workflowId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid workflowId,
        [FromBody] ApprovalRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApprovedBy))
        {
            return BadRequest(new
            {
                message = "ApprovedBy is required."
            });
        }

        var result = await _orchestrator.ResumeAsync(
    workflowId,
    request.ApprovedBy,
    request.Reason,
    cancellationToken);

        return result.Status switch
        {
            AgentExecutionStatus.Succeeded =>
                Ok(new
                {
                    workflowId,
                    status = result.Status.ToString(),
                    message = result.Message
                }),

            AgentExecutionStatus.WaitingForApproval =>
                StatusCode(StatusCodes.Status202Accepted, new
                {
                    workflowId,
                    status = result.Status.ToString(),
                    message = result.Message
                }),

            _ =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        workflowId,
                        status = result.Status.ToString(),
                        message = result.Message,
                        errors = result.Errors
                    })
        };
    }
}