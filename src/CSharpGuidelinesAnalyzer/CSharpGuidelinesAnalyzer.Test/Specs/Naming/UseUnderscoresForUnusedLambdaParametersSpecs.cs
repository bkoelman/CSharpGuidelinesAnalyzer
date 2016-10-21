using CSharpGuidelinesAnalyzer.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class UseUnderscoresForUnusedLambdaParametersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => UseUnderscoresForUnusedLambdaParametersAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new UseUnderscoresForUnusedLambdaParametersAnalyzer();
        }
    }
}