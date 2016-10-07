using System;
using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class BuildWithTheHighestWarningLevelSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => BuildWithTheHighestWarningLevelAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new BuildWithTheHighestWarningLevelAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}