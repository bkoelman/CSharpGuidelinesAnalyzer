using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public partial class EvaluateQueryBeforeReturnSpecs
{
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
}
