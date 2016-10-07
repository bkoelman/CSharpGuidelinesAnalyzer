using System;
using CSharpGuidelinesAnalyzer.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign
{
    public class ReturnInterfacesToCollectionsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ReturnInterfacesToCollectionsAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ReturnInterfacesToCollectionsAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}