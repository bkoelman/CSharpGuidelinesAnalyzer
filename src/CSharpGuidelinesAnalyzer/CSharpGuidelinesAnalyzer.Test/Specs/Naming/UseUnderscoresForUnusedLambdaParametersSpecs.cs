using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class UseUnderscoresForUnusedLambdaParametersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => UseUnderscoresForUnusedLambdaParametersAnalyzer.DiagnosticId;

        [Fact]
        internal void When_lamda_parameter_is_unused_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N([|x|] => true);
                    }

                    void N(Func<int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Unused lambda parameter 'x' should be renamed to one or more underscores.");
        }

        [Fact]
        internal void When_lamda_parameters_are_unused_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(([|x|], [|y|]) =>
                        {
                            throw new NotImplementedException();
                        });
                    }

                    void N(Func<int, string, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Unused lambda parameter 'x' should be renamed to one or more underscores.",
                "Unused lambda parameter 'y' should be renamed to one or more underscores.");
        }

        [Fact]
        internal void When_unused_lamda_parameter_is_named_underscore_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(_ => true);
                    }

                    void N(Func<int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_unused_lamda_parameters_are_named_with_underscores_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N((_, __) => true);
                    }

                    void N(Func<int, string, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_lamda_parameter_is_read_from_in_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(x => x > 5);
                    }

                    void N(Func<int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_lamda_parameter_is_written_to_in_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(x =>
                        {
                            x = 5;
                            return true;
                        });
                    }

                    void N(Func<int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_lamda_parameter_is_captured_in_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            N(x =>
                            {
                                N(_ => x > 5);
                                return true;
                            });
                        }

                        void N(Func<int, bool> f)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new UseUnderscoresForUnusedLambdaParametersAnalyzer();
        }
    }
}
