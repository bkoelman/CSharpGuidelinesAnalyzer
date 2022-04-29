using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class DoNotUseOptionalParameterInTypeHierarchySpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotUseOptionalParameterInTypeHierarchyAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_using_optional_parameter_in_regular_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    void M(int p = 5) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_required_parameter_in_interface_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
                    void M(int p);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_interface_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
                    void M([|int p = 5|]);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'I.M(int)' contains optional parameter 'p'");
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_implicitly_implemented_interface_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
                    void M([|int p = 5|]);
                }

                class C : I
                {
                    public void M([|int q = 8|]) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'I.M(int)' contains optional parameter 'p'",
            "Method 'C.M(int)' contains optional parameter 'q'");
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_implicitly_implemented_method_from_external_assembly_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor(@"
                public interface I
                {
                    void M(int p = 5);
                }
            ")
            .InGlobalScope(@"
                abstract class B : I
                {
                    public abstract void M(int q = 8);
                }

                class C : I
                {
                    public void M(int q = 8) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_explicitly_implemented_interface_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
                    void M([|int p = 5|]);
                }

                class C : I
                {
                    void I.M([|int q = 8|]) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'I.M(int)' contains optional parameter 'p'",
            "Method 'C.I.M(int)' contains optional parameter 'q'");
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_explicitly_implemented_interface_method_from_external_assembly_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor(@"
                public interface I
                {
                    void M(int p = 5);
                }
            ")
            .InGlobalScope(@"
                class C : I
                {
                    void I.M(int q = 8) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_abstract_virtual_or_overridden_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                abstract class A
                {
                    protected abstract void M([|int p = 5|]);
                }

                class B
                {
                    protected virtual void M([|int q = 8|]) => throw null;
                }

                class D : B
                {
                    protected override void M([|int r = 12|]) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'A.M(int)' contains optional parameter 'p'",
            "Method 'B.M(int)' contains optional parameter 'q'",
            "Method 'D.M(int)' contains optional parameter 'r'");
    }

    [Fact]
    internal async Task When_using_optional_parameter_in_overridden_method_from_external_assembly_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReferenceToExternalAssemblyFor(@"
                public abstract class B
                {
                    protected abstract void M(int p = 5);
                }
            ")
            .InGlobalScope(@"
                class C : B
                {
                    protected override void M(int q = 8) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotUseOptionalParameterInTypeHierarchyAnalyzer();
    }
}
