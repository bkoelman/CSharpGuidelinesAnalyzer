using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidMultipleTypesPerFileSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidMultipleTypesPerFileAnalyzer.DiagnosticId;

        [Fact]
        internal async Task When_file_declares_a_single_class_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N1.N2
                    {
                        class C
                        {
                            class Nested
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
        internal async Task When_file_declares_multiple_classes_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        class C
                        {
                            class Nested
                            {
                            }
                        }

                        class [|C2|]
                        {
                        }
                    }

                    class [|C3|]
                    {
                    }

                    namespace N2.N3
                    {
                        class [|C4|]
                        {
                            class Nested
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "File 'Example.cs' contains additional type 'N1.C2'",
                "File 'Example.cs' contains additional type 'C3'",
                "File 'Example.cs' contains additional type 'N2.N3.C4'");
        }

        [Fact]
        internal async Task When_file_declares_multiple_classes_with_same_name_but_different_arity_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        class C
                        {
                            class Nested
                            {
                            }
                        }

                        class \u0043<T> // unicode escape for C
                        {
                        }
                    }

                    class C<TA, TB>
                    {
                    }

                    namespace N2.N3
                    {
                        class C<TA, TB, TC>
                        {
                            class Nested
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
        internal async Task When_file_declares_a_single_struct_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N1.N2
                    {
                        struct C
                        {
                            struct Nested
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
        internal async Task When_file_declares_multiple_structs_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        struct S
                        {
                            struct Nested
                            {
                            }
                        }

                        struct [|S2|]
                        {
                        }
                    }

                    struct [|S3|]
                    {
                    }

                    namespace N2.N3
                    {
                        struct [|S4|]
                        {
                            struct Nested
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "File 'Example.cs' contains additional type 'N1.S2'",
                "File 'Example.cs' contains additional type 'S3'",
                "File 'Example.cs' contains additional type 'N2.N3.S4'");
        }

        [Fact]
        internal async Task When_file_declares_multiple_structs_with_same_name_but_different_arity_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        struct S
                        {
                            struct Nested
                            {
                            }
                        }

                        struct \u0053<T> // unicode escape for S
                        {
                        }
                    }

                    struct S<TA, TB>
                    {
                    }

                    namespace N2.N3
                    {
                        struct S<TA, TB, TC>
                        {
                            struct Nested
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
        internal async Task When_file_declares_a_single_enum_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N1.N2
                    {
                        enum E
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_file_declares_multiple_enums_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        enum E
                        {
                        }

                        enum [|E2|]
                        {
                        }
                    }

                    enum [|E3|]
                    {
                    }

                    namespace N2.N3
                    {
                        enum [|E4|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "File 'Example.cs' contains additional type 'N1.E2'",
                "File 'Example.cs' contains additional type 'E3'",
                "File 'Example.cs' contains additional type 'N2.N3.E4'");
        }

        [Fact]
        internal async Task When_file_declares_a_single_interface_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N1.N2
                    {
                        interface I
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_file_declares_multiple_interfaces_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        interface I
                        {
                        }

                        interface [|I2|]
                        {
                        }
                    }

                    interface [|I3|]
                    {
                    }

                    namespace N2.N3
                    {
                        interface [|I4|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "File 'Example.cs' contains additional type 'N1.I2'",
                "File 'Example.cs' contains additional type 'I3'",
                "File 'Example.cs' contains additional type 'N2.N3.I4'");
        }

        [Fact]
        internal async Task When_file_declares_multiple_interfaces_with_same_name_but_different_arity_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        interface I
                        {
                        }

                        interface \u0049<T> // unicode escape for I
                        {
                        }
                    }

                    interface I<TA, TB>
                    {
                    }

                    namespace N2.N3
                    {
                        interface I<TA, TB, TC>
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_file_declares_a_single_delegate_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N1.N2
                    {
                        delegate void D();
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_file_declares_multiple_delegates_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        delegate void D();

                        delegate void [|D2|]();
                    }

                    delegate void [|D3|]();

                    namespace N2.N3
                    {
                        delegate void [|D4|]();
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "File 'Example.cs' contains additional type 'N1.D2'",
                "File 'Example.cs' contains additional type 'D3'",
                "File 'Example.cs' contains additional type 'N2.N3.D4'");
        }

        [Fact]
        internal async Task When_file_declares_multiple_delegates_with_same_name_but_different_arity_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InFileNamed("Example.cs")
                .InGlobalScope(@"
                    namespace N1
                    {
                        delegate void D();

                        delegate void \u0044<T>(T t); // unicode escape for D
                    }

                    delegate TB D<TA, TB>(TA ta);

                    namespace N2.N3
                    {
                        delegate TC D<TA, TB, TC>(TA ta, TB tb);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidMultipleTypesPerFileAnalyzer();
        }
    }
}
