using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AvoidMultipleTypesPerFileSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidMultipleTypesPerFileAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidMultipleTypesPerFileAnalyzer();
        }
    }
}