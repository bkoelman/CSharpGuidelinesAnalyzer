using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class UseUnderscoreForUnusedLambdaParameterSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => UseUnderscoreForUnusedLambdaParameterAnalyzer.DiagnosticId;

        [Fact]
        internal void When_anonymous_method_parameter_is_unused_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(delegate(int [|x|])
                        {
                            return true;
                        });
                    }

                    void N(Func<int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Unused anonymous method parameter 'x' should be renamed to underscore(s).");
        }

        [Fact]
        internal void When_anonymous_method_parameters_are_unused_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(delegate(int [|x|], int [|y|])
                        {
                            return true;
                        });
                    }

                    void N(Func<int, int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Unused anonymous method parameter 'x' should be renamed to underscore(s).",
                "Unused anonymous method parameter 'y' should be renamed to underscore(s).");
        }

        [Fact]
        internal void When_unused_anonymous_method_parameter_is_named_underscore_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(delegate(int _)
                        {
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
        internal void When_unused_anonymous_method_parameters_are_named_with_underscores_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N(delegate(int _, int __)
                        {
                            return true;
                        });
                    }

                    void N(Func<int, int, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_lambda_parameter_is_unused_in_body_it_must_be_reported()
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
                "Unused lambda parameter 'x' should be renamed to underscore(s).");
        }

        [Fact]
        internal void When_lambda_parameters_are_unused_in_body_it_must_be_reported()
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
                "Unused lambda parameter 'x' should be renamed to underscore(s).",
                "Unused lambda parameter 'y' should be renamed to underscore(s).");
        }

        [Fact]
        internal void When_unused_lambda_parameter_is_named_underscore_it_must_be_skipped()
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
        internal void When_unused_lambda_parameters_are_named_with_underscores_it_must_be_skipped()
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
        internal void When_lambda_parameter_is_read_from_in_body_it_must_be_skipped()
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
        internal void When_lambda_parameter_is_written_to_in_body_it_must_be_skipped()
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
        internal void When_lambda_parameter_is_captured_in_body_it_must_be_skipped()
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

        [Fact]
        internal void When_using_self_referencing_nameof_in_property_it_must_not_crash()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        object P { get; } = Create(nameof(P), x => x.Length > 0);

                        static object Create(string name, Func<string, bool> f) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new UseUnderscoreForUnusedLambdaParameterAnalyzer();
        }
    }
}
