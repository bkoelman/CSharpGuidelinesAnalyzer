using System.Collections;
using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public sealed class EvaluateQueryBeforeReturnSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => EvaluateQueryBeforeReturnAnalyzer.DiagnosticId;

    #region Analysis, based solely on method signature

    [Fact]
    internal async Task When_method_return_type_is_void_it_must_be_skipped()
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_untyped_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                IEnumerable M()
                {
                    yield return new int[0].Where(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_int_it_must_be_skipped()
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_int_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<int> M()
                {
                    yield return 5;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_string_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                string M()
                {
                    return string.Empty;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_string_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<string> M()
                {
                    yield return string.Empty;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_object_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                object M()
                {
                    return new int[0].Where(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_object_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                IEnumerable<object> M()
                {
                    yield return new int[0].Where(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_dynamic_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                dynamic M()
                {
                    return new int[0].Where(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_dynamic_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                IEnumerable<dynamic> M()
                {
                    yield return new int[0].Where(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_tuple_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                (IEnumerable, string) M()
                {
                    return (new int[0].Where(x => true), string.Empty);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_tuple_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                IEnumerable<(IEnumerable, string)> M()
                {
                    yield return (new int[0].Where(x => true), string.Empty);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_array_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                IEnumerable[] M()
                {
                    return new[] { new int[0].Where(x => true) };
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_array_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IEnumerable[]> M()
                {
                    yield return new[] { new int[0].Where(x => true) };
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_ArrayList_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(ArrayList).Namespace)
            .InDefaultClass(@"
                ArrayList M()
                {
                    return new ArrayList();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_ArrayList_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .Using(typeof(ArrayList).Namespace)
            .InDefaultClass(@"
                IEnumerable<ArrayList> M()
                {
                    yield return new ArrayList();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_ListT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                List<string> M()
                {
                    return new List<string>();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_ListT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<List<string>> M()
                {
                    yield return new List<string>();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_IOrderedEnumerableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IOrderedEnumerable<int> M()
                {
                    return new List<int>().Where(x => true).OrderBy(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_IOrderedEnumerableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IOrderedEnumerable<int>> M()
                {
                    yield return new List<int>().Where(x => true).OrderBy(x => true);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_IQueryable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IQueryable M()
                {
                    return new List<int>().Where(x => true).AsQueryable();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_IQueryable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IQueryable> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_IQueryableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IQueryable<int> M()
                {
                    return new List<int>().Where(x => true).AsQueryable();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_IQueryableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IQueryable<int>> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_IOrderedQueryable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IOrderedQueryable M()
                {
                    return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_IOrderedQueryable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IOrderedQueryable> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_IOrderedQueryableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IOrderedQueryable<int> M()
                {
                    return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_return_type_is_IOrderedQueryableT_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IQueryable<>).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass(@"
                IEnumerable<IOrderedQueryable<int>> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    #endregion

    #region Analysis, based on compile-time type of return value

    [Fact]
    internal async Task When_method_returns_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<IEnumerable<string>> M()
                    {
                        yield break;
                        yield return null;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M()
                    {
                        return default(IEnumerable);
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<IEnumerable> M()
                    {
                        yield return default(IEnumerable);
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<T> M<T>()
                    {
                        return default;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<IEnumerable<T>> M<T>()
                    {
                        yield return default;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M()
                    {
                        return new List<int>();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<IEnumerable<int>> M()
                    {
                        yield return new List<int>();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        return source.Where(x => true).ToArray();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<IEnumerable> M(IList<int> source)
                    {
                        yield return source.Where(x => true).ToArray();
                    }
                }
            ")
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_iterator_method_returns_array_after_a_query_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        IQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|yield return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        IQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true);
                        [|yield return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        IOrderedQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IOrderedQueryable result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|yield return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        IOrderedQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<IEnumerable> M()
                    {
                        IOrderedQueryable<int> result = Enumerable.Empty<int>().AsQueryable().Where(x => true).OrderBy(x => x);
                        [|yield return|] result;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns an IQueryable, which uses deferred execution");
    }

    #endregion

    #region Analysis, based on block scope

    [Fact]
    internal async Task When_local_function_returns_the_result_of_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M()
                    {
                        return null;

                        IEnumerable<int> L()
                        {
                            return new int[0].Where(x => true);
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_lambda_expression_returns_the_result_of_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M()
                    {
                        Func<IEnumerable<int>> f = () =>
                        {
                            return new int[0].Where(x => true);
                        };

                        return null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_anonymous_method_returns_the_result_of_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M()
                    {
                        Func<IEnumerable<int>> f = delegate
                        {
                            return new int[0].Where(x => true);
                        };

                        return null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    #endregion

    #region Flow analysis for matching signature, return type and scope.

    #region Flow analysis on chained method calls

    [Fact]
    internal async Task When_method_returns_the_result_of_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M(IList<int> source)
                    {
                        [|return|] source.Where(x => true);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_Select_invocation_with_nested_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M(IList<IEnumerable<char>> source)
                    {
                        [|return|] source.Select(x => x.Where(y => true));
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<IEnumerable<char>>)' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_Where_invocation_after_ToArray_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M(IList<int> source)
                    {
                        [|return|] source.ToArray().Where(x => true);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a call to 'Where', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_GroupBy_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        [|return|] new int[0].GroupBy(x => x);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'GroupBy', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_cast_to_result_of_Skip_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IEnumerable<int> source)
                    {
                        [|return|] (IList<int>)source.Skip(2);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IEnumerable<int>)' returns the result of a call to 'Skip', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_Select_invocation_with_code_block_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        var result = Enumerable.Empty<int>().Select(x =>
                        {
                            if (x == 0)
                            {
                                return string.Empty;
                            }

                            return x.ToString();
                        });

                        [|return|] result;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'Select', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_the_result_of_string_Join_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<char> M()
                    {
                        IEnumerable<char> result = string.Join(string.Empty, string.Empty);
                        return result;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    #endregion

    #region Flow analysis on query expressions

    [Fact]
    internal async Task When_method_returns_a_simple_query_expression_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a query, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_a_simple_query_expression_with_redundant_parentheses_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a query, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_a_query_expression_with_ordering_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        [|return|]
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            orderby item
                            select item;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>)' returns the result of a query, which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_a_query_expression_with_grouping_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<IGrouping<int, int>> M()
                    {
                        [|return|]
                            from item in Enumerable.Empty<int>()
                            group item by item.GetHashCode() into grp
                            select grp;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a query, which uses deferred execution");
    }

    #endregion

    #region Flow analysis using variable tracking

    [Fact]
    internal async Task When_method_returns_variable_that_contains_the_result_of_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        var result = source.Where(x => true);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = source.Where(x => true).ToList();
                        return result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = Enumerable.Empty<int>();
                        result = source.Where(x => true);
                        [|return|] result;
                    }
                }
            ")
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_variable_that_eventually_contains_the_result_of_Select_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(IList<int>, bool)' returns the result of a call to 'Select', which uses deferred execution");
    }

    #endregion

    #region Flow analysis on control flow constructs

    [Fact]
    internal async Task When_method_returns_the_result_of_conditional_operator_after_Where_invocation_on_left_side_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source, bool condition)
                    {
                        var result = source.Where(x => true);
                        [|return|] condition ? result : source;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source, bool condition)
                    {
                        var result = source.Where(x => true);
                        [|return|] condition ? source : result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source, bool condition)
                    {
                        [|return|] (condition ? (source.Where(x => true)) : (null));
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M(IEnumerable<int> source, bool condition)
                    {
                        var result = source.Where(x => true);
                        return condition ? result.ToArray() : source.ToArray();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        int[] result = source.ToArray();
                        [|return|] result?.Select(x => x);
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = source.Select(x => x);
                        return result?.ToList();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        IEnumerable<int> result = source.Select(x => x);
                        [|return|] result ?? new List<int>();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(IList<int> source)
                    {
                        [|return|] source ?? new int[0].Select(x => x);
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M(List<int> source)
                    {
                        IEnumerable<int> result = source.ToArray();
                        return result ?? new List<int>();
                    }
                }
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        IEnumerable result = new[] { 1, 2, 3 }.Where(x => true);
                        [|return|] result;
                    }
                }
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M(IList<IEnumerable<int>> source)
                    {
                        IEnumerable<int>[] array = source.Where(x => true).ToArray();
                        return array[0];
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M(IList<IEnumerable<int>> source)
                    {
                        Func<IEnumerable<IEnumerable<int>>> f = () => source.Where(x => true);
                        return f();
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable<int> result = default;
                        return result;
                    }
                }
            ")
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
            .InGlobalScope(@"
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
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable result = nameof(Enumerable.SkipWhile);
                        return result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M()
                    {
                        IEnumerable result = """";
                        return result;
                    }
                }
            ")
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
            .InGlobalScope(@"
                class C
                {
                    IEnumerable M(IList<int> source)
                    {
                        IEnumerable result = source ?? throw new Exception(Enumerable.Empty<int>().Skip(1).ToString());
                        return result;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    #endregion

    #region Flow analysis on assignment in return statement

    [Fact]
    internal async Task When_method_returns_variable_assignment_to_result_of_Skip_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M()
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_variable_assignment_to_result_of_ToArray_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    IEnumerable<int> M()
                    {
                        IEnumerable<int> temp = new List<int>
                        {
                            1, 2, 3, 4, 5
                        }.Skip(2);

                        return temp = temp.ToArray();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_parameter_assignment_to_result_of_Skip_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<int> M(ref IEnumerable<int> source)
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(ref IEnumerable<int>)' returns the result of a call to 'Skip', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_field_assignment_to_result_of_Skip_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    private IEnumerable<int> f;

                    public IEnumerable<int> M()
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution");
    }

    [Fact]
    internal async Task When_method_returns_property_assignment_to_result_of_Skip_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    private IEnumerable<int> P { get; set; }

                    public IEnumerable<int> M()
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a call to 'Skip', which uses deferred execution");
    }

    #endregion

    #endregion

    [Fact]
    internal async Task When_enum_defines_field_with_explicit_value_it_must_not_crash()
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_declaration_initializes_with_empty_delegate_it_must_not_crash()
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
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_private_method_returns_the_result_of_Where_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                public class C
                {
                    private IEnumerable<int> M(IEnumerable<int> source)
                    {
                        return source.Where(x => true);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new EvaluateQueryBeforeReturnAnalyzer();
    }
}
