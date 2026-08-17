namespace TestForge.Application.Analysis;

public static class TestProjectSelector
{
    public static IReadOnlyList<string> Select(
        IReadOnlyList<string> detectedTestProjects)
    {
        ArgumentNullException.ThrowIfNull(detectedTestProjects);

        return detectedTestProjects
            .OrderBy(
                projectPath => projectPath,
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
