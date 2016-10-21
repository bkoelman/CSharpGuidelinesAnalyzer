using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class UseFrameworkTerminologyInMemberNamesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => UseFrameworkTerminologyInMemberNamesAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new UseFrameworkTerminologyInMemberNamesAnalyzer();
        }
    }
}