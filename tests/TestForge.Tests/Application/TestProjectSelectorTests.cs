using TestForge.Application.Analysis;

namespace TestForge.Tests.Application;

public sealed class TestProjectSelectorTests
{
    [Fact]
    public void Select_ReturnsAllDetectedTestProjects_InStableOrder()
    {
        // Arrange
        IReadOnlyList<string> detectedTestProjects =
        [
            "tests/Zeta.Tests/Zeta.Tests.csproj",
            "tests/EvoFit.Tests/EvoFit.Tests.csproj",
            "tests/Alpha.Tests/Alpha.Tests.csproj"
        ];

        // Act
        var result = TestProjectSelector.Select(detectedTestProjects);

        // Assert
        Assert.Equal(
            [
                "tests/Alpha.Tests/Alpha.Tests.csproj",
                "tests/EvoFit.Tests/EvoFit.Tests.csproj",
                "tests/Zeta.Tests/Zeta.Tests.csproj"
            ],
            result);
    }
}
