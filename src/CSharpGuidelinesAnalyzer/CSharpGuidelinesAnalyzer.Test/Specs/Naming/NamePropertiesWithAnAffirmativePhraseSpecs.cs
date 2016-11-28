using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class NamePropertiesWithAnAffirmativePhraseSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => NamePropertiesWithAnAffirmativePhraseAnalyzer.DiagnosticId;

        [Fact]
        internal void When_public_field_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public int some;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_field_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_field_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_field_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_field_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_property_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public int Some { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_property_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool IsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool [|ThisIsVisible|] { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_property_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? IsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? [|ThisIsVisible|] { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_inherited_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract bool [|ThisIsVisible|] { get; set; }
                    }

                    public class C : B
                    {
                        public override bool ThisIsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_inherited_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract bool? [|ThisIsVisible|] { get; set; }
                    }

                    public class C : B
                    {
                        public override bool? ThisIsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_implemented_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool [|ThisIsVisible|] { get; }
                    }

                    public class C : I
                    {
                        public bool ThisIsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        public void
            When_public_implemented_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool? [|ThisIsVisible|] { get; }
                    }

                    public class C : I
                    {
                        public bool? ThisIsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean property 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_method_return_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public int Some()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_method_return_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool IsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool [|ThisIsVisible|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_operator_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public struct C
                    {
                        public static bool operator ==(C left, C right)
                        {
                            throw new NotImplementedException();
                        }

                        public static bool operator !=(C left, C right)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_method_return_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? IsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_public_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool? [|ThisIsVisible|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_inherited_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract bool [|ThisIsVisible|]();
                    }

                    public class C : B
                    {
                        public override bool ThisIsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        public void
            When_public_inherited_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract bool? [|ThisIsVisible|]();
                    }

                    public class C : B
                    {
                        public override bool? ThisIsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_implemented_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool [|ThisIsVisible|]();
                    }

                    public class C : I
                    {
                        public bool ThisIsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        public void
            When_public_implemented_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool? [|ThisIsVisible|]();
                    }

                    public class C : I
                    {
                        public bool? ThisIsVisible()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean method 'ThisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_parameter_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M(int other)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_parameter_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M(bool isVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_public_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M(bool [|thisIsVisible|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void When_public_parameter_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M(bool? isVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_public_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M(bool? [|thisIsVisible|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_inherited_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract void M(bool [|thisIsVisible|]);
                    }

                    public class C : B
                    {
                        public override void M(bool thisIsVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        public void
            When_public_inherited_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract void M(bool? [|thisIsVisible|]);
                    }

                    public class C : B
                    {
                        public override void M(bool? thisIsVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_public_implemented_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool [|thisIsVisible|]);
                    }

                    public class C : I
                    {
                        public void M(bool thisIsVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        public void
            When_public_implemented_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool? [|thisIsVisible|]);
                    }

                    public class C : I
                    {
                        public void M(bool? thisIsVisible)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        internal void
            When_field_type_is_bool_and_name_does_not_start_with_a_verb_in_public_class_hierarchy_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public class D
                        {
                            public bool [|thisIsVisible1|];

                            protected bool [|thisIsVisible2|];

                            internal bool [|thisIsVisible3|];

                            protected internal bool [|thisIsVisible4|];

                            private bool thisIsVisible5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible1' should start with a verb.",
                "The name of boolean field 'thisIsVisible2' should start with a verb.",
                "The name of boolean field 'thisIsVisible3' should start with a verb.",
                "The name of boolean field 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_field_type_is_bool_and_name_does_not_start_with_a_verb_in_protected_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected class D
                        {
                            public bool [|thisIsVisible1|];

                            protected bool [|thisIsVisible2|];

                            internal bool [|thisIsVisible3|];

                            protected internal bool [|thisIsVisible4|];

                            private bool thisIsVisible5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible1' should start with a verb.",
                "The name of boolean field 'thisIsVisible2' should start with a verb.",
                "The name of boolean field 'thisIsVisible3' should start with a verb.",
                "The name of boolean field 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_field_type_is_bool_and_name_does_not_start_with_a_verb_in_internal_class_hierarchy_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    internal class C
                    {
                        internal class D
                        {
                            public bool [|thisIsVisible1|];

                            protected bool [|thisIsVisible2|];

                            internal bool [|thisIsVisible3|];

                            protected internal bool [|thisIsVisible4|];

                            private bool thisIsVisible5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible1' should start with a verb.",
                "The name of boolean field 'thisIsVisible2' should start with a verb.",
                "The name of boolean field 'thisIsVisible3' should start with a verb.",
                "The name of boolean field 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_field_type_is_bool_and_name_does_not_start_with_a_verb_in_protected_internal_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    internal class C
                    {
                        protected internal class D
                        {
                            public bool [|thisIsVisible1|];

                            protected bool [|thisIsVisible2|];

                            internal bool [|thisIsVisible3|];

                            protected internal bool [|thisIsVisible4|];

                            private bool thisIsVisible5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible1' should start with a verb.",
                "The name of boolean field 'thisIsVisible2' should start with a verb.",
                "The name of boolean field 'thisIsVisible3' should start with a verb.",
                "The name of boolean field 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_field_type_is_bool_and_name_does_not_start_with_a_verb_in_private_class_hierarchy_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        private class D
                        {
                            public bool thisIsVisible1;

                            protected bool thisIsVisible2;

                            internal bool thisIsVisible3;

                            protected internal bool thisIsVisible4;

                            private bool thisIsVisible5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_in_public_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public class D
                        {
                            public void M1(bool [|thisIsVisible1|])
                            {
                            }

                            protected void M2(bool [|thisIsVisible2|])
                            {
                            }

                            internal void M3(bool [|thisIsVisible3|])
                            {
                            }

                            protected internal void M4(bool [|thisIsVisible4|])
                            {
                            }

                            private void M5(bool thisIsVisible5)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible1' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible2' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible3' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_in_protected_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected class D
                        {
                            public void M1(bool [|thisIsVisible1|])
                            {
                            }

                            protected void M2(bool [|thisIsVisible2|])
                            {
                            }

                            internal void M3(bool [|thisIsVisible3|])
                            {
                            }

                            protected internal void M4(bool [|thisIsVisible4|])
                            {
                            }

                            private void M5(bool thisIsVisible5)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible1' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible2' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible3' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_in_internal_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    internal class C
                    {
                        internal class D
                        {
                            public void M1(bool [|thisIsVisible1|])
                            {
                            }

                            protected void M2(bool [|thisIsVisible2|])
                            {
                            }

                            internal void M3(bool [|thisIsVisible3|])
                            {
                            }

                            protected internal void M4(bool [|thisIsVisible4|])
                            {
                            }

                            private void M5(bool thisIsVisible5)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible1' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible2' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible3' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_in_protected_internal_class_hierarchy_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    internal class C
                    {
                        protected internal class D
                        {
                            public void M1(bool [|thisIsVisible1|])
                            {
                            }

                            protected void M2(bool [|thisIsVisible2|])
                            {
                            }

                            internal void M3(bool [|thisIsVisible3|])
                            {
                            }

                            protected internal void M4(bool [|thisIsVisible4|])
                            {
                            }

                            private void M5(bool thisIsVisible5)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean parameter 'thisIsVisible1' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible2' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible3' should start with a verb.",
                "The name of boolean parameter 'thisIsVisible4' should start with a verb.");
        }

        [Fact]
        internal void
            When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_in_private_class_hierarchy_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        private class D
                        {
                            public void M1(bool thisIsVisible1)
                            {
                            }

                            protected void M2(bool thisIsVisible2)
                            {
                            }

                            internal void M3(bool thisIsVisible3)
                            {
                            }

                            protected internal void M4(bool thisIsVisible4)
                            {
                            }

                            private void M5(bool thisIsVisible5)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new NamePropertiesWithAnAffirmativePhraseAnalyzer();
        }
    }
}
