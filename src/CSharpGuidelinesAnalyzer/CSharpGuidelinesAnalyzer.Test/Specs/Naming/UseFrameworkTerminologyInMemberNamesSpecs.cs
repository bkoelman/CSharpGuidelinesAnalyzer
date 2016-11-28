using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class UseFrameworkTerminologyInMemberNamesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => UseFrameworkTerminologyInMemberNamesAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_is_named_AddItem_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void [|AddItem|]()
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'AddItem' should be renamed to 'Add'.");
        }

        [Fact]
        internal void When_method_is_named_Delete_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void [|Delete|]()
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'Delete' should be renamed to 'Remove'.");
        }

        [Fact]
        internal void When_property_is_named_NumberOfItems_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public int [|NumberOfItems|] { get; set; }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'NumberOfItems' should be renamed to 'Count'.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new UseFrameworkTerminologyInMemberNamesAnalyzer();
        }
    }
}
