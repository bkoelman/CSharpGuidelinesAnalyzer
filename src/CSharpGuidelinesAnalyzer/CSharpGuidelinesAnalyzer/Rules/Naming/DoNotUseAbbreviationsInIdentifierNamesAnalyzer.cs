using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseAbbreviationsInIdentifierNamesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1706";

        private const string Title = "Member, parameter or variable name contains an abbreviation or is too short";
        private const string MessageFormat = "{0} '{1}' should have a more descriptive name.";
        private const string Description = "Don't use abbreviations.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds = ImmutableArray.Create(SymbolKind.Property,
            SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("Btn", "Ctrl", "Frm", "Chk", "Cmb",
            "Ctx", "Dg", "Pnl", "Dlg", "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw",
            "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt", "Len", "Idx", "Str");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclaration),
                OperationKind.VariableDeclaration);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor() || context.Symbol.IsOverride)
            {
                return;
            }

            if (IsBlacklisted(context.Symbol.Name) || IsSingleLetter(context.Symbol.Name))
            {
                if (!context.Symbol.IsInterfaceImplementation())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                        context.Symbol.Name));
                }
            }
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.ContainingSymbol.IsOverride)
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
            var method = parameter.ContainingSymbol as IMethodSymbol;
            return method != null && method.MethodKind == MethodKind.LambdaMethod;
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration)context.Operation;

            if (!string.IsNullOrWhiteSpace(declaration.Variable.Name))
            {
                if (IsBlacklisted(declaration.Variable.Name) || IsSingleLetter(declaration.Variable.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Variable.Locations[0], "Variable",
                        declaration.Variable.Name));
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
