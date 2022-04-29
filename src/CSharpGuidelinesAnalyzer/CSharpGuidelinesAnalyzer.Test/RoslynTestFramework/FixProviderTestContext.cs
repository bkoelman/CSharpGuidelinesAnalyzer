using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace RoslynTestFramework
{
    public sealed class FixProviderTestContext
    {
        [NotNull]
        public AnalyzerTestContext AnalyzerTestContext { get; }

        [NotNull]
        [ItemNotNull]
        public ImmutableList<string> ExpectedCode { get; }

        [NotNull]
        [ItemNotNull]
        public ImmutableList<string> EquivalenceKeysForFixAll { get; }

        public TextComparisonMode CodeComparisonMode { get; }

        public FixProviderTestContext([NotNull] AnalyzerTestContext analyzerTestContext,
            [NotNull] [ItemNotNull] IEnumerable<string> expectedCode, TextComparisonMode codeComparisonMode)
            : this(analyzerTestContext, expectedCode, ImmutableList<string>.Empty, codeComparisonMode)
        {
            FrameworkGuard.NotNull(analyzerTestContext, nameof(analyzerTestContext));
            FrameworkGuard.NotNull(expectedCode, nameof(expectedCode));
        }

        private FixProviderTestContext([NotNull] AnalyzerTestContext analyzerTestContext,
            [NotNull] [ItemNotNull] IEnumerable<string> expectedCode,
            [NotNull] [ItemNotNull] IEnumerable<string> equivalenceKeysForFixAll, TextComparisonMode codeComparisonMode)
        {
            AnalyzerTestContext = analyzerTestContext;
            ExpectedCode = ImmutableList.CreateRange(expectedCode);
            EquivalenceKeysForFixAll = ImmutableList.CreateRange(equivalenceKeysForFixAll);
            CodeComparisonMode = codeComparisonMode;
        }

        [NotNull]
        public FixProviderTestContext WithExpectedCode([NotNull] [ItemNotNull] IEnumerable<string> expectedCode)
        {
            FrameworkGuard.NotNull(expectedCode, nameof(expectedCode));

            return new FixProviderTestContext(AnalyzerTestContext, expectedCode, EquivalenceKeysForFixAll, CodeComparisonMode);
        }

        [NotNull]
        public FixProviderTestContext WithEquivalenceKeysForFixAll(
            [NotNull] [ItemNotNull] IEnumerable<string> equivalenceKeysForFixAll)
        {
            FrameworkGuard.NotNull(equivalenceKeysForFixAll, nameof(equivalenceKeysForFixAll));

            return new FixProviderTestContext(AnalyzerTestContext, ExpectedCode, equivalenceKeysForFixAll, CodeComparisonMode);
        }
    }
}
