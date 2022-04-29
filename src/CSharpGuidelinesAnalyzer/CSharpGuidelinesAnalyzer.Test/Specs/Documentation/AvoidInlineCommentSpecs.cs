using System.Diagnostics;
using CSharpGuidelinesAnalyzer.Rules.Documentation;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Documentation;

public sealed class AvoidInlineCommentSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidInlineCommentAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_body_contains_single_line_comment_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|// Example|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Code block should not contain inline comment");
    }

    [Fact]
    internal async Task When_method_body_contains_multi_line_comment_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|/* Example
                    block
                    of text*/|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Code block should not contain inline comment");
    }

    [Fact]
    internal async Task When_method_contains_documentation_comment_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                /// <summary>...</summary>
                void M()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_preprocessor_directive_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
#if DEBUG
                    Console.WriteLine(""Debug mode"");
#else
                    Console.WriteLine(""Release mode"");
#endif
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_region_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    #region Example
                    int i = 4;
                    #endregion
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_pragma_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
#pragma warning disable CS0219
                    int i = 4;
#pragma warning restore
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_field_initializer_contains_single_line_comment_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int F = Int32
                    [|// comment|]
                    .Parse(""1"");
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Code block should not contain inline comment");
    }

    [Fact]
    internal async Task When_property_getter_contains_multiple_comments_they_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public string P
                {
                    get
                    {
                        [|// comment |]

                        return null;

                        [|/* unreachable */|]
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Code block should not contain inline comment",
            "Code block should not contain inline comment");
    }

    [Fact]
    internal async Task When_method_contains_leading_comment_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                // some
                void M()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_trailing_comment_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                }
                // some
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_Resharper_inspections_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    string s = null;

                    // ReSharper disable PossibleNullReferenceException
                    if (s.Length > 1)
                    // ReSharper restore PossibleNullReferenceException
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_Resharper_language_injection_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    /*language=regexp|jsregexp*/
                    string regex = @""^[A-Z]+$"";
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_Resharper_formatter_configurations_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    // @formatter:max_line_length 50
                    string text = string.Empty;
                    // @formatter:max_line_length restore

                    // @formatter:off
                    int i = 1;
                    // @formatter:on
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_Arrange_Act_Assert_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Debug).Namespace)
            .InDefaultClass(@"
                void UnitTest()
                {
                    // Arrange
                    int x = 10;

                    // Act
                    x -= 1;

                    // Assert
                    Debug.Assert(x == 9);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_body_contains_simplified_Arrange_Act_Assert_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Debug).Namespace)
            .InDefaultClass(@"
                void UnitTest()
                {
                    // Arrange
                    int x = 10;

                    // Act and assert
                    Debug.Assert(--x == 9);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_else_clause_contains_only_comment_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    if (i > 10)
                    {
                        Console.WriteLine('>');
                    }
                    else if (i < 8)
                    {
                        Console.WriteLine('<');
                    }
                    else
                    {
                        // No action required.
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidInlineCommentAnalyzer();
    }
}
