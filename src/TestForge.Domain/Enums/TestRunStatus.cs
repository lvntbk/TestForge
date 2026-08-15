namespace TestForge.Domain.Enums;

public enum TestRunStatus
{
    Queued = 0,
    Cloning = 1,
    Analyzing = 2,
    Building = 3,
    Testing = 4,
    Completed = 5,
    Failed = 6
}
