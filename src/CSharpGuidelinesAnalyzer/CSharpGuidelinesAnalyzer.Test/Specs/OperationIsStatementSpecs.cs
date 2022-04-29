#if DEBUG
using CSharpGuidelinesAnalyzer.Rules;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs;

public sealed class OperationIsStatementSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => OperationIsStatementAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_for_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|for|] (int i = 0; i < 10; i++)
                    {
                        [|for|] (int j = 0; j < i; j++)
                        {
                            [|i--;|]
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'for' should be a statement",
            "Operation 'for' should be a statement",
            "Operation 'i--;' should be a statement");
    }

    [Fact]
    internal async Task When_for_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|for|] (int i = 0; i < 10; i++)
                        [|for|] (int j = 0; j < i; j++)
                            [|i =- 1;|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'for' should be a statement",
            "Operation 'for' should be a statement",
            "Operation 'i =- 1;' should be a statement");
    }

    [Fact]
    internal async Task When_foreach_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|foreach|] (var item in new int[0])
                    {
                        [|continue|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'foreach' should be a statement",
            "Operation 'continue' should be a statement");
    }

    [Fact]
    internal async Task When_foreach_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|foreach|] (var item in new int[0])
                        [|System.Console.WriteLine(item);|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'foreach' should be a statement",
            "Operation 'System.Console.WriteLine(item);' should be a statement");
    }

    [Fact]
    internal async Task When_while_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|while|] (System.DateTime.Now.Ticks % 2 != 1)
                    {
                        [|throw|] new System.Exception();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'while' should be a statement",
            "Operation 'throw' should be a statement");
    }

    [Fact]
    internal async Task When_while_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|while|] (System.DateTime.Now.Ticks % 2 != 1)
                        [|throw|] new System.Exception();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'while' should be a statement",
            "Operation 'throw' should be a statement");
    }

    [Fact]
    internal async Task When_if_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|if|] (System.DateTime.Now.Ticks % 2 != 1)
                    {
                        [|while|] (true)
                        {
                        }
                    }
                    else
                    {
                        [|;|]
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'if' should be a statement",
            "Operation 'while' should be a statement",
            "Operation ';' should be a statement");
    }

    [Fact]
    internal async Task When_if_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|if|] (System.DateTime.Now.Ticks % 2 != 1)
                        [|;|]
                    else
                        [|do|]
                        {
                        }
                        while (true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'if' should be a statement",
            "Operation ';' should be a statement",
            "Operation 'do' should be a statement");
    }

    [Fact]
    internal async Task When_try_catch_finally_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|try|]
                    {
                        [|int i = 0;|]
                    }
                    catch (System.Exception ex)
                    {
                        [|int j = 0;|]
                    }
                    finally
                    {
                        [|int k = 0;|]
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'try' should be a statement",
            "Operation 'int i = 0;' should be a statement",
            "Operation 'int j = 0;' should be a statement",
            "Operation 'int k = 0;' should be a statement");
    }

    [Fact]
    internal async Task When_switch_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    [|switch|] (i)
                    {
                        case 1:
                        {
                            [|bool b = i++ > 0 ? true : throw new System.Exception();|]
                            [|break|];
                        }
                        default:
                        {
                            [|throw|] new System.NotSupportedException();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'switch' should be a statement",
            "Operation 'bool b = i++ > 0 ? true : throw new System.Exception();' should be a statement",
            "Operation 'break' should be a statement",
            "Operation 'throw' should be a statement");
    }

    [Fact]
    internal async Task When_switch_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    [|switch|] (i)
                    {
                        case 1:
                            [|break|];
                        case 2:
                            [|goto|] case 1;
                        default:
                            [|throw|] new System.NotSupportedException();
                   }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'switch' should be a statement",
            "Operation 'break' should be a statement",
            "Operation 'goto' should be a statement",
            "Operation 'throw' should be a statement");
    }

    [Fact]
    internal async Task When_lock_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|lock|] (new object())
                    {
                        {
                            [|var value = new object() ?? new object();|]
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'lock' should be a statement",
            "Operation 'var value = new object() ?? new object();' should be a statement");
    }

    [Fact]
    internal async Task When_lock_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|lock|] (new object())
                        [|new object()?.ToString();|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'lock' should be a statement",
            "Operation 'new object()?.ToString();' should be a statement");
    }

    [Fact]
    internal async Task When_label_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    start:
                    {
                        [|goto|] done;
                    }
                    done:
                    {
                        [|goto|] start;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'goto' should be a statement",
            "Operation 'goto' should be a statement");
    }

    [Fact]
    internal async Task When_label_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    start:
                        [|goto|] done;
                    done:
                        [|goto|] start;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'goto' should be a statement",
            "Operation 'goto' should be a statement");
    }

    [Fact]
    internal async Task When_local_function_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void [|N|]()
                    {
                        void [|O|]()
                        {
                            [|int i = 0;|]
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'N' should be a statement",
            "Operation 'O' should be a statement",
            "Operation 'int i = 0;' should be a statement");
    }

    [Fact]
    internal async Task When_local_function_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void [|N|]() => throw new System.Exception();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'N' should be a statement");
    }

    [Fact(Skip = "TODO: Look into nested location reporting")]
    internal async Task When_lambda_expression_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Action action = () [|=>|]
                    {
                        [|int i = 0;|]
                    };
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation '=>' should be a statement",
            "Operation 'int i = 0;' should be a statement");
    }

    [Fact(Skip = "TODO: Look into nested location reporting")]
    internal async Task When_lambda_expression_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Action action = () [|=>|] [|new object();|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation '=>' should be a statement",
            "Operation 'new object();' should be a statement");
    }

    [Fact]
    internal async Task When_using_keyword_with_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|using|] (IDisposable x = null)
                    {
                        [|using|] (IDisposable y = null)
                        {
                            [|string z = null;|]
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'using' should be a statement",
            "Operation 'using' should be a statement",
            "Operation 'string z = null;' should be a statement");
    }

    [Fact]
    internal async Task When_using_keyword_without_block_is_found_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    [|using|] (IDisposable x = null)
                        [|using|] (IDisposable y = null)
                            [|new string[0].ToString();|]
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Operation 'using' should be a statement",
            "Operation 'using' should be a statement",
            "Operation 'new string[0].ToString();' should be a statement");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new OperationIsStatementAnalyzer();
    }
}
#endif
