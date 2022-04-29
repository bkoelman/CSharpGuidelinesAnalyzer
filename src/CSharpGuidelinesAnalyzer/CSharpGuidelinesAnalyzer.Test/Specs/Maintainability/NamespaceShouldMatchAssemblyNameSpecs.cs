using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class NamespaceShouldMatchAssemblyNameSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => NamespaceShouldMatchAssemblyNameAnalyzer.DiagnosticId;

        [Fact]
        internal async Task When_assembly_name_matches_with_namespace_name_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace Scope
                        {
                            namespace Example
                            {
                                namespace Deeper
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_assembly_name_does_not_match_with_namespace_name_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace [|WrongScope|]
                        {
                            namespace [|Example|]
                            {
                                namespace [|Deeper|]
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'Some.WrongScope' does not match with assembly name 'Some.Scope.Example'",
                "Namespace 'Some.WrongScope.Example' does not match with assembly name 'Some.Scope.Example'",
                "Namespace 'Some.WrongScope.Example.Deeper' does not match with assembly name 'Some.Scope.Example'");
        }

        [Fact]
        internal async Task When_assembly_name_starts_with_namespace_name_but_does_not_match_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace [|Scope2|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'Some.Scope2' does not match with assembly name 'Some.Scope.Example'");
        }

        [Fact]
        internal async Task When_namespace_name_starts_with_assembly_name_but_does_not_match_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope2.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace [|Scope|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'Some.Scope' does not match with assembly name 'Some.Scope2.Example'");
        }

        [Fact]
        internal async Task When_assembly_name_matches_with_namespace_of_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace Scope
                        {
                            namespace Example
                            {
                                public class Good
                                {
                                }

                                namespace Deeper
                                {
                                    public class AlsoGood
                                    {
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_assembly_name_does_not_match_with_namespace_of_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace Scope
                        {
                            public class [|TooHigh|]
                            {
                            }

                            namespace Example
                            {
                                namespace Deeper
                                {
                                }
                            }

                            public class [|AlsoTooHigh|]
                            {
                            }

                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Type 'TooHigh' is declared in namespace 'Some.Scope', which does not match with assembly name 'Some.Scope.Example'",
                "Type 'AlsoTooHigh' is declared in namespace 'Some.Scope', which does not match with assembly name 'Some.Scope.Example'");
        }

        [Fact]
        internal async Task When_assembly_name_starts_with_namespace_of_type_but_does_not_match_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope2")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace [|Scope|]
                        {
                            public class [|WrongSpace|]
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'Some.Scope' does not match with assembly name 'Some.Scope2'",
                "Type 'WrongSpace' is declared in namespace 'Some.Scope', which does not match with assembly name 'Some.Scope2'");
        }

        [Fact]
        internal async Task When_namespace_of_type_starts_with_assembly_name_but_does_not_match_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope")
                .InGlobalScope(@"
                    namespace Some
                    {
                        namespace [|Scope2|]
                        {
                            public class [|WrongSpace|]
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'Some.Scope2' does not match with assembly name 'Some.Scope'",
                "Type 'WrongSpace' is declared in namespace 'Some.Scope2', which does not match with assembly name 'Some.Scope'");
        }

        [Fact]
        internal async Task When_type_is_defined_in_global_namespace_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .InGlobalScope(@"
                    public class [|C|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Type 'C' is declared in global namespace, which does not match with assembly name 'Some.Scope.Example'");
        }

        [Fact]
        internal async Task When_type_is_defined_in_global_namespace_in_Core_assembly_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Core")
                .InGlobalScope(@"
                    public class [|C|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Type 'C' is declared in global namespace, which does not match with assembly name 'Core'");
        }

        [Fact]
        internal async Task When_type_is_defined_in_non_global_namespace_in_Core_assembly_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Core")
                .InGlobalScope(@"
                    namespace Some
                    {
                        public class C
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_type_is_defined_in_global_namespace_in_assembly_name_ending_with_Core_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Company.Core")
                .InGlobalScope(@"
                    public class [|C|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Type 'C' is declared in global namespace, which does not match with assembly name 'Company.Core'");
        }

        [Fact]
        internal async Task When_type_is_defined_in_namespace_above_Core_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Company.Core")
                .InGlobalScope(@"
                    namespace Company
                    {
                        class Top
                        {
                        }

                        namespace Deeper
                        {
                            class C
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_namespace_is_JetBrains_Annotations_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Company.ProductName")
                .InGlobalScope(@"
                    namespace JetBrains
                    {
                        namespace Annotations
                        {
                            class C
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_namespace_is_not_JetBrains_Annotations_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Company.ProductName")
                .InGlobalScope(@"
                    namespace [|WrongRoot|]
                    {
                        namespace [|JetBrains|]
                        {
                            namespace [|Annotations|]
                            {
                                class [|C|]
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Namespace 'WrongRoot' does not match with assembly name 'Company.ProductName'",
                "Namespace 'WrongRoot.JetBrains' does not match with assembly name 'Company.ProductName'",
                "Namespace 'WrongRoot.JetBrains.Annotations' does not match with assembly name 'Company.ProductName'",
                "Type 'C' is declared in namespace 'WrongRoot.JetBrains.Annotations', which does not match with assembly name 'Company.ProductName'");
        }

        [Fact]
        internal async Task When_top_level_program_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .WithOutputKind(OutputKind.ConsoleApplication)
                .InGlobalScope(@"
                    System.Console.WriteLine("""");
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_explicit_program_in_global_namespace_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InAssemblyNamed("Some.Scope.Example")
                .WithOutputKind(OutputKind.ConsoleApplication)
                .InGlobalScope(@"
                    static class [|Program|]
                    {
                        static void Main()
                        {
                            System.Console.WriteLine("""");
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source, "Type 'Program' is declared in global namespace, which does not match with assembly name 'Some.Scope.Example'");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new NamespaceShouldMatchAssemblyNameAnalyzer();
        }
    }
}
