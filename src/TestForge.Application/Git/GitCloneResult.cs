namespace TestForge.Application.Git;

public sealed record GitCloneResult(
    bool IsSuccessful,
    string WorkspacePath,
    string StandardOutput,
    string StandardError);
