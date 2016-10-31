using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class NamePropertiesWithAnAffirmativePhraseSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => NamePropertiesWithAnAffirmativePhraseAnalyzer.DiagnosticId;

        [Fact]
        public void When_field_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int some;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private bool isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private bool [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        public void When_field_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private bool? isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private bool? [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean field 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        public void When_property_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public int Some { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public bool IsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_property_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public bool? IsVisible { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_inherited_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_inherited_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_property_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_property_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_method_return_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_method_return_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_method_return_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_inherited_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_inherited_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_method_return_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_method_return_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_parameter_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_parameter_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_parameter_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        public void When_inherited_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_inherited_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_parameter_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_implemented_parameter_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_variable_type_is_int_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int some = 3;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_type_is_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        bool isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_type_is_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        bool [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean variable 'thisIsVisible' should start with a verb.");
        }

        [Fact]
        public void When_variable_type_is_nullable_bool_and_name_starts_with_a_verb_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        bool? isVisible;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_type_is_nullable_bool_and_name_does_not_start_with_a_verb_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        bool? [|thisIsVisible|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The name of boolean variable 'thisIsVisible' should start with a verb.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new NamePropertiesWithAnAffirmativePhraseAnalyzer();
        }
    }
}