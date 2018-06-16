using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class FileShouldBeNamedCorrectlySpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => FileShouldBeNamedCorrectlyAnalyzer.DiagnosticId;

        [Fact]
        internal void When_file_name_matches_rules_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new EmptySourceCodeBuilder()
                .InFileNamed("MainForm.cs")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_file_name_starts_with_lowercase_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new EmptySourceCodeBuilder()
                .InFileNamed("mainForm.cs")
                .WithCode("[||]")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "File 'mainForm.cs' should be named using Pascal casing.");
        }

        [Fact]
        internal void When_file_name_contains_underscore_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new EmptySourceCodeBuilder()
                .InFileNamed("Main_Form.cs")
                .WithCode("[||]")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "File 'Main_Form.cs' should be named without underscores.");
        }

        [Fact]
        internal void When_file_name_contains_generic_arity_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new EmptySourceCodeBuilder()
                .InFileNamed("ValueContainer`1.cs")
                .WithCode("[||]")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "File 'ValueContainer`1.cs' should be named without generic arity.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new FileShouldBeNamedCorrectlyAnalyzer();
        }
    }
}
