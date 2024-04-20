using System.Collections;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_returns_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        return null;
                    }

                    IEnumerable<int> N()
                    {
                        return null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable<string>> M()
                    {
                        yield break;
                        yield return null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_default_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M()
                    {
                        return default(IEnumerable);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_default_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable> M()
                    {
                        yield return default(IEnumerable);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_untyped_default_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<T> M<T>()
                    {
                        return default;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_untyped_default_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable<T>> M<T>()
                    {
                        yield return default;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_List_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<int> M()
                    {
                        return new List<int>();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_List_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable<int>> M()
                    {
                        yield return new List<int>();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_array_after_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        return source.Where(x => true).ToArray();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_array_after_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable> M(IList<int> source)
                    {
                        yield return source.Where(x => true).ToArray();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_array_after_a_query_expression_it_must_be_skipped()
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
                        return (
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            select item
                            ).ToArray();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_array_after_a_query_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    IEnumerable<IEnumerable<int>> M(IList<int> source)
                    {
                        yield return (
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            select item
                            ).ToArray();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_Queryable_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        IQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_iterator_method_returns_Queryable_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|yield return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_QueryableT_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        IQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_iterator_method_returns_QueryableT_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|yield return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_OrderedQueryable_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        IOrderedQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_iterator_method_returns_OrderedQueryable_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IOrderedQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|yield return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_OrderedQueryableT_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        IOrderedQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    [Fact]
    internal async Task When_iterator_method_returns_OrderedQueryableT_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IOrderedQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|yield return|] result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }
}
