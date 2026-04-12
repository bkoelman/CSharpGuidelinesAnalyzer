using System.Globalization;
using System.Net;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class DoNotDeclareRefOrOutParameterSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotDeclareRefOrOutParameterAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_parameter_has_no_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int p)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(ref int [|p|])
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_out_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(out int [|p|])
                {
                    p = 1;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_in_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(in int p)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_has_out_modifier_in_deconstruct_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public string Text;
                    public bool IsEnabled;

                    public void Deconstruct(out string text, out bool isEnabled)
                    {
                        text = Text;
                        isEnabled = IsEnabled;
                    }

                    static void Test()
                    {
                        S s = new S
                        {
                            Text = string.Empty,
                            IsEnabled = true
                        };

                        (string a, bool b) = s;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_to_ref_struct_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                ref struct S
                {
                }

                class C
                {
                    void M(ref S s) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_constructor_parameter_has_no_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    C(TimeSpan p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_constructor_parameter_has_ref_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    C(ref bool [|p|])
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_constructor_parameter_has_out_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    C(out string [|p|])
                    {
                        p = default;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_local_function_parameter_has_no_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(int p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_parameter_has_ref_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(ref int [|p|])
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_local_function_parameter_has_out_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(out int [|p|])
                    {
                        p = 1;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_delegate_parameter_has_no_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("public delegate void D(DateTime p);")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_parameter_has_ref_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("public delegate void D(ref object [|p|]);")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_delegate_parameter_has_out_modifier_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("public delegate void D(out string [|p|]);")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_in_overridden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    protected virtual void M(ref string [|p|])
                    {
                    }
                }

                class D : C
                {
                    protected override void M(ref string p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_in_hidden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    protected virtual void M(ref string [|p|])
                    {
                    }
                }

                class D : C
                {
                    protected new void M(ref string p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_in_implicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                interface I
                {
                    void M(ref bool [|p|]);
                }

                class C : I
                {
                    public void M(ref bool p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_method_parameter_has_ref_modifier_in_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                interface I
                {
                    void M(ref bool [|p|]);
                }

                class C : I
                {
                    void I.M(ref bool p)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p' is declared as ref or out");
    }

    [Fact]
    internal async Task When_invocation_argument_has_ref_or_out_modifier_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int x, int y, int z)
                {
                    Target(x, in y, ref z, out int _);
                }

                void Target(int a, in int b, ref int [|c|], out int [|d|]) => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'c' is declared as ref or out",
            "Parameter 'd' is declared as ref or out");
    }

    [Fact]
    internal async Task When_invocation_argument_has_out_modifier_in_TryParse_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(CultureInfo).Namespace)
            .Using(typeof(IPAddress).Namespace)
            .InDefaultClass("""
                void M()
                {
                    bool result1 = int.TryParse(string.Empty, out _);

                    float number;
                    bool result2 = Single.TryParse(string.Empty, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out number);

                    bool result3 = IPAddress.TryParse(string.Empty, out var ipAddress);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_has_out_modifier_in_TryParse_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    bool result = Fraction.TryParse(string.Empty, out var value);
                }

                public sealed class Fraction
                {
                    public static bool TryParse(string text, out Fraction value) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_has_out_modifier_in_TryConvert_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    bool result = Fraction.TryConvert(string.Empty, out var value);
                }

                public sealed class Fraction
                {
                    public static bool TryConvert(string text, out Fraction value) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotDeclareRefOrOutParameterAnalyzer();
    }
}
