using System;
using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class FavorAsyncAwaitOverTaskContinueWithSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => FavorAsyncAwaitOverTaskContinueWithAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new FavorAsyncAwaitOverTaskContinueWithAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}