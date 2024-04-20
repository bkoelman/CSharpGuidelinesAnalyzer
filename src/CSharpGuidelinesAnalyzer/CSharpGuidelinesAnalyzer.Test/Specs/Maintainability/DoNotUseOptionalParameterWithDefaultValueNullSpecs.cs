using System.Collections;
using System.Runtime.CompilerServices;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class DoNotUseOptionalParameterWithDefaultValueNullSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotUseOptionalParameterWithDefaultValueNullAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_using_optional_nullable_int_parameter_with_default_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M(int? p = null) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_string_parameter_with_default_value_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M(string p = "empty") => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_string_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M([|string p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'string' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_string_parameter_with_default_null_with_CallerArgumentExpression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(CallerArgumentExpressionAttribute).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M(object value, [CallerArgumentExpression("value")] string parameterName = null) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_List_of_int_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(List<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|List<int> p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'List<int>' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_IEnumerable_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|IEnumerable p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'IEnumerable' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_int_array_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void M([|int[] p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'int[]' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_Task_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|Task p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'Task' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_Task_of_int_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|Task<int> p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'Task<int>' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_ValueTask_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(ValueTask).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|ValueTask? p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'ValueTask?' has default value 'null'");
    }

    [Fact]
    internal async Task When_using_optional_ValueTask_of_int_parameter_with_default_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(ValueTask<>).Namespace)
            .InGlobalScope("""
                class C
                {
                    void M([|ValueTask<int>? p = null|]) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Optional parameter 'p' of type 'ValueTask<int>?' has default value 'null'");
    }

    [Fact]
    internal async Task When_method_implicitly_implements_interface_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor("""
                public interface I
                {
                    void M(string s = null);
                }
                """)
            .InGlobalScope("""
                class C : I
                {
                    public void M(string s = null) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_explicitly_implements_interface_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor("""
                public interface I
                {
                    void M(string s = null);
                }
                """)
            .InGlobalScope("""
                class C : I
                {
                    void I.M(string s = null) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_overrides_base_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor("""
                public abstract class B
                {
                    public virtual void M(string s = null) => throw null;
                }
                """)
            .InGlobalScope("""
                class C : B
                {
                    public override void M(string s = null) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotUseOptionalParameterWithDefaultValueNullAnalyzer();
    }
}
