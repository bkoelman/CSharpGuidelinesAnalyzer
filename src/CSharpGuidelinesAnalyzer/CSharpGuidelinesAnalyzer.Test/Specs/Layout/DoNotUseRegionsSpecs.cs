using CSharpGuidelinesAnalyzer.Rules.Layout;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Layout;

public sealed class DoNotUseRegionsSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotUseRegionsAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_source_contains_no_regions_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                // #region This region is ignored due to single-line comments
                // #endregion

                /* #region This region is ignored due to multi-line comments
                #endregion */

                public class Region
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_source_contains_top_level_regions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                [|#region First|]
                #endregion

                [|#region Second|]
                #endregion
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Region should be removed",
            "Region should be removed");
    }

    [Fact]
    internal async Task When_source_contains_nested_regions_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                [|#region Outer |]

                namespace N
                {
                    [|#region Inside N|]

                    class C
                    {
                        [|#region Inside N.C|]

                        public void M()
                        {
                            [|#region Inside N.C.M|]

                            throw null;

                            #endregion
                        }

                        #endregion

                        [|#region End of N.C|]
                        #endregion
                    }

                    #endregion
                }

                #endregion
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Region should be removed",
            "Region should be removed",
            "Region should be removed",
            "Region should be removed",
            "Region should be removed");
    }

    [Fact]
    internal async Task When_source_contains_unbalanced_region_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .AllowingCompileErrors()
            .InGlobalScope("[|#region Missing end-marker |]")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Region should be removed");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotUseRegionsAnalyzer();
    }
}
