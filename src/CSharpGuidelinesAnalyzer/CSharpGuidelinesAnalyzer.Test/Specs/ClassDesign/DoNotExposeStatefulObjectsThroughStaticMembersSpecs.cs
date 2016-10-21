using CSharpGuidelinesAnalyzer.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public class DoNotExposeStatefulObjectsThroughStaticMembersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotExposeStatefulObjectsThroughStaticMembersAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotExposeStatefulObjectsThroughStaticMembersAnalyzer();
        }
    }
}