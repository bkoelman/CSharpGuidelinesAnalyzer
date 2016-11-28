using CSharpGuidelinesAnalyzer.Rules.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public sealed class TypesShouldHaveASinglePurposeSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => TypesShouldHaveASinglePurposeAnalyzer.DiagnosticId;

        [Fact]
        internal void When_class_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class [|CustomerAndOrder|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'CustomerAndOrder' contains the word 'and'.");
        }

        [Fact]
        internal void When_struct_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct [|customer_and_order|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'customer_and_order' contains the word 'and'.");
        }

        [Fact]
        internal void When_uppercase_struct_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct [|CUSTOMER_AND_ORDER|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'CUSTOMER_AND_ORDER' contains the word 'and'.");
        }

        [Fact]
        internal void When_enum_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|Match1And2|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'Match1And2' contains the word 'and'.");
        }

        [Fact]
        internal void When_uppercase_enum_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|MATCH1AND2|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'MATCH1AND2' contains the word 'and'.");
        }

        [Fact]
        internal void When_class_name_contains_the_word_Land_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class TheBigLand
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_struct_name_contains_the_word_Andy_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct AndyWithJohn
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new TypesShouldHaveASinglePurposeAnalyzer();
        }
    }
}
