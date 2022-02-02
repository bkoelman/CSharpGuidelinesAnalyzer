using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class ClauseInSwitchStatementShouldHaveBlockSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ClauseInSwitchStatementShouldHaveBlockAnalyzer.DiagnosticId;

        [Fact]
        internal void When_case_clause_contains_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            case ""A"":
                            case ""B"":
                            {
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_pattern_case_clause_contains_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            case var v:
                            {
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_default_case_clause_contains_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            default:
                            {
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_case_clause_contains_no_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            case ""A"":
                            [|case|] ""B"":
                                break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing block in case or default clause of switch statement");
        }

        [Fact]
        internal void When_pattern_case_clause_contains_no_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            [|case|] var v:
                                break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing block in case or default clause of switch statement");
        }

        [Fact]
        internal void When_default_case_clause_contains_no_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            [|default|]:
                                break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing block in case or default clause of switch statement");
        }

        [Fact]
        internal void When_case_clause_is_invalid_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .AllowingCompileErrors()
                .InDefaultClass(@"
                    void M(string s)
                    {
                        switch (s)
                        {
                            case ""A"":
                            case ""B"":
                                #;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ClauseInSwitchStatementShouldHaveBlockAnalyzer();
        }
    }
}
