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
        internal void When_documentation_comments_are_not_well_formed_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <missingClosingTag>
                        internal class C
                        {
                            /// <missingEndOfTag
                            public void M(int p) => throw null;
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
        internal void When_internal_class_is_undocumented_in_private_class_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private class D
                            {
                                internal class E
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
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.E'.");
        }

        [Fact]
        internal void When_internal_enum_member_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary/>
                        internal enum E
                        {
                            [|X|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.E.X'.");
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
                            /// <summary />
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
        internal void When_undocumented_member_is_not_internal_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            public string F1;
                            protected string F2;
                            protected internal string F3;
                            private string F4;

                            public string P1 { get; set; }
                            protected string P2 { get; set; }
                            protected internal string P3 { get; set; }
                            private string P4 { get; set; }

                            public event EventHandler E1;
                            protected event EventHandler E2;
                            protected internal event EventHandler E3;
                            private event EventHandler E4;

                            public int M1(string p1) => throw new NotImplementedException();
                            protected int M2(string p2) => throw new NotImplementedException();
                            protected internal int M3(string p3) => throw new NotImplementedException();
                            private int M4(string p4) => throw new NotImplementedException();
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
        internal void When_public_field_in_internal_type_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary/>
                        internal class C
                        {
                            public int [|F|];
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
        internal void When_private_protected_field_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private protected int [|F|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.F'.");
        }

        [Fact]
        internal void When_private_protected_field_is_documented_it_must_be_skipped()
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
                            private protected int F;
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
        internal void When_public_property_in_internal_type_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary/>
                        internal class C
                        {
                            public string [|P|] { get; set; }
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
        internal void When_private_protected_property_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private protected string [|P|] { get; set; }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.P'.");
        }

        [Fact]
        internal void When_private_protected_property_is_documented_it_must_be_skipped()
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
                            private protected string P { get; set; }
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
        internal void When_public_event_in_internal_type_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary/>
                        internal class C
                        {
                            public event EventHandler [|E|];
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
        internal void When_private_protected_event_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private protected event EventHandler [|E|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Missing XML comment for internally visible type or member 'N.M.C.E'.");
        }

        [Fact]
        internal void When_private_protected_event_is_documented_it_must_be_skipped()
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
                            private protected event EventHandler E;
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
        internal void When_public_method_in_internal_type_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        /// <summary/>
                        internal class C
                        {
                            public void [|M|]()
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
        internal void When_private_protected_method_is_undocumented_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            private protected void [|M|]()
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
        internal void When_private_protected_method_is_documented_it_must_be_skipped()
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
                            private protected void M()
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
        internal void When_parameter_in_internal_method_is_documented_via_inheritance_using_open_close_tag_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <inheritdoc></inheritdoc>
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
        internal void When_parameter_in_internal_method_is_documented_via_inheritance_using_self_closing_tag_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <inheritdoc />
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
        internal void
            When_parameter_in_internal_method_is_documented_via_inheritance_using_parameterized_self_closing_tag_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithDocumentationComments()
                .InGlobalScope(@"
                    namespace N.M
                    {
                        public class C
                        {
                            /// <inheritdoc cref=""some""/>
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
        internal void When_parameter_in_private_protected_method_is_undocumented_it_must_be_reported()
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
                            private protected void M(int [|i|])
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
        internal void When_parameter_in_private_protected_method_is_documented_it_must_be_skipped()
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
                            private protected void M(int i)
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
        internal void When_parameter_in_local_function_is_undocumented_it_must_be_skipped()
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
                                void N(int i)
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

        [Fact]
        internal void When_private_protected_method_has_documentation_for_missing_parameter_it_must_be_reported()
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
                            private protected void [|M|](int i)
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
