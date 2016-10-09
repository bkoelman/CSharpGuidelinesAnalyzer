using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Framework;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public class BuildWithTheHighestWarningLevelSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => BuildWithTheHighestWarningLevelAnalyzer.DiagnosticId;

        [Fact]
        public void When_warning_level_is_set_to_four_it_must_be_skipped()
        {
            // Arrange
            const int warningLevel = 4;
            DocumentWithSpans documentWithSpans = CreateEmptyDocumentWithWarningLevel(warningLevel);

            // Act
            ImmutableArray<Diagnostic> diagnostics = GetDiagnosticsForDocument(documentWithSpans.Document, null, false);

            // Assert
            diagnostics.Should().HaveCount(0);
        }

        [Fact]
        public void When_warning_level_is_set_to_three_it_must_be_reported()
        {
            // Arrange
            const int warningLevel = 3;
            DocumentWithSpans documentWithSpans = CreateEmptyDocumentWithWarningLevel(warningLevel);

            // Act
            ImmutableArray<Diagnostic> diagnostics = GetDiagnosticsForDocument(documentWithSpans.Document, null, false);

            // Assert
            diagnostics.Should().HaveCount(1);
            diagnostics[0].Id.Should().Be(DiagnosticId);
            diagnostics[0].Location.Should().Be(Location.None);
        }

        [Fact]
        public void When_warning_level_is_set_to_two_it_must_be_reported()
        {
            // Arrange
            const int warningLevel = 2;
            DocumentWithSpans documentWithSpans = CreateEmptyDocumentWithWarningLevel(warningLevel);

            // Act
            ImmutableArray<Diagnostic> diagnostics = GetDiagnosticsForDocument(documentWithSpans.Document, null, false);

            // Assert
            diagnostics.Should().HaveCount(1);
            diagnostics[0].Id.Should().Be(DiagnosticId);
            diagnostics[0].Location.Should().Be(Location.None);
        }

        [Fact]
        public void When_warning_level_is_set_to_one_it_must_be_reported()
        {
            // Arrange
            const int warningLevel = 1;
            DocumentWithSpans documentWithSpans = CreateEmptyDocumentWithWarningLevel(warningLevel);

            // Act
            ImmutableArray<Diagnostic> diagnostics = GetDiagnosticsForDocument(documentWithSpans.Document, null, false);

            // Assert
            diagnostics.Should().HaveCount(1);
            diagnostics[0].Id.Should().Be(DiagnosticId);
            diagnostics[0].Location.Should().Be(Location.None);
        }

        // Note: at warning level 0, analyzer do not even run. So a test for that is omitted here.

        [NotNull]
        private static DocumentWithSpans CreateEmptyDocumentWithWarningLevel(int warningLevel)
        {
            return TestHelpers.GetDocumentAndSpansFromMarkup(string.Empty, LanguageNames.CSharp,
                ImmutableList<MetadataReference>.Empty, SourceCodeBuilder.DefaultFilename, warningLevel);
        }

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