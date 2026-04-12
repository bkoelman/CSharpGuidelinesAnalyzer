using System.Collections;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_returns_variable_that_contains_the_result_of_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        var result = source.Where(x => true);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_variable_that_contains_the_result_of_ToList_after_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = source.Where(x => true).ToList();
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_variable_that_eventually_contains_the_result_of_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = Enumerable.Empty<int>();
                        result = source.Where(x => true);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_variable_that_eventually_contains_the_result_of_ToArray_after_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
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
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_variable_that_eventually_contains_the_result_of_Select_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        var result1 = Enumerable.Empty<int>();
                        result1 = source.Select(x => x);

                        var result2 = result1;

                        var result3 = result2;
                        [|return|] result3;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_variable_that_contains_the_result_of_a_query_expression_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        var result =
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            select item;

                        [|return|] (result);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a query, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_variable_that_contains_the_result_of_ToList_after_a_query_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = (
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            select item
                            ).ToList();

                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_variable_that_is_overwritten_by_deconstruction_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source, int p)
                    {
                        IEnumerable result = source.Where(x => true);
                        (result, p) = this;
                        return result;
                    }

                    void Deconstruct(out IEnumerable<int> i, out int j) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_local_that_is_reassigned_after_copy_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        var result1 = source.Select(x => x);
                        var result2 = result1;

                        result1 = result2.ToArray(); // should not impact return value

                        [|return|] result2;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_local_that_is_conditionally_reassigned_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IList<int> source, bool condition)
                    {
                        var result1 = source.Select(x => x);
                        var result2 = result1;

                        IEnumerable<int> result3 = null;

                        if (condition)
                        {
                            result3 = result2; // should scan right through, we're not perfect
                        }

                        [|return|] result3;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Select', which uses deferred execution");
    }
}
