using System;
using CSharpGuidelinesAnalyzer.Documentation;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Documentation
{
    public class AvoidCodeCommentsWithToDosSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidCodeCommentsWithToDosAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidCodeCommentsWithToDosAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}