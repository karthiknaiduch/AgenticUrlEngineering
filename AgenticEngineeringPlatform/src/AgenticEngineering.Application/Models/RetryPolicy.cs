namespace AgenticEngineering.Application.Models;

public class RetryPolicy
{
    public int MaxRetries { get; init; } = 3;

    public TimeSpan Delay { get; init; } =
        TimeSpan.FromSeconds(1);
}