namespace TestForge.Application.Analysis;

public interface IProjectAnalyzer
{
    Task<ProjectAnalysisResult> AnalyzeAsync(
        string workspacePath,
        CancellationToken cancellationToken = default);
}
