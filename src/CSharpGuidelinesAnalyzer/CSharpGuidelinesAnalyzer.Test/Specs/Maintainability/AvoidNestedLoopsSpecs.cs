using System;
using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AvoidNestedLoopsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidNestedLoopsAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidNestedLoopsAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}