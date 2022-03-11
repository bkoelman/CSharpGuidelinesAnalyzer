using System;
using System.Collections.Immutable;
using System.Threading;
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
        private const string Title = "Member or local function contains the word 'and', which suggests doing multiple things";
        private const string MessageFormat = "{0} '{1}' contains the word 'and', which suggests doing multiple things";
        private const string Description = "A property, method or local function should do only one thing.";
        private const string BlacklistWord = "and";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1115";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeMemberAction = context => context.SkipEmptyName(AnalyzeMember);

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeLocalFunctionAction = context => context.SkipInvalid(AnalyzeLocalFunction);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMemberAction, MemberSymbolKinds);
            context.RegisterOperationAction(AnalyzeLocalFunctionAction, OperationKind.LocalFunction);
        }

        private static void AnalyzeMember(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context.ReportDiagnostic, context.CancellationToken);
        }

        private static void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var operation = (ILocalFunctionOperation)context.Operation;

            AnalyzeSymbol(operation.Symbol, context.ReportDiagnostic, context.CancellationToken);
        }

        private static void AnalyzeSymbol([NotNull] ISymbol symbol, [NotNull] Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
        {
            if (RequiresAnalysis(symbol, cancellationToken) && ContainsBlacklistedWord(symbol.Name))
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.GetKind(), symbol.Name);
                reportDiagnostic(diagnostic);
            }
        }

        private static bool RequiresAnalysis([NotNull] ISymbol symbol, CancellationToken cancellationToken)
        {
            if (symbol.IsPropertyOrEventAccessor() || symbol.IsUnitTestMethod() || symbol.IsSynthesized())
            {
                return false;
            }

            return !symbol.IsOverride && !symbol.HidesBaseMember(cancellationToken) && !symbol.IsInterfaceImplementation();
        }

        private static bool ContainsBlacklistedWord([NotNull] string name)
        {
            return name.ContainsWordInTheMiddle(BlacklistWord);
        }
    }
}
