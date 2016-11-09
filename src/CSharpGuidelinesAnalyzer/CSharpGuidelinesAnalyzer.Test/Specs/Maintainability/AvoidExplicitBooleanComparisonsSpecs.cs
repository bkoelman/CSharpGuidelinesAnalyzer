using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AvoidExplicitBooleanComparisonsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidExplicitBooleanComparisonsAnalyzer.DiagnosticId;

        [Fact]
        public void When_testing_boolean_for_equality_with_true_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool b)
                    {
                        if (b == [|true|])
                        {
                        }

                        if ([|true|] == b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Expression contains explicit comparison to 'true'.",
                "Expression contains explicit comparison to 'true'.");
        }

        [Fact]
        public void When_testing_boolean_for_inequality_with_true_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool b)
                    {
                        if (b != [|true|])
                        {
                        }

                        if ([|true|] != b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Expression contains explicit comparison to 'true'.",
                "Expression contains explicit comparison to 'true'.");
        }

        [Fact]
        public void When_testing_boolean_for_equality_with_false_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool b)
                    {
                        if (b == [|false|])
                        {
                        }

                        if ([|false|] == b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Expression contains explicit comparison to 'false'.",
                "Expression contains explicit comparison to 'false'.");
        }

        [Fact]
        public void When_testing_boolean_for_inequality_with_false_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool b)
                    {
                        if (b != [|false|])
                        {
                        }

                        if ([|false|] != b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Expression contains explicit comparison to 'false'.",
                "Expression contains explicit comparison to 'false'.");
        }

        [Fact]
        public void When_testing_nullable_boolean_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool? b)
                    {
                        if (b == true)
                        {
                        }

                        if (false != b)
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
            return new AvoidExplicitBooleanComparisonsAnalyzer();
        }
    }
}