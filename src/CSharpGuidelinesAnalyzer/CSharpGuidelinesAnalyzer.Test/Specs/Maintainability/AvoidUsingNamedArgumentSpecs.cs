using System.Threading.Tasks;
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
        internal async Task When_using_a_named_argument_of_string_type_it_must_be_reported()
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
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 's' in the call to 'Test.M(int, string)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_a_non_trailing_named_argument_of_string_type_it_must_be_reported()
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
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 's' in the call to 'Test.M(int, string, int)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_a_named_argument_of_object_type_in_local_function_it_must_be_reported()
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
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 'o' in the call to 'L(int, object)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_a_named_argument_of_boolean_type_it_must_be_skipped()
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
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_using_a_named_argument_of_boolean_type_in_local_function_it_must_be_skipped()
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
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_using_a_named_argument_of_nullable_boolean_type_it_must_be_skipped()
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
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_using_a_named_argument_of_nullable_boolean_type_in_local_function_it_must_be_skipped()
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
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_using_only_unneeded_named_arguments_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int a, string b = null, double c = 0.4, float d = 0.7f)
                    {
                    }

                    void N()
                    {
                        M(1, [|d:|] 3.0f, [|b:|] string.Empty, [|c:|] 2.0);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 'd' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument",
                "Parameter 'b' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument",
                "Parameter 'c' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_some_unneeded_named_arguments_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int a, string b = null, double c = 0.4, float d = 0.7f)
                    {
                    }

                    void N()
                    {
                        M(1, [|c:|] 2.0, [|b:|] string.Empty);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 'c' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument",
                "Parameter 'b' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_one_unneeded_named_argument_after_regular_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int a, string b = null, double c = 0.4, float d = 0.7f)
                    {
                    }

                    void N()
                    {
                        M(1, d: 3.0f, [|b:|] string.Empty);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 'b' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_one_unneeded_named_argument_after_optional_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int a, string b = null, double c = 0.4, float d = 0.7f)
                    {
                    }

                    void N()
                    {
                        M(1, string.Empty, [|c:|] 2.0);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Parameter 'c' in the call to 'Test.M(int, string, double, float)' is invoked with a named argument");
        }

        [Fact]
        internal async Task When_using_only_needed_named_arguments_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int a, string b = null, double c = 0.4, float d = 0.7f)
                    {
                    }

                    void N()
                    {
                        M(1, d: 3.0f, c: 2.0);
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidUsingNamedArgumentAnalyzer();
        }
    }
}
