using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class DoNotUseAbbreviationsInIdentifierNamesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotUseAbbreviationsInIdentifierNamesAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseAbbreviationsInIdentifierNamesAnalyzer();
        }
    }
}