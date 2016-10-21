using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AvoidConditionsWithDoubleNegativesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidConditionsWithDoubleNegativesAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidConditionsWithDoubleNegativesAnalyzer();
        }
    }
}