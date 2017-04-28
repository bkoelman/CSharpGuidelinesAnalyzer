using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public sealed class EvaluateQueriesBeforeReturningThemSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => EvaluateQueriesBeforeReturningThemAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_return_type_is_void_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        return;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_return_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    int M()
                    {
                        return 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_return_type_is_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(List<string>).Namespace)
                .InDefaultClass(@"
                    List<string> M()
                    {
                        return new List<string>
                        {
                            ""A""
                        };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_null_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M()
                        {
                            return null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_default_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .Using(typeof(IList<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M()
                        {
                            return default(IList<int>);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable).Namespace)
                .Using(typeof(IList<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M(IList<int> source)
                        {
                            [|return|] source.Where(x => true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_the_result_of_First_after_Where_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IList<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        int M(IList<int> source)
                        {
                            return source.Where(x => true).First();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_variable_that_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = source.Where(x => true);
                            [|return|] result;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_conditionally_returns_variable_that_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source, bool condition)
                        {
                            var result = source.Where(x => true);
                            [|return|] condition ? result : null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        internal void
            When_method_conditionally_returns_variable_that_contains_the_result_of_Where_call_with_redundant_braces_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source, bool condition)
                        {
                            [|return|] (condition ? (source.Where(x => true)) : (null));
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_variable_that_contains_the_result_of_ToList_after_Where_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = source.Where(x => true).ToList();
                            return result;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_variable_that_eventually_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            IEnumerable<int> result = Enumerable.Empty<int>();
                            result = source.Where(x => true);
                            [|return|] result;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        internal void
            When_method_returns_variable_that_eventually_contains_the_result_of_ToArray_after_Where_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = Enumerable.Empty<int>();
                            result = source.Where(x => true);
                            result = result.ToArray();
                            return result;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_variable_that_eventually_contains_the_result_of_Select_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result1 = Enumerable.Empty<int>();
                            result1 = source.Select(x => x);

                            var result2 = result1;

                            var result3 = result2;
                            [|return|] result3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_the_result_of_a_query_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            [|return|]
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a query that uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_the_result_of_a_query_expression_with_redundant_braces_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            [|return|] ((
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item));
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a query that uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_the_result_of_ToArray_after_a_query_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            return (
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item
                                ).ToArray();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_variable_that_contains_the_result_of_a_query_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result =
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item;

                            [|return|] (result);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a query that uses deferred execution.");
        }

        [Fact]
        internal void
            When_method_returns_variable_that_contains_the_result_of_ToList_after_a_query_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = (
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item
                                ).ToList();

                            return result;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_the_result_of_null_conditional_access_operator_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = source.ToArray();

                            [|return|] result?.Select(x => x);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_the_result_of_null_coalescing_operator_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            [|return|] source ?? new int[0].Select(x => x);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_new_array_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M()
                        {
                            return new int[0];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_variable_assignment_to_result_of_Skip_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M()
                        {
                            IEnumerable<int> temp;

                            [|return|] temp = new List<int>
                            {
                                1, 2, 3, 4, 5
                            }.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_parameter_assignment_to_result_of_Skip_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(ref IEnumerable<int> source)
                        {
                            [|return|] source = new List<int>
                            {
                                1, 2, 3, 4, 5
                            }.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(ref IEnumerable<int>)' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_field_assignment_to_result_of_Skip_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private IEnumerable<int> f;

                        IEnumerable<int> M()
                        {
                            [|return|] f = new List<int>
                            {
                                1, 2, 3, 4, 5
                            }.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_property_assignment_to_result_of_Skip_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private IEnumerable<int> P { get; set; }

                        IEnumerable<int> M()
                        {
                            [|return|] P = new List<int>
                            {
                                1, 2, 3, 4, 5
                            }.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_returns_cast_to_result_of_Skip_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IEnumerable<int> source)
                        {
                            [|return|] (IList<int>)source.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IEnumerable<int>)' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_yield_returns_a_value_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M()
                        {
                            yield return 5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_yield_returns_a_deferred_sequence_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<IEnumerable<int>> M(IEnumerable<int> source)
                        {
                            [|yield return|] source.Skip(2);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IEnumerable<int>)' returns the result of a call to 'Skip', which uses deferred execution.");
        }

        [Fact]
        internal void When_method_contains_multiple_returns_it_should_profit_from_cache_hits()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source, bool flag)
                        {
                            var result1 = Enumerable.Empty<int>();
                            result1 = source.Select(x => x);

                            var result2 = result1;

                            var result3 = result2;

                            if (flag)
                            {
                                [|return|] result2;
                            }
                            else
                            {
                                [|return|] result3;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Select', which uses deferred execution.",
                "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Select', which uses deferred execution.");
        }

        [Fact]
        internal void When_enum_defines_field_with_explicit_value_it_must_not_crash()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum E
                    {
                        A = 1
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_event_declaration_initializes_with_empty_delegate_it_must_not_crash()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler SomethingChanged = delegate { };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_iterator_breaks_it_must_not_crash()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InDefaultClass(@"
                    class C
                    {
                        private IEnumerable M()
                        {
                            yield break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new EvaluateQueriesBeforeReturningThemAnalyzer();
        }
    }
}
