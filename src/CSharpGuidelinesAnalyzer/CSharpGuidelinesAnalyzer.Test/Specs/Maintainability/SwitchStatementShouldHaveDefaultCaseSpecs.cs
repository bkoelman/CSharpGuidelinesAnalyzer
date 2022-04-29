using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class SwitchStatementShouldHaveDefaultCaseSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => SwitchStatementShouldHaveDefaultCaseAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_switch_statement_type_is_bool_and_contains_a_default_case_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool b)
                {
                    switch (b)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_bool_and_is_exhaustive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool b)
                {
                    switch (b)
                    {
                        case true:
                            return;
                        case false:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_bool_and_is_non_exhaustive_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool b)
                {
                    [|switch|] (b)
                    {
                        case false:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Non-exhaustive switch statement requires a default case clause");
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_bool_and_contains_a_default_case_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool? b)
                {
                    switch (b)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_bool_and_is_exhaustive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool? b)
                {
                    switch (b)
                    {
                        case true:
                        case null:
                            return;
                        case false:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_bool_and_is_non_exhaustive_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool? b)
                {
                    [|switch|] (b)
                    {
                        case true:
                        case false:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Non-exhaustive switch statement requires a default case clause");
    }

    [Fact]
    internal async Task When_switch_statement_type_is_flags_enum_and_is_non_exhaustive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                [Flags]
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    switch (s)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_enum_and_contains_a_default_case_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    switch (s)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_enum_and_is_exhaustive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    switch (s)
                    {
                        case Status.Pending:
                            return;
                        case Status.Active:
                        case Status.Completed:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_enum_and_is_non_exhaustive_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    [|switch|] (s)
                    {
                        case Status.Pending:
                        case Status.Active:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Non-exhaustive switch statement requires a default case clause");
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_enum_and_contains_a_default_case_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status? s)
                {
                    switch (s)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_enum_and_is_exhaustive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status? s)
                {
                    switch (s)
                    {
                        case null:
                        case Status.Pending:
                            return;
                        case Status.Active:
                        case Status.Completed:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_enum_and_is_non_exhaustive_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status? s)
                {
                    [|switch|] (s)
                    {
                        case Status.Pending:
                        case Status.Active:
                        case Status.Completed:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Non-exhaustive switch statement requires a default case clause");
    }

    [Fact]
    internal async Task When_switch_statement_type_is_nullable_byte_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(byte? b)
                {
                    switch (b)
                    {
                        case 0xF0:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_char_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(char c)
                {
                    switch (c)
                    {
                        case 'A':
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_int_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    switch (i)
                    {
                        case 5:
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_string_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(string s)
                {
                    switch (s)
                    {
                        case ""X"":
                            throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_contains_a_non_constant_case_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool b)
                {
                    switch (b)
                    {
                        case true && true:
                            throw new NotImplementedException();
                    }

                    switch (b)
                    {
                        case !true:
                            throw new NotImplementedException();
                    }
               }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_is_invalid_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .AllowingCompileErrors()
            .InDefaultClass(@"
                void M(bool b)
                {
                    switch (b)
                    {
                        case true:
                            throw new NotImplementedException();
                        case true:
                            throw new NotImplementedException();
                    }
               }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_enum_with_guard_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    switch (s)
                    {
                        case Status.Completed when new[] { 1 }.Length > 0:
                        {
                            throw null;
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_switch_statement_type_is_enum_with_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public enum Status { Pending, Active, Completed }

                void M(Status s)
                {
                    switch (s)
                    {
                        case object obj:
                        {
                            throw null;
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new SwitchStatementShouldHaveDefaultCaseAnalyzer();
    }
}
