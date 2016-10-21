using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class PrefixEventHandlersWithOnSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => PrefixEventHandlersWithOnAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new PrefixEventHandlersWithOnAnalyzer();
        }
    }
}