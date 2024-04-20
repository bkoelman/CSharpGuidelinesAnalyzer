using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public partial class AvoidMemberWithManyStatementsSpecs
{
    [Fact]
    internal async Task When_method_contains_eight_statements_with_local_function_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void [|M|]()
                    {
                        void L()
                        {
                            ; ;
                            ; ;
                        }

                        ; ;
                        ; ;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_method_contains_seven_statements_with_local_function_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M()
                    {
                        void L()
                        {
                            ; ;
                            ; ;
                        }

                        ; ;
                        ;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_eight_statements_with_lambda_block_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void [|M|]()
                    {
                        ; ;
                        ; ;

                        Action<int> action = i =>
                        {
                            ; ;
                            ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_method_contains_seven_statements_with_lambda_block_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M()
                    {
                        ; ;
                        ; ;

                        Action<int> action = i =>
                        {
                            ; ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_eight_statements_with_parenthesized_lambda_block_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void [|M|]()
                    {
                        ; ;
                        ; ;

                        Action action = () =>
                        {
                            ; ;
                            ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_method_contains_seven_statements_with_parenthesized_lambda_block_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M()
                    {
                        ; ;
                        ; ;

                        Action action = () =>
                        {
                            ; ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_eight_statements_with_anonymous_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void [|M|]()
                    {
                        ; ;
                        ; ;

                        Action action = delegate
                        {
                            ; ;
                            ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_method_contains_seven_statements_with_anonymous_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M()
                    {
                        ; ;
                        ; ;

                        Action action = delegate
                        {
                            ; ;
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }
}
