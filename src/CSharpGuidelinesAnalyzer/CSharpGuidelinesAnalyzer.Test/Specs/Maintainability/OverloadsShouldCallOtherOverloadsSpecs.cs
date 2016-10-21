using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class OverloadsShouldCallOtherOverloadsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => OverloadsShouldCallOtherOverloadsAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new OverloadsShouldCallOtherOverloadsAnalyzer();
        }
    }
}