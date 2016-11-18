using CSharpGuidelinesAnalyzer.Rules.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public class AvoidStaticClassesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidStaticClassesAnalyzer.DiagnosticId;

        [Fact]
        public void When_class_is_not_static_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_static_class_name_does_not_end_with_Extensions_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Class 'C' should be non-static or its name should be suffixed with 'Extensions'.");
        }

        [Fact]
        public void When_static_class_has_no_extension_methods_and_name_ends_with_Extensions_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_static_class_has_public_extension_method_and_name_ends_with_Extensions_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_static_class_has_internal_extension_method_and_name_ends_with_Extensions_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_static_class_has_public_nonextension_method_and_name_ends_with_Extensions_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Class 'SomeExtensions' contains public non-extension method 'M'.");
        }

        [Fact]
        public void When_static_class_has_internal_nonextension_method_and_name_ends_with_Extensions_it_must_be_reported
            ()
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
            VerifyGuidelineDiagnostic(source,
                "Class 'SomeExtensions' contains internal non-extension method 'M'.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidStaticClassesAnalyzer();
        }
    }
}