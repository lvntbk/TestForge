namespace TestForge.Application.Analysis;

public sealed record ProjectAnalysisResult(
    IReadOnlyList<string> SolutionPaths,
    IReadOnlyList<string> ProjectPaths,
    IReadOnlyList<string> WebProjectPaths,
    IReadOnlyList<string> TestProjectPaths)
{
    public bool IsSupported => WebProjectPaths.Count > 0;
}
