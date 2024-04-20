using System.Collections;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_return_type_is_void_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    return;
                }
                """)
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
            .InDefaultClass("""
                IEnumerable M()
                {
                    yield return new int[0].Where(x => true);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_int_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                int M()
                {
                    return 5;
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<int> M()
                {
                    yield return 5;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_return_type_is_string_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                string M()
                {
                    return string.Empty;
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<string> M()
                {
                    yield return string.Empty;
                }
                """)
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
            .InDefaultClass("""
                object M()
                {
                    return new int[0].Where(x => true);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<object> M()
                {
                    yield return new int[0].Where(x => true);
                }
                """)
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
            .InDefaultClass("""
                dynamic M()
                {
                    return new int[0].Where(x => true);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<dynamic> M()
                {
                    yield return new int[0].Where(x => true);
                }
                """)
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
            .InDefaultClass("""
                (IEnumerable, string) M()
                {
                    return (new int[0].Where(x => true), string.Empty);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<(IEnumerable, string)> M()
                {
                    yield return (new int[0].Where(x => true), string.Empty);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable[] M()
                {
                    return new[] { new int[0].Where(x => true) };
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IEnumerable[]> M()
                {
                    yield return new[] { new int[0].Where(x => true) };
                }
                """)
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
            .InDefaultClass("""
                ArrayList M()
                {
                    return new ArrayList();
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<ArrayList> M()
                {
                    yield return new ArrayList();
                }
                """)
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
            .InDefaultClass("""
                List<string> M()
                {
                    return new List<string>();
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<List<string>> M()
                {
                    yield return new List<string>();
                }
                """)
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
            .InDefaultClass("""
                IOrderedEnumerable<int> M()
                {
                    return new List<int>().Where(x => true).OrderBy(x => true);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IOrderedEnumerable<int>> M()
                {
                    yield return new List<int>().Where(x => true).OrderBy(x => true);
                }
                """)
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
            .InDefaultClass("""
                IQueryable M()
                {
                    return new List<int>().Where(x => true).AsQueryable();
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IQueryable> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable();
                }
                """)
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
            .InDefaultClass("""
                IQueryable<int> M()
                {
                    return new List<int>().Where(x => true).AsQueryable();
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IQueryable<int>> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable();
                }
                """)
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
            .InDefaultClass("""
                IOrderedQueryable M()
                {
                    return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IOrderedQueryable> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
                """)
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
            .InDefaultClass("""
                IOrderedQueryable<int> M()
                {
                    return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
                """)
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
            .InDefaultClass("""
                IEnumerable<IOrderedQueryable<int>> M()
                {
                    yield return new List<int>().Where(x => true).AsQueryable().OrderBy(x => x);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }
}
