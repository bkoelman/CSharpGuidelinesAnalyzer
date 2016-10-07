using System;
using CSharpGuidelinesAnalyzer.Documentation;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Documentation
{
    public class DocumentAllInternalMembersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DocumentAllInternalMembersAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DocumentAllInternalMembersAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}