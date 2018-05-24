using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseAbbreviationInIdentifierNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1706";

        private const string Title = "Identifier contains an abbreviation or is too short";
        private const string MessageFormat = "{0} '{1}' should have a more descriptive name.";
        private const string Description = "Don't use abbreviations.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("Btn", "Ctrl", "Frm", "Chk", "Cmb",
            "Ctx", "Dg", "Pnl", "Dlg", "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw",
            "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt", "Len", "Idx", "Str");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclarator), OperationKind.VariableDeclarator);

            context.RegisterSyntaxNodeAction(AnalyzeFromClause, SyntaxKind.FromClause);
            context.RegisterSyntaxNodeAction(AnalyzeJoinClause, SyntaxKind.JoinClause);
            context.RegisterSyntaxNodeAction(AnalyzeJoinIntoClause, SyntaxKind.JoinIntoClause);
            context.RegisterSyntaxNodeAction(AnalyzeQueryContinuation, SyntaxKind.QueryContinuation);
            context.RegisterSyntaxNodeAction(AnalyzeLetClause, SyntaxKind.LetClause);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.IsSynthesized())
            {
                return;
            }

            if (IsBlacklisted(type.Name) || IsSingleLetter(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.TypeKind, type.Name));
            }
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor() || context.Symbol.IsOverride || context.Symbol.IsSynthesized())
            {
                return;
            }

            if (IsBlacklisted(context.Symbol.Name) || IsSingleLetter(context.Symbol.Name))
            {
                if (!context.Symbol.IsInterfaceImplementation())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.GetKind(),
                        context.Symbol.Name));
                }
            }
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var localFunction = (ILocalFunctionOperation)context.Operation;

            if (IsBlacklisted(localFunction.Symbol.Name) || IsSingleLetter(localFunction.Symbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, localFunction.Symbol.Locations[0],
                    localFunction.Symbol.GetKind(), localFunction.Symbol.Name));
            }
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.ContainingSymbol.IsOverride || parameter.IsSynthesized())
            {
                return;
            }

            bool requiresReport = IsInLambdaExpression(parameter)
                ? IsBlacklisted(parameter.Name)
                : IsBlacklisted(parameter.Name) || IsSingleLetter(parameter.Name);

            if (requiresReport && !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }
        }

        private static bool IsInLambdaExpression([NotNull] IParameterSymbol parameter)
        {
            return parameter.ContainingSymbol is IMethodSymbol method && method.MethodKind == MethodKind.LambdaMethod;
        }

        private void AnalyzeVariableDeclarator(OperationAnalysisContext context)
        {
            var declarator = (IVariableDeclaratorOperation)context.Operation;
            ILocalSymbol variable = declarator.Symbol;

            if (!string.IsNullOrWhiteSpace(variable.Name) && !variable.IsSynthesized())
            {
                if (IsBlacklisted(variable.Name) || IsSingleLetter(variable.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name));
                }
            }
        }

        private void AnalyzeFromClause(SyntaxNodeAnalysisContext context)
        {
            var fromClause = (FromClauseSyntax)context.Node;
            AnalyzeRangeVariable(fromClause.Identifier, context);
        }

        private void AnalyzeJoinClause(SyntaxNodeAnalysisContext context)
        {
            var joinClause = (JoinClauseSyntax)context.Node;
            AnalyzeRangeVariable(joinClause.Identifier, context);
        }

        private void AnalyzeJoinIntoClause(SyntaxNodeAnalysisContext context)
        {
            var joinIntoClause = (JoinIntoClauseSyntax)context.Node;
            AnalyzeRangeVariable(joinIntoClause.Identifier, context);
        }

        private void AnalyzeQueryContinuation(SyntaxNodeAnalysisContext context)
        {
            var queryContinuation = (QueryContinuationSyntax)context.Node;
            AnalyzeRangeVariable(queryContinuation.Identifier, context);
        }

        private void AnalyzeLetClause(SyntaxNodeAnalysisContext context)
        {
            var letClause = (LetClauseSyntax)context.Node;
            AnalyzeRangeVariable(letClause.Identifier, context);
        }

        private static void AnalyzeRangeVariable(SyntaxToken identifierToken, SyntaxNodeAnalysisContext context)
        {
            string rangeVariableName = identifierToken.ValueText;

            if (IsBlacklisted(rangeVariableName) || IsSingleLetter(rangeVariableName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, identifierToken.GetLocation(), "Range variable",
                    rangeVariableName));
            }
        }

        private static bool IsBlacklisted([NotNull] string name)
        {
            return name.GetWordsInList(WordsBlacklist).Any();
        }

        private static bool IsSingleLetter([NotNull] string name)
        {
            return name.Length == 1 && char.IsLetter(name[0]);
        }
    }
}
