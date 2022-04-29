using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class IfElseIfConstructShouldFinishWithElseClauseSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => IfElseIfConstructShouldFinishWithElseClauseAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_an_if_statement_has_no_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool condition)
                {
                    if (condition)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_an_if_statement_has_an_unconditional_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool condition)
                {
                    if (condition)
                    {
                    }
                    else
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_an_if_else_if_statement_does_not_end_with_an_unconditional_else_clause_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second)
                {
                    [|if|] (first)
                    {
                    }
                    else if (second)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "If-else-if construct should end with an unconditional else clause");
    }

    [Fact]
    internal async Task When_an_if_else_if_statement_ends_with_an_unconditional_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second)
                {
                    if (first)
                    {
                    }
                    else if (second)
                    {
                    }
                    else
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_an_if_else_if_else_if_statement_does_not_end_with_an_unconditional_else_clause_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second, bool third)
                {
                    [|if|] (first)
                    {
                    }
                    else if (second)
                    {
                    }
                    else if (third)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "If-else-if construct should end with an unconditional else clause");
    }

    [Fact]
    internal async Task When_an_if_else_if_else_if_statement_ends_with_an_unconditional_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second, bool third)
                {
                    if (first)
                    {
                    }
                    else if (second)
                    {
                    }
                    else if (third)
                    {
                    }
                    else
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_an_if_else_if_else_if_else_if_statement_does_not_end_with_an_unconditional_else_clause_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second, bool third, bool fourth)
                {
                    [|if|] (first)
                    {
                    }
                    else if (second)
                    {
                    }
                    else if (third)
                    {
                    }
                    else if (fourth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "If-else-if construct should end with an unconditional else clause");
    }

    [Fact]
    internal async Task When_an_if_else_if_else_if_else_if_statement_ends_with_an_unconditional_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second, bool third, bool fourth)
                {
                    if (first)
                    {
                    }
                    else if (second)
                    {
                    }
                    else if (third)
                    {
                    }
                    else if (fourth)
                    {
                    }
                    else
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_nested_if_else_if_statements_do_not_end_with_an_unconditional_else_clause_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second)
                {
                    [|if|] (first)
                    {
                    }
                    else if (second)
                    {
                        [|if|] (first && second)
                        {
                            [|if|] (second)
                            {
                            }
                            else if (false)
                            {
                            }
                        }
                        else if (true)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "If-else-if construct should end with an unconditional else clause",
            "If-else-if construct should end with an unconditional else clause",
            "If-else-if construct should end with an unconditional else clause");
    }

    [Fact]
    internal async Task When_nested_if_else_if_statements_end_with_an_unconditional_else_clause_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool first, bool second)
                {
                    if (first)
                    {
                    }
                    else if (second)
                    {
                        if (first && second)
                        {
                            if (second)
                            {
                            }
                            else if (false)
                            {
                            }
                            else
                            {
                            }
                        }
                        else if (true)
                        {
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new IfElseIfConstructShouldFinishWithElseClauseAnalyzer();
    }
}
