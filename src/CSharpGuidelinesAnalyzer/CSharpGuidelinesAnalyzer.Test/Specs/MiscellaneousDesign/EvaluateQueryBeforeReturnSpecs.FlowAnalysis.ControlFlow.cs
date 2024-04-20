using System.Collections;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_returns_the_result_of_conditional_operator_after_Where_invocation_on_left_side_it_must_be_reported()
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
                        var result = source.Where(x => true);
                        [|return|] condition ? result : source;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_conditional_operator_after_Where_invocation_on_right_side_it_must_be_reported()
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
                        var result = source.Where(x => true);
                        [|return|] condition ? source : result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_conditional_operator_after_Where_invocation_with_redundant_parentheses_it_must_be_reported()
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
                        [|return|] (condition ? (source.Where(x => true)) : (null));
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_conditional_operator_after_ToArray_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<int> M(IEnumerable<int> source, bool condition)
                    {
                        var result = source.Where(x => true);
                        return condition ? result.ToArray() : source.ToArray();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_null_conditional_access_operator_after_Select_invocation_it_must_be_reported()
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
                        int[] result = source.ToArray();
                        [|return|] result?.Select(x => x);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_null_conditional_access_operator_after_ToList_invocation_it_must_be_skipped()
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
                        IEnumerable<int> result = source.Select(x => x);
                        return result?.ToList();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_null_coalescing_operator_after_Select_on_left_side_it_must_be_reported()
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
                        IEnumerable<int> result = source.Select(x => x);
                        [|return|] result ?? new List<int>();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_null_coalescing_operator_after_Select_on_right_side_it_must_be_reported()
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
                        [|return|] source ?? new int[0].Select(x => x);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_null_coalescing_operator_after_ToArray_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<int> M(List<int> source)
                    {
                        IEnumerable<int> result = source.ToArray();
                        return result ?? new List<int>();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_an_array_creation_with_initializer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        return new[]
                        {
                            source.Where(x => true)
                        }.AsEnumerable();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_Where_invocation_after_array_creation_with_initializer_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        IEnumerable result = new[] { 1, 2, 3 }.Where(x => true);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_an_anonymous_object_creation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IEnumerable<int> source)
                    {
                        var result = new
                        {
                            S = source.Skip(1)
                        };
                
                        return result.S;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_an_object_creation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        IEnumerable<int> result = new X(source.Where(x => true));
                        return result;
                    }
                
                    class X : IEnumerable<int>
                    {
                        public X(IEnumerable<int> sequence) => throw null;
                
                        public IEnumerator<int> GetEnumerator() => throw null;
                        IEnumerator IEnumerable.GetEnumerator() => throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_an_object_creation_with_dynamic_constructor_argument_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        IEnumerable<int> result = new X((dynamic)(source.Where(x => true)));
                        return result;
                    }
                
                    class X : IEnumerable<int>
                    {
                        public X(dynamic sequence) => throw null;
                
                        public IEnumerator<int> GetEnumerator() => throw null;
                        IEnumerator IEnumerable.GetEnumerator() => throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_an_object_creation_with_initializer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        IEnumerable<int> result = new X
                        {
                            Sequence = source.Where(x => true)
                        };
                        return result;
                    }
                
                    class X : IEnumerable<int>
                    {
                        public IEnumerable<int> Sequence;
                
                        public IEnumerator<int> GetEnumerator() => throw null;
                        IEnumerator IEnumerable.GetEnumerator() => throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_a_collection_creation_with_initializer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        return new List<IEnumerable<int>>
                        {
                            source.Where(x => true)
                        }.AsEnumerable();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_an_array_element_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<IEnumerable<int>> source)
                    {
                        IEnumerable<int>[] array = source.Where(x => true).ToArray();
                        return array[0];
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_invocation_of_local_variable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<IEnumerable<int>> source)
                    {
                        Func<IEnumerable<IEnumerable<int>>> f = () => source.Where(x => true);
                        return f();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_default_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable<int> result = default;
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_dynamic_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        dynamic d = GetValue();
                        IEnumerable<int> result = d.Skip(1);
                
                        return result;
                    }
                
                    dynamic GetValue() => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_nameof_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable result = nameof(Enumerable.SkipWhile);
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_constant_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable result = "";
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_throw_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        IEnumerable result = source ?? throw new Exception(Enumerable.Empty<int>().Skip(1).ToString());
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }
}
