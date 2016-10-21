using CSharpGuidelinesAnalyzer.Documentation;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Documentation
{
    public class DocumentAllInternalMembersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DocumentAllInternalMembersAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DocumentAllInternalMembersAnalyzer();
        }
    }
}