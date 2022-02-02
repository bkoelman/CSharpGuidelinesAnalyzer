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
        internal void When_warning_level_is_9999_with_warnings_as_errors_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(9999)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_warning_level_is_below_9999_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(5)
                .CompileWithWarningAsError()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with warning level 9999");
        }

        [Fact]
        internal void When_compiling_with_warnings_not_as_errors_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .CompileAtWarningLevel(9999)
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Build with -warnaserror");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new BuildWithTheHighestWarningLevelAnalyzer();
        }
    }
}
