using System.Collections;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
    [Fact]
    internal async Task When_method_returns_the_result_of_Where_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable).Namespace)
            .Using(typeof(IList<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M(IList<int> source)
                    {
                        [|return|] source.Where(x => true);
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M(IList<IEnumerable<char>> source)
                    {
                        [|return|] source.Select(x => x.Where(y => true));
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M(IList<int> source)
                    {
                        [|return|] source.ToArray().Where(x => true);
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public IEnumerable M()
                    {
                        [|return|] new int[0].GroupBy(x => x);
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public IEnumerable<int> M(IEnumerable<int> source)
                    {
                        [|return|] (IList<int>)source.Skip(2);
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    IEnumerable<char> M()
                    {
                        IEnumerable<char> result = string.Join(string.Empty, string.Empty);
                        return result;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }
}
