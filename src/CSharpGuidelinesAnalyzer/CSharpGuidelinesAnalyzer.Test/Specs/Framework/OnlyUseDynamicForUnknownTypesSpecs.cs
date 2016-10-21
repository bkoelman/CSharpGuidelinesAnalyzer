using System.Runtime.CompilerServices;
using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class OnlyUseDynamicForUnknownTypesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => OnlyUseDynamicForUnknownTypesAnalyzer.DiagnosticId;

        [Fact]
        public void When_declared_variable_of_type_dynamic_is_assigned_to_another_dynamic_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InDefaultClass(@"
                    void M(dynamic p)
                    {
                        dynamic d = p;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_declared_variable_of_type_dynamic_is_assigned_to_an_Object_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(object p)
                    {
                        dynamic d = p;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void
            When_declared_variable_of_type_dynamic_is_assigned_to_implicitly_cast_string_constant_it_must_be_reported
            ()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|dynamic d = ""A"";|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "A non-dynamic result is implicitly assigned to dynamic identifier 'd'.");
        }

        [Fact]
        public void
            When_declared_variable_of_type_dynamic_is_assigned_to_explicitly_cast_string_constant_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        dynamic d = (dynamic)""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_of_type_dynamic_is_assigned_to_another_dynamic_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InDefaultClass(@"
                    void M(dynamic p)
                    {
                        dynamic d;
                        d = p;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_of_type_dynamic_is_assigned_to_an_Object_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(object p)
                    {
                        dynamic d;
                        d = p;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_variable_of_type_dynamic_is_assigned_to_implicitly_cast_string_constant_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        dynamic d;
                        [|d = ""A""|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "A non-dynamic result is implicitly assigned to dynamic identifier 'd'.");
        }

        [Fact]
        public void When_variable_of_type_dynamic_is_assigned_to_explicitly_cast_string_constant_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        dynamic d;
                        d = (dynamic)""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_parameter_of_type_dynamic_is_assigned_to_another_dynamic_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        void M(dynamic p1, ref dynamic p2)
                        {
                            p2 = p1;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_parameter_of_type_dynamic_is_assigned_to_an_Object_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        void M(object p, out dynamic d)
                        {
                            d = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_parameter_of_type_dynamic_is_assigned_to_implicitly_cast_string_constant_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        void M(ref dynamic d)
                        {
                            [|d = ""A""|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "A non-dynamic result is implicitly assigned to dynamic identifier 'd'.");
        }

        [Fact]
        public void When_parameter_of_type_dynamic_is_assigned_to_explicitly_cast_string_constant_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        void M(ref dynamic d)
                        {
                            d = (dynamic)""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_of_type_dynamic_is_assigned_to_another_dynamic_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        private dynamic f;

                        void M(dynamic p)
                        {
                            f = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_of_type_dynamic_is_assigned_to_an_Object_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        private dynamic f;

                        void M(object p)
                        {
                            f = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_field_of_type_dynamic_is_assigned_to_implicitly_cast_string_constant_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        private dynamic f;

                        void M()
                        {
                            [|f = ""A""|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "A non-dynamic result is implicitly assigned to dynamic identifier 'f'.");
        }

        [Fact]
        public void When_field_of_type_dynamic_is_assigned_to_explicitly_cast_string_constant_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        private dynamic f;

                        void M()
                        {
                            f = (dynamic)""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_of_type_dynamic_is_assigned_to_another_dynamic_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        dynamic P { get; set; }

                        void M(dynamic p)
                        {
                            P = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_of_type_dynamic_is_assigned_to_an_Object_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        dynamic P { get; set; }

                        void M(object p)
                        {
                            P = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_of_type_dynamic_is_assigned_to_implicitly_cast_string_constant_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        dynamic P { get; set; }

                        void M()
                        {
                            [|P = ""A""|];
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "A non-dynamic result is implicitly assigned to dynamic identifier 'P'.");
        }

        [Fact]
        public void When_property_of_type_dynamic_is_assigned_to_explicitly_cast_string_constant_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .WithReference(typeof (DynamicAttribute).Assembly)
                .InGlobalScope(@"
                    class C
                    {
                        dynamic P { get; set; }

                        void M()
                        {
                            P = (dynamic)""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new OnlyUseDynamicForUnknownTypesAnalyzer();
        }
    }
}