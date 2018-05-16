using CSharpGuidelinesAnalyzer.Rules.Documentation;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Documentation
{
    public sealed class DocumentInternalMemberSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DocumentInternalMemberAnalyzer.DiagnosticId;

        [Fact]
        internal void When_documentation_comments_are_disabled_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal class C
                        {
                            internal void M(DateTime p)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_undocumented_type_is_not_internal_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            protected abstract class X
                            {
                            }

                            protected internal class Y
                            {
                            }

                            private sealed class Z
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_class_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal class [|C|]
                        {
                            internal class [|X|]
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C'.",
                "Missing XML comment for internally visible type or member 'N.M.C.X'.");
        }

        [Fact]
        internal void When_internal_class_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary />
                        internal class C
                        {
                            /// <summary />
                            internal class X
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_struct_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal struct [|C|]
                        {
                            internal struct [|X|]
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C'.",
                "Missing XML comment for internally visible type or member 'N.M.C.X'.");
        }

        [Fact]
        internal void When_internal_struct_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary />
                        internal struct C
                        {
                            /// <summary />
                            internal struct X
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_enum_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal enum [|E|]
                        {
                            X
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.E'.");
        }

        [Fact]
        internal void When_internal_enum_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary />
                        internal enum E
                        {
                            X
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_interface_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal interface [|I|]
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.I'.");
        }

        [Fact]
        internal void When_internal_interface_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary />
                        internal interface I
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_delegate_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        internal delegate int [|D|](string s);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.D'.");
        }

        [Fact]
        internal void When_internal_delegate_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary />
                        internal delegate int D(string s);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_property_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public string P { get; set; }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_property_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            internal string [|P|] { get; set; }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.P'.");
        }

        [Fact]
        internal void When_internal_property_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal string P { get; set; }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_property_in_private_class_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class X
                            {
                                internal string P { get; set; }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_method_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public void M()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_method_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            internal void [|M|]()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.M()'.");
        }

        [Fact]
        internal void When_internal_method_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal void M()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_method_in_private_class_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class X
                            {
                                internal void M()
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_field_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public int F;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_field_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            internal int [|F|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.F'.");
        }

        [Fact]
        internal void When_internal_field_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal int F;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_field_in_private_class_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class X
                            {
                                internal int F;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_event_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public event EventHandler E;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_event_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            internal event EventHandler [|E|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.E'.");
        }

        [Fact]
        internal void When_internal_event_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal event EventHandler E;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_event_in_private_class_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class X
                            {
                                internal event EventHandler E;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_in_public_method_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public void M(int i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_in_internal_method_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal void M(int [|i|])
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible parameter 'i'.");
        }

        [Fact]
        internal void When_parameter_in_internal_method_is_documented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            /// <param name=""i"" />
                            internal void M(int i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_in_internal_method_in_private_class_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class X
                            {
                                internal void M(int i)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_in_lambda_expression_is_undocumented_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            internal void M()
                            {
                                Func<int, bool> action = i => true;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_internal_method_has_documentation_for_missing_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <summary />
                            /// <param name=""z"" />
                            /// <param name=""i"" />
                            internal void [|M|](int i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'z' in XML comment not found in method signature.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DocumentInternalMemberAnalyzer();
        }
    }
}
