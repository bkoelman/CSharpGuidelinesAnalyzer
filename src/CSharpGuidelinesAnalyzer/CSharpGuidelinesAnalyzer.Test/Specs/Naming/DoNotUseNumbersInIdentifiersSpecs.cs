using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class DoNotUseNumbersInIdentifiersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotUseNumbersInIdentifiersAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseNumbersInIdentifiersAnalyzer();
        }
    }
}