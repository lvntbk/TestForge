using TestForge.Application.Analysis;

namespace TestForge.Infrastructure.Analysis;

public sealed class DotNetProjectAnalyzer : IProjectAnalyzer
{
    public async Task<ProjectAnalysisResult> AnalyzeAsync(
        string workspacePath,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(workspacePath))
        {
            throw new DirectoryNotFoundException(
                $"Workspace bulunamadı: {workspacePath}");
        }

        var solutionPaths = FindFiles(workspacePath, "*.sln");

        var projectPaths = FindFiles(workspacePath, "*.csproj");

        var webProjectPaths = new List<string>();
        var testProjectPaths = new List<string>();

        foreach (var projectPath in projectPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = Path.Combine(
                workspacePath,
                projectPath);

            var content = await File.ReadAllTextAsync(
                fullPath,
                cancellationToken);

            if (content.Contains(
                    "Microsoft.NET.Sdk.Web",
                    StringComparison.OrdinalIgnoreCase))
            {
                webProjectPaths.Add(projectPath);
            }

            if (IsTestProject(projectPath, content))
            {
                testProjectPaths.Add(projectPath);
            }
        }

        return new ProjectAnalysisResult(
            solutionPaths,
            projectPaths,
            webProjectPaths,
            testProjectPaths);
    }

    private static List<string> FindFiles(
        string workspacePath,
        string searchPattern)
    {
        return Directory
            .EnumerateFiles(
                workspacePath,
                searchPattern,
                SearchOption.AllDirectories)
            .Where(path => !IsIgnoredPath(workspacePath, path))
            .Select(path => Path.GetRelativePath(workspacePath, path))
            .OrderBy(path => path)
            .ToList();
    }

    private static bool IsIgnoredPath(
        string workspacePath,
        string path)
    {
        var relativePath = Path.GetRelativePath(
            workspacePath,
            path);

        var segments = relativePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        return segments.Any(segment =>
            segment.Equals(
                ".git",
                StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(
                "bin",
                StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(
                "obj",
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTestProject(
        string projectPath,
        string content)
    {
        var fileName = Path.GetFileNameWithoutExtension(projectPath);

        return fileName.Contains(
                   "Test",
                   StringComparison.OrdinalIgnoreCase) ||
               content.Contains(
                   "Microsoft.NET.Test.Sdk",
                   StringComparison.OrdinalIgnoreCase) ||
               content.Contains(
                   "xunit",
                   StringComparison.OrdinalIgnoreCase) ||
               content.Contains(
                   "NUnit",
                   StringComparison.OrdinalIgnoreCase) ||
               content.Contains(
                   "MSTest",
                   StringComparison.OrdinalIgnoreCase);
    }
}
