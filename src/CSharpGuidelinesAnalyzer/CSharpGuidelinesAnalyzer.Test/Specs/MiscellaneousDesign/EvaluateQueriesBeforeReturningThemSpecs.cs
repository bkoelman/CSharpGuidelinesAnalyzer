using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpGuidelinesAnalyzer.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public class EvaluateQueriesBeforeReturningThemSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => EvaluateQueriesBeforeReturningThemAnalyzer.DiagnosticId;

        [Fact]
        public void When_method_returns_void_it_must_be_skipped()
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
        public void When_method_returns_int_it_must_be_skipped()
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
        public void When_method_returns_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M(IList<int> source)
                        {
                            [|return source.Where(x => true);|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        public void When_method_returns_the_result_of_First_after_Where_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
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
        public void When_method_returns_variable_that_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result = source.Where(x => true);
                            [|return result;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        public void
            When_method_conditionally_returns_variable_that_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source, bool condition)
                        {
                            var result = source.Where(x => true);
                            [|return condition ? result : null;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        public void When_method_returns_variable_that_contains_the_result_of_ToList_after_Where_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
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
        public void When_method_returns_variable_that_eventually_contains_the_result_of_Where_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            IEnumerable<int> result = Enumerable.Empty<int>();
                            result = source.Where(x => true);
                            [|return result;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution.");
        }

        [Fact]
        public void
            When_method_returns_variable_that_eventually_contains_the_result_of_ToArray_after_Where_call_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
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
        public void When_method_returns_variable_that_eventually_contains_the_result_of_Select_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result1 = Enumerable.Empty<int>();
                            result1 = source.Select(x => x);

                            var result2 = result1;

                            var result3 = result2;
                            [|return result3;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution.");
        }

        [Fact]
        public void When_method_returns_the_result_of_a_query_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            [|return
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a query that uses deferred execution.");
        }

        [Fact]
        public void When_method_returns_the_result_of_ToArray_after_a_query_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
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
        public void When_method_returns_variable_that_contains_the_result_of_a_query_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable<int> M(IList<int> source)
                        {
                            var result =
                                from item in Enumerable.Empty<int>()
                                where item != 2
                                select item;

                            [|return result;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'C.M(IList<int>)' returns the result of a query that uses deferred execution.");
        }

        [Fact]
        public void
            When_method_returns_variable_that_contains_the_result_of_ToList_after_a_query_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (Enumerable).Assembly)
                .Using(typeof (Enumerable).Namespace)
                .Using(typeof (IEnumerable<>).Namespace)
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

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new EvaluateQueriesBeforeReturningThemAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}