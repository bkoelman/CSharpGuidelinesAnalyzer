using CSharpGuidelinesAnalyzer.Rules.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign;

public sealed class AvoidStaticClassSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidStaticClassAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_class_is_not_static_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_name_does_not_end_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class [|C|]
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'C' should be non-static or its name should be suffixed with 'Extensions'");
    }

    [Fact]
    internal async Task When_partial_static_class_name_does_not_end_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static partial class [|C|]
                {
                }

                static partial class C
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'C' should be non-static or its name should be suffixed with 'Extensions'");
    }

    [Fact]
    internal async Task When_static_class_contains_no_members_and_name_ends_with_Extensions_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_contains_nonstatic_nested_type_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    class Nested
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_contains_static_nested_type_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    static class [|Nested|]
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'Nested' should be non-static or its name should be suffixed with 'Extensions'");
    }

    [Fact]
    internal async Task When_static_class_contains_public_extension_method_and_name_ends_with_Extensions_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public static void M(this string s)
                    {
                    }

                    private static void InnerMethod(string s)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_contains_internal_extension_method_and_name_ends_with_Extensions_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    internal static void M(this string s)
                    {
                    }

                    private static void InnerMethod(string s)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_contains_public_non_extension_method_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public static void [|M|](string s)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains public member 'M', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_internal_non_extension_method_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    internal static void [|M|](string s)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains internal member 'M', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_public_const_field_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public const int [|C|] = 1;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains public member 'C', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_public_static_field_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public static readonly int [|F|] = 1;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains public member 'F', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_public_static_property_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public static int [|P|]
                    {
                        get; set;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains public member 'P', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_public_static_event_and_name_ends_with_Extensions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    public static event EventHandler [|E|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Extension method container class 'SomeExtensions' contains public member 'E', which is not an extension method");
    }

    [Fact]
    internal async Task When_static_class_contains_static_constructor_and_name_ends_with_Extensions_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class SomeExtensions
                {
                    static SomeExtensions()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_contains_entry_point_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOutputKind(OutputKind.WindowsApplication)
            .InGlobalScope(@"
                static class Program
                {
                    static void Main(string[] args)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_static_class_is_platform_invoke_wrapper_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                static class NativeMethods
                {
                }

                static class SafeNativeMethods
                {
                }

                static class UnsafeNativeMethods
                {
                }

                public class Wrapper
                {
                    private static class NativeMethods
                    {
                    } 
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidStaticClassAnalyzer();
    }
}
