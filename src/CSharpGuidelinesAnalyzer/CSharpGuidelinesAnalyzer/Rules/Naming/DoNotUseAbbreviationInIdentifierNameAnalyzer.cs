using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseAbbreviationInIdentifierNameAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1706";

        private const string Title =
            "Member, local function, parameter or variable name contains an abbreviation or is too short";

        private const string MessageFormat = "{0} '{1}' should have a more descriptive name.";
        private const string Description = "Don't use abbreviations.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

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

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclarator), OperationKind.VariableDeclarator);
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

            if (requiresReport)
            {
                if (!parameter.IsInterfaceImplementation())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
                }
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
