using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public sealed partial class EvaluateQueryBeforeReturnSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => EvaluateQueryBeforeReturnAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_enum_defines_field_with_explicit_value_it_must_not_crash()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum E
                {
                    A = 1
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_declaration_initializes_with_empty_delegate_it_must_not_crash()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public event EventHandler SomethingChanged = delegate { };
                }
                """)
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
            .InGlobalScope("""
                public class C
                {
                    private IEnumerable<int> M(IEnumerable<int> source)
                    {
                        return source.Where(x => true);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new EvaluateQueryBeforeReturnAnalyzer();
    }
}
