using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidBooleanParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1564";

        private const string Title = "Parameter is of type bool or bool?";
        private const string MessageFormat = "Parameter {0} is of type {1}, which should be avoided.";
        private const string Description = "Avoid methods that take a bool flag.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(c => AnalyzeParameter(SyntaxToSymbolContext(c)), SyntaxKind.Parameter);
        }

        private static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext)
        {
            ISymbol symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
            return SyntaxToSymbolContext(syntaxContext, symbol);
        }

        private static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext,
            [NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            return new SymbolAnalysisContext(symbol, syntaxContext.SemanticModel.Compilation, syntaxContext.Options,
                syntaxContext.ReportDiagnostic, x => true, syntaxContext.CancellationToken);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (parameter.Type.SpecialType == SpecialType.System_Boolean || AnalysisUtilities.IsNullableBoolean(parameter.Type))
            {
                AnalyzeBooleanParameter(parameter, context);
            }
        }               

        private void AnalyzeBooleanParameter([NotNull] IParameterSymbol parameter, SymbolAnalysisContext context)
        {
            ISymbol containingMember = parameter.ContainingSymbol;
            if (containingMember.IsOverride)
            {
                return;
            }

            if (HidesBaseMember(containingMember, context.CancellationToken))
            {
                return;
            }

            if (IsInterfaceImplementation(parameter))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, parameter.Type));
        }

        private bool HidesBaseMember([NotNull] ISymbol member, CancellationToken cancellationToken)
        {
            SyntaxNode syntax = member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken);

            var method = syntax as MethodDeclarationSyntax;
            var indexer = syntax as IndexerDeclarationSyntax;

            return ContainsNewModifier(method?.Modifiers ?? indexer?.Modifiers);
        }

        private static bool ContainsNewModifier([CanBeNull] SyntaxTokenList? modifiers)
        {
            return modifiers != null && modifiers.Value.Any(m => m.Kind() == SyntaxKind.NewKeyword);
        }

        private bool IsInterfaceImplementation([NotNull] IParameterSymbol parameter)
        {
            ISymbol containingMember = parameter.ContainingSymbol;

            foreach (INamedTypeSymbol iface in parameter.ContainingType.AllInterfaces)
            {
                foreach (ISymbol ifaceMember in iface.GetMembers())
                {
                    ISymbol implementer = parameter.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                    if (containingMember.Equals(implementer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}