using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public sealed class BuildWithTheHighestWarningLevelSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => BuildWithTheHighestWarningLevelAnalyzer.DiagnosticId;

        [Fact]
        internal void When_warning_level_is_set_to_four_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(4)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_warning_level_is_set_to_three_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(3)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with warning level 4.");
        }

        [Fact]
        internal void When_warning_level_is_set_to_two_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(2)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with warning level 4.");
        }

        [Fact]
        internal void When_warning_level_is_set_to_one_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(1)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with warning level 4.");
        }

        // Note: at warning level 0, analyzers do not even run. So a test for that is omitted here.

        [Fact]
        internal void When_warnaserror_is_enabled_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_warnaserror_is_disabled_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with -warnaserror.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new BuildWithTheHighestWarningLevelAnalyzer();
        }
    }
}
