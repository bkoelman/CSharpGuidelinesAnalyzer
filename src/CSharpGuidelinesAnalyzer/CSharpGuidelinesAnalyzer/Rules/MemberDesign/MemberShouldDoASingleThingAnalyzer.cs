using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberShouldDoASingleThingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1115";

        private const string Title = "Member or local function contains the word 'and', which suggests doing multiple things";
        private const string MessageFormat = "{0} '{1}' contains the word 'and', which suggests doing multiple things.";
        private const string Description = "A property, method or local function should do only one thing.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        private const string BlacklistWord = "and";

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context.ReportDiagnostic);
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var operation = (ILocalFunctionOperation)context.Operation;

            AnalyzeSymbol(operation.Symbol, context.ReportDiagnostic);
        }

        private static void AnalyzeSymbol([NotNull] ISymbol symbol, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (symbol.IsPropertyOrEventAccessor() || symbol.IsUnitTestMethod() || symbol.IsSynthesized())
            {
                return;
            }

            if (ContainsBlacklistedWord(symbol.Name))
            {
                reportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.GetKind(), symbol.Name));
            }
        }

        private static bool ContainsBlacklistedWord([NotNull] string name)
        {
            return name.ContainsWordInTheMiddle(BlacklistWord);
        }
    }
}
