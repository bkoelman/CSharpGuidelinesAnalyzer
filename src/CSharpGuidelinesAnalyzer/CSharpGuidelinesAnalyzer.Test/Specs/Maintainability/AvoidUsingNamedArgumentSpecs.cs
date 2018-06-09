using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidUsingNamedArgumentSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidUsingNamedArgumentAnalyzer.DiagnosticId;

        [Fact]
        internal void When_using_a_named_argument_of_string_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int i, string s = null)
                    {
                    }

                    void N()
                    {
                        M(3, [|s:|] string.Empty);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 's' in the call to 'Test.M(int, string)' is invoked with a named argument.");
        }

        [Fact]
        internal void When_using_a_non_trailing_named_argument_of_string_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int i, string s = null, int j = 1)
                    {
                    }

                    void N()
                    {
                        M(3, [|s:|] string.Empty, 6);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 's' in the call to 'Test.M(int, string, int)' is invoked with a named argument.");
        }

        [Fact]
        internal void When_using_a_named_argument_of_object_type_in_local_function_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Outer()
                    {
                        void L(int i, object o = null)
                        {
                        }

                        void N()
                        {
                            L(3, [|o:|] null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'o' in the call to 'L(int, object)' is invoked with a named argument.");
        }

        [Fact]
        internal void When_using_a_named_argument_of_boolean_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool b = false)
                    {
                    }

                    void N()
                    {
                        M(b: true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_using_a_named_argument_of_boolean_type_in_local_function_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        void L(bool b = false)
                        {
                        }

                        void N()
                        {
                            L(b: true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_using_a_named_argument_of_nullable_boolean_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool? b = null)
                    {
                    }

                    void N()
                    {
                        M(b: true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_using_a_named_argument_of_nullable_boolean_type_in_local_function_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        void L(bool? b = null)
                        {
                        }

                        void N()
                        {
                            L(b: true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidUsingNamedArgumentAnalyzer();
        }
    }
}
