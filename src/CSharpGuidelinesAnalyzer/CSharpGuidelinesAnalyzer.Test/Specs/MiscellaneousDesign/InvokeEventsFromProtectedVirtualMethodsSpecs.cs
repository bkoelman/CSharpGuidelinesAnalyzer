using System;
using CSharpGuidelinesAnalyzer.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public class InvokeEventsFromProtectedVirtualMethodsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => InvokeEventsFromProtectedVirtualMethodsAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new InvokeEventsFromProtectedVirtualMethodsAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}