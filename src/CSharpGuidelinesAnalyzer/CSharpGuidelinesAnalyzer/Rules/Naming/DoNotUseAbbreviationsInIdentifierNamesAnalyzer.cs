using System.Collections.Immutable;
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
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            new[] { SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event }.ToImmutableArray();

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist =
            new[]
            {
                "Btn",
                "Ctrl",
                "Frm",
                "Chk",
                "Cmb",
                "Ctx",
                "Dg",
                "Pnl",
                "Dlg",
                "Lbl",
                "Txt",
                "Mnu",
                "Prg",
                "Rb",
                "Cnt",
                "Tv",
                "Ddl",
                "Fld",
                "Lnk",
                "Img",
                "Lit",
                "Vw",
                "Gv",
                "Dts",
                "Rpt",
                "Vld",
                "Pwd",
                "Ctl",
                "Tm",
                "Mgr",
                "Flt",
                "Len",
                "Idx",
                "Str"
            }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
            context.RegisterSyntaxNodeAction(c => AnalyzeParameter(c.ToSymbolContext()), SyntaxKind.Parameter);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(AnalyzeVariableDeclaration, OperationKind.VariableDeclaration);
                }
            });
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor())
            {
                return;
            }

            if (NameRequiresReport(context.Symbol.Name, false) && !context.Symbol.IsOverride &&
                !context.Symbol.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                    context.Symbol.Name));
            }
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (parameter.Name.Length == 0)
            {
                return;
            }

            bool isInLambdaExpression = IsInLambdaExpression(parameter);

            if (NameRequiresReport(parameter.Name, isInLambdaExpression) && !parameter.ContainingSymbol.IsOverride &&
                !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }
        }

        private static bool IsInLambdaExpression([NotNull] IParameterSymbol parameter)
        {
            var method = parameter.ContainingSymbol as IMethodSymbol;
            return method != null && method.MethodKind == MethodKind.LambdaMethod;
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration) context.Operation;

            if (NameRequiresReport(declaration.Variable.Name, false))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Variable.Locations[0], "Variable",
                    declaration.Variable.Name));
            }
        }

        private bool NameRequiresReport([NotNull] string identifierName, bool allowSingleLetter)
        {
            bool isSingleLetter = identifierName.Length == 1 && char.IsLetter(identifierName[0]);
            bool isBlacklisted = identifierName.GetFirstWordInSetFromIdentifier(WordsBlacklist, true) != null;

            return allowSingleLetter ? isBlacklisted : isSingleLetter || isBlacklisted;
        }
    }
}