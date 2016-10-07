using System;
using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class ProvideAssemblyInformationSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ProvideAssemblyInformationAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ProvideAssemblyInformationAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}