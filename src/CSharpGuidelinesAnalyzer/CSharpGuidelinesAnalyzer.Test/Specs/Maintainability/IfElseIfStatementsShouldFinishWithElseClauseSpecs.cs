using System;
using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class IfElseIfStatementsShouldFinishWithElseClauseSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => IfElseIfStatementsShouldFinishWithElseClauseAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new IfElseIfStatementsShouldFinishWithElseClauseAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}