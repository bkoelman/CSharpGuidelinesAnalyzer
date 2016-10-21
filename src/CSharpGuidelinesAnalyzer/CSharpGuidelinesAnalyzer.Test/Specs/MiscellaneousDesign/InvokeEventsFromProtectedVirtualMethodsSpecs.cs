using CSharpGuidelinesAnalyzer.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public class InvokeEventsFromProtectedVirtualMethodsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => InvokeEventsFromProtectedVirtualMethodsAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new InvokeEventsFromProtectedVirtualMethodsAnalyzer();
        }
    }
}