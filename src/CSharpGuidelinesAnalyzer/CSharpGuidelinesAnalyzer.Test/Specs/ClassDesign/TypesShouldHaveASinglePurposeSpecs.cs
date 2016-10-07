using System;
using CSharpGuidelinesAnalyzer.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public class TypesShouldHaveASinglePurposeSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => TypesShouldHaveASinglePurposeAnalyzer.DiagnosticId;

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new TypesShouldHaveASinglePurposeAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}