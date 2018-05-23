using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseFrameworkTerminologyInMemberNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1711";

        private const string Title = "Name members and local functions similarly to members of .NET Framework classes";
        private const string MessageFormat = "{0} '{1}' should be renamed to '{2}'.";
        private const string Description = "Name members similarly to members of related .NET Framework classes.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [NotNull]
        private static readonly ImmutableDictionary<string, string> WordsReplacementMap =
            new Dictionary<string, string> { { "AddItem", "Add" }, { "Delete", "Remove" }, { "NumberOfItems", "Count" } }
                .ToImmutableDictionary();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor() || context.Symbol.IsSynthesized())
            {
                return;
            }

            AnalyzeSymbol(context.Symbol, context.ReportDiagnostic);
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var localFunction = (ILocalFunctionOperation)context.Operation;

            AnalyzeSymbol(localFunction.Symbol, context.ReportDiagnostic);
        }

        private static void AnalyzeSymbol([NotNull] ISymbol symbol, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (WordsReplacementMap.ContainsKey(symbol.Name))
            {
                reportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.GetKind(), symbol.Name,
                    WordsReplacementMap[symbol.Name]));
            }
        }
    }
}
