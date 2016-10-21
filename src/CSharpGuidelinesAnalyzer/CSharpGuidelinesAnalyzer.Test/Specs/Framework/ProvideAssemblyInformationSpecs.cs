using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class ProvideAssemblyInformationSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ProvideAssemblyInformationAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ProvideAssemblyInformationAnalyzer();
        }
    }
}