using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework;

public sealed class AvoidQuerySyntaxForSimpleExpressionSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidQuerySyntaxForSimpleExpressionAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_query_is_empty_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        select item|];
                
                    // Method chain equivalent (no invocations):
                    // var query = Enumerable.Empty<string>();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_converts_to_enumerable_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(List<>).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in new List<string>()
                        select item|];
                
                    // Method chain equivalent (single invocation):
                    // var query = new List<string>().Select(item => item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_filter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        where item.Length > 2
                        select item|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().Where(item => item.Length > 2);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_multiple_filters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        where item != null
                        where item.Length > 2
                        select item;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .Where(item => item != null)
                    //     .Where(item => item.Length > 2);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_embedded_query_contains_filter_with_ToArray_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var result = Process([|(
                            from item in Enumerable.Empty<string>()
                            where item.Length > 2
                            select item
                        )
                        .ToArray()|]);
                
                    // Method chain equivalent (multiple invocations, yet still simpler):
                    // var result = Process(Enumerable.Empty<string>()
                    //     .Where(item => item.Length > 2)
                    //     .ToArray());
                }

                object Process(object p) => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_ToList_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query = [|(
                        from item in Enumerable.Empty<string>()
                        select item
                    ).ToList()|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().ToList();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_nested_query_contains_filter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in (
                            [|from other in Enumerable.Empty<string>()
                            where other.Length > 2
                            select other|])
                        where item.Length > 2
                        orderby item.Length
                        select item;
                
                    // Method chain equivalent (single invocation):
                    // var query =
                    //     from item in
                    //         Enumerable.Empty<string>().Where(other => other.Length > 2)
                    //     where item.Length > 2
                    //     orderby item.Length
                    //     select item;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_cast_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from string item in Enumerable.Empty<object>()
                        select item|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<object>().Cast<string>();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_cast_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from string item in Enumerable.Empty<object>()
                        where item.Length > 2
                        select item;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<object>()
                    //     .Cast<string>()
                    //     .Where(item => item.Length > 2);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_grouping_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        group item by item.Length|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().GroupBy(item => item.Length);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_grouping_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        where item != null
                        group item by item.Length;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .Where(item => item != null)
                    //     .GroupBy(item => item.Length);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_grouping_into_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        group item by item.Length
                        into grp
                        select grp|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().GroupBy(item => item.Length);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_grouping_into_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        group item by item.Length
                        into grp
                        where grp.Key > 0
                        select grp;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .GroupBy(item => item.Length)
                    //     .Where(grp => grp.Key > 0);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_join_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        join other in Enumerable.Empty<string>() on item.Length equals other.Length
                        select item;
                
                    // Method chain equivalent (single invocation, yet more complex):
                    // var query = Enumerable.Empty<string>()
                    //     .Join(Enumerable.Empty<string>(),
                    //         item => item.Length,
                    //         other => other.Length,
                    //         (item, other) => item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_join_into_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        join other in Enumerable.Empty<string>() on item.Length equals other.Length
                        into product
                        select product;
                
                    // Method chain equivalent (single invocation, yet more complex):
                    // var query = Enumerable.Empty<string>()
                    //     .GroupJoin(Enumerable.Empty<string>(),
                    //         item => item.Length,
                    //         other => other.Length,
                    //         (item, product) => product);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_ordering_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        orderby item descending
                        select item|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().OrderByDescending(item => item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_ordering_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        where item.Length > 2
                        orderby item descending
                        select item;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .Where(item => item.Length > 2)
                    //     .OrderByDescending(item => item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_multiple_orderings_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        orderby item.Length, item
                        select item;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .OrderBy(item => item.Length)
                    //     .ThenBy(item => item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_multiple_sources_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from left in Enumerable.Empty<string>()
                        from right in Enumerable.Empty<string>()
                        select new { left, right };
                
                    // Method chain equivalent (single invocation, yet more complex):
                    // var query = Enumerable.Empty<string>().SelectMany(left => Enumerable.Empty<string>(), (left, right) => new { left, right });
                }
                """)
            .Build();

        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_multiple_sources_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from left in Enumerable.Empty<string>()
                        from right in Enumerable.Empty<string>()
                        where left.Length > right.Length
                        select new { left, right };
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .SelectMany(left => Enumerable.Empty<string>(), (left, right) => new { left, right })
                    //     .Where(pair => pair.left.Length > pair.right.Length)
                    //     .Select(pair => new { pair.left, pair.right });
                }
                """)
            .Build();

        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_projection_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        [|from item in Enumerable.Empty<string>()
                        select new { item, item.Length }|];
                
                    // Method chain equivalent (single invocation):
                    // var query = Enumerable.Empty<string>().Select(item => new { item, item.Length });
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Simple query should be replaced by extension method call");
    }

    [Fact]
    internal async Task When_query_contains_projection_with_filter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        where item != null
                        select new { item, item.Length };
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .Where(item => item != null)
                    //     .Select(item => new { item, item.Length });
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_contains_let_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        let isLong = item.Length > 2
                        select item;
                
                    // Method chain equivalent (multiple invocations):
                    // var query = Enumerable.Empty<string>()
                    //     .Select(item => new { item, isLong = item.Length > 2 })
                    //     .Select(pair => pair.item);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_query_is_incomplete_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .AllowingCompileErrors()
            .InDefaultClass("""
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<string>()
                        select ;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidQuerySyntaxForSimpleExpressionAnalyzer();
    }
}
