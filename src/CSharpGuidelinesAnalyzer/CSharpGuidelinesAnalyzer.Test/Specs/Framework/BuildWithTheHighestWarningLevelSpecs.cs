using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework;

public sealed class BuildWithTheHighestWarningLevelSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => BuildWithTheHighestWarningLevelAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_compiling_with_warnings_as_errors_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .CompileWithWarningAsError()
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_compiling_with_warnings_not_as_errors_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Pass -warnaserror to the compiler or add <TreatWarningsAsErrors>True</TreatWarningsAsErrors> to your project file");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new BuildWithTheHighestWarningLevelAnalyzer();
    }
}
