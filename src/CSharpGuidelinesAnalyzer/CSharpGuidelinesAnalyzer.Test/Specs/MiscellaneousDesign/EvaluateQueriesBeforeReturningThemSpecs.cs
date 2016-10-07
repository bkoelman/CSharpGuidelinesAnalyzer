using System;
using CSharpGuidelinesAnalyzer.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public class EvaluateQueriesBeforeReturningThemSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => EvaluateQueriesBeforeReturningThemAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new EvaluateQueriesBeforeReturningThemAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}