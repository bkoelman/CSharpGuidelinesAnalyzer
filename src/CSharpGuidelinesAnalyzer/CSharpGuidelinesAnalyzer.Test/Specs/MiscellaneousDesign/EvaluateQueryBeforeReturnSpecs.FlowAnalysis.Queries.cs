using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_returns_a_simple_query_expression_it_must_be_reported()
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
                        [|return|]
                            from item in Enumerable.Empty<int>()
                            where item != 2
                            select item;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M()' returns the result of a query, which uses deferred execution");
    }
}
