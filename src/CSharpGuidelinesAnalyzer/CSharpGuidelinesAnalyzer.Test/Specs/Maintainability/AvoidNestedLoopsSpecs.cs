using System.Linq;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidNestedLoopsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidNestedLoopsAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_contains_single_for_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_for_loop_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            [|for|] (int j = 0; j < 10; j++)
                            {
                                [|for|] (int k = 0; k < 10; k++)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Loop statement contains nested loop",
                "Loop statement contains nested loop");
        }

        [Fact]
        internal void When_method_contains_single_foreach_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        foreach (var outer in new int[0])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_foreach_loop_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        foreach (var outer in new int[0])
                        {
                            [|foreach|] (var inner in new int[0])
                            {
                                [|foreach|] (var nestedInner in new int[0])
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Loop statement contains nested loop",
                "Loop statement contains nested loop");
        }

        [Fact]
        internal void When_method_contains_single_deconstructing_foreach_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        foreach (var (outer1, outer2) in Enumerable.Empty<(int, int)>())
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_deconstructing_foreach_loop_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        foreach (var (outer1, outer2) in Enumerable.Empty<(int, int)>())
                        {
                            [|foreach|] (var (inner1, inner2) in Enumerable.Empty<(int, int)>())
                            {
                                [|foreach|] (var (nestedInner1, nestedInner2) in Enumerable.Empty<(int, int)>())
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Loop statement contains nested loop",
                "Loop statement contains nested loop");
        }

        [Fact]
        internal void When_method_contains_single_while_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        while (true)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_while_loop_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        while (true)
                        {
                            [|while|] (true)
                            {
                                [|while|] (true)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Loop statement contains nested loop",
                "Loop statement contains nested loop");
        }

        [Fact]
        internal void When_method_contains_single_do_while_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        do
                        {
                        }
                        while (true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_do_while_loop_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        do
                        {
                            [|do|]
                            {
                                [|do|]
                                {
                                }
                                while (true);
                            }
                            while (true);
                        }
                        while (true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Loop statement contains nested loop",
                "Loop statement contains nested loop");
        }

        [Fact]
        internal void When_method_contains_nested_loop_in_local_function_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            void Local()
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_loop_in_lambda_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Action<int> action = x =>
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                }
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_loop_in_parenthesized_lambda_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Action action = () =>
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                }
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_nested_loop_in_anonymous_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Action action = delegate
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                }
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidNestedLoopsAnalyzer();
        }
    }
}
