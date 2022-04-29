#if DEBUG
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs
{
    public sealed class OperationHasKeywordSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => OperationHasKeywordAnalyzer.DiagnosticId;

        [Fact]
        internal async Task When_empty_statement_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|;|]
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_while_loop_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|while|] (System.DateTime.Now.Ticks % 2 != 1)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_do_while_loop_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|do|]
                        {
                        }
                        while (System.DateTime.Now.Ticks % 2 != 1);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_do_while_loop_with_alternate_location_marker_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void ReportAtAlternateLocation()
                    {
                        do
                        {
                        }
                        [|while|] (System.DateTime.Now.Ticks % 2 != 1);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_for_loop_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|for|] (int i = 0; i < 10; i++)
                        {
                            [|break|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword",
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_foreach_loop_is_found_it_must_be_reported()
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
                "Operation should have a keyword",
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_return_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|return|];
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_yield_return_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InDefaultClass(@"
                    IEnumerable<int> M()
                    {
                        [|yield return|] 1;
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_yield_break_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InDefaultClass(@"
                    IEnumerable<int> M()
                    {
                        [|yield break|];
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_goto_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|goto|] Finish;

                        Finish:
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_if_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|if|] (System.DateTime.Now.Ticks % 2 != 1)
                        {
                        }
                        else
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_using_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|using|] (IDisposable x = null)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_lock_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|lock|] (new object())
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_constant_switch_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int i)
                    {
                        [|switch|] (i)
                        {
                            [|case|] 1:
                            [|case|] 2:
                            {
                                [|break|];
                            }
                            [|case|] 3:
                            {
                                [|goto|] case 1;
                            }
                            [|default|]:
                            {
                                [|throw|] null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_pattern_switch_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(object o)
                    {
                        [|switch|] (o)
                        {
                            [|case|] string s:
                            {
                                [|break|];
                            }
                            [|case|] int i when i > 5:
                            {
                                [|return|];
                            }
                            [|default|]:
                            {
                                [|throw|] null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword",
                "Operation should have a keyword");
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
                        }
                        [|catch|] (System.Exception ex)
                        {
                        }
                        finally
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword",
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_try_catch_finally_with_alternate_location_marker_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void ReportAtAlternateLocation()
                    {
                        try
                        {
                        }
                        [|catch|] (System.Exception ex)
                        {
                        }
                        [|finally|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword",
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_throw_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|throw|] new Exception(string.Empty);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_await_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InDefaultClass(@"
                    async Task M()
                    {
                        [|await|] Task.Yield();
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_sizeof_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InDefaultClass(@"
                    async Task M()
                    {
                        int size = [|sizeof|](int);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_typeof_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InDefaultClass(@"
                    async Task M()
                    {
                        var type = [|typeof|](string);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        [Fact]
        internal async Task When_nameof_is_found_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InDefaultClass(@"
                    async Task M()
                    {
                        var name = [|nameof|](M);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Operation should have a keyword");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new OperationHasKeywordAnalyzer();
        }
    }
}
#endif
