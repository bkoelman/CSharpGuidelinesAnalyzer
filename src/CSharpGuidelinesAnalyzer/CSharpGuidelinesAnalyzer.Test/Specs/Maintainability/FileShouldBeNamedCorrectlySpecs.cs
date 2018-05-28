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
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InFileNamed("MainForm.cs")
                .AllowingDiagnosticsOutsideSourceTree()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_file_name_starts_with_lowercase_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InFileNamed("mainForm.cs")
                .AllowingDiagnosticsOutsideSourceTree()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "File 'mainForm.cs' should be named using Pascal casing.");
        }

        [Fact]
        internal void When_file_name_contains_underscore_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InFileNamed("Main_Form.cs")
                .AllowingDiagnosticsOutsideSourceTree()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "File 'Main_Form.cs' should be named without underscores.");
        }

        [Fact]
        internal void When_file_name_contains_generic_arity_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InFileNamed("ValueContainer`1.cs")
                .AllowingDiagnosticsOutsideSourceTree()
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
