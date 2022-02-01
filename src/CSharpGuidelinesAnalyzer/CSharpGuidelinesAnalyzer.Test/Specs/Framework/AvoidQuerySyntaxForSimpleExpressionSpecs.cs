using System.Linq;
using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public sealed class AvoidQuerySyntaxForSimpleExpressionSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidQuerySyntaxForSimpleExpressionAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_contains_empty_query_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            select item|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_simple_filter_query_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            where item.Length > 2
                            select item|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_multiple_filter_query_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from string item in Enumerable.Empty<object>()
                            where item != null
                            where item.Length > 2
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_embedded_simple_filter_query_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var result = Process((
                            [|from item in Enumerable.Empty<string>()
                            where item.Length > 2
                            select item|]).ToArray());
                    }

                    object Process(object p) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_nested_simple_filter_query_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
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
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_simple_query_with_cast_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from string item in Enumerable.Empty<object>()
                            select item|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_complex_query_with_cast_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from string item in Enumerable.Empty<object>()
                            where item.Length > 2
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_simple_query_with_grouping_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            group item by item.Length|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_complex_query_with_grouping_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            where item != null
                            group item by item.Length;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_complex_query_with_grouping_into_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            group item by item.Length into grp
                            where grp != null
                            select grp;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_complex_query_with_join_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            join other in Enumerable.Empty<string>() on item.Length equals other.Length
                            where item != null
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_complex_query_with_join_into_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            join other in Enumerable.Empty<string>() on item.Length equals other.Length into product
                            where product != null
                            select product;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_simple_query_with_ordering_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            orderby item
                            select item|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_complex_query_with_ordering_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            where item.Length > 2
                            orderby item
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_simple_query_with_descending_ordering_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            orderby item descending
                            select item|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_complex_query_with_descending_ordering_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            where item.Length > 2
                            orderby item descending
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_complex_query_with_multiple_ordering_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            orderby item.Length, item
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_complex_query_with_multiple_sources_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            from other in Enumerable.Empty<string>()
                            where item.Length > 2
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_simple_projection_query_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            [|from item in Enumerable.Empty<string>()
                            select new { item, item.Length }|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Simple query should be replaced by extension method call");
        }

        [Fact]
        internal void When_method_contains_complex_projection_query_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            where item != null
                            select new { item, item.Length };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_let_query_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            let isLong = item.Length > 2
                            select item;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_incomplete_query_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .AllowingCompileErrors()
                .InDefaultClass(@"
                    void M()
                    {
                        var query =
                            from item in Enumerable.Empty<string>()
                            select ;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidQuerySyntaxForSimpleExpressionAnalyzer();
        }
    }
}
