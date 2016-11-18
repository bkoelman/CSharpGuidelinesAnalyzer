using System.Reflection;
using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public sealed class ProvideAssemblyInformationSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ProvideAssemblyInformationAnalyzer.DiagnosticId;

        [Fact]
        internal void When_assembly_has_all_attributes_filled_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof (AssemblyTitleAttribute).Assembly)
                .Using(typeof (AssemblyTitleAttribute).Namespace)
                .InGlobalScope(@"
                    [assembly: AssemblyTitle(""XXX"")]
                    [assembly: AssemblyDescription(""XXX"")]
                    [assembly: AssemblyConfiguration(""XXX"")]
                    [assembly: AssemblyCompany(""XXX"")]
                    [assembly: AssemblyProduct(""XXX"")]
                    [assembly: AssemblyCopyright(""XXX"")]
                    [assembly: AssemblyTrademark(""XXX"")]
                    [assembly: AssemblyVersion(""1.2.3.4"")]
                    [assembly: AssemblyFileVersion(""1.2.3.4"")]
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_assembly_has_all_attributes_empty_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof (AssemblyTitleAttribute).Assembly)
                .Using(typeof (AssemblyTitleAttribute).Namespace)
                .AllowingDiagnosticsOutsideSourceTree()
                .InGlobalScope(@"
                    [assembly: [|AssemblyTitle("""")|]]
                    [assembly: [|AssemblyDescription("""")|]]
                    [assembly: [|AssemblyConfiguration("""")|]]
                    [assembly: [|AssemblyCompany("""")|]]
                    [assembly: [|AssemblyProduct("""")|]]
                    [assembly: [|AssemblyCopyright("""")|]]
                    [assembly: [|AssemblyTrademark("""")|]]
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assembly-level attribute 'System.Reflection.AssemblyTitleAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyDescriptionAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyConfigurationAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyCompanyAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyProductAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyCopyrightAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyTrademarkAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyVersionAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyFileVersionAttribute' is missing or empty.");
        }

        [Fact]
        internal void When_assembly_has_no_attributes_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof (AssemblyTitleAttribute).Assembly)
                .Using(typeof (AssemblyTitleAttribute).Namespace)
                .AllowingDiagnosticsOutsideSourceTree()
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assembly-level attribute 'System.Reflection.AssemblyTitleAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyDescriptionAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyConfigurationAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyCompanyAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyProductAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyCopyrightAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyTrademarkAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyVersionAttribute' is missing or empty.",
                "Assembly-level attribute 'System.Reflection.AssemblyFileVersionAttribute' is missing or empty.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ProvideAssemblyInformationAnalyzer();
        }
    }
}