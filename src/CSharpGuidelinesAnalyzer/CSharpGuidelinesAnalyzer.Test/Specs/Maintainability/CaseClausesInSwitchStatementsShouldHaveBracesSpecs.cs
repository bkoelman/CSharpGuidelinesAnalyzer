using System;
using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class CaseClausesInSwitchStatementsShouldHaveBracesSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => CaseClausesInSwitchStatementsShouldHaveBracesAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new CaseClausesInSwitchStatementsShouldHaveBracesAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}