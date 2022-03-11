using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseOptionalParameterInTypeHierarchyAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Method contains optional parameter in type hierarchy";
        private const string MessageFormat = "Method '{0}' contains optional parameter '{1}'";
        private const string Description = "Do not use optional parameters in interface methods or their concrete implementations.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1554";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeParameterAction = context => context.SkipEmptyName(AnalyzeParameter);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeParameterAction, SyntaxKind.Parameter);
        }

        private static void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.IsOptional)
            {
                INamedTypeSymbol type = parameter.ContainingType;
                ISymbol method = parameter.ContainingSymbol;

                if (type.TypeKind == TypeKind.Interface || method.IsInterfaceImplementation() || method.IsAbstract || method.IsVirtual || method.IsOverride)
                {
                    string containerName = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                    SyntaxReference syntaxReference = parameter.DeclaringSyntaxReferences.First();
                    var location = Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);

                    var diagnostic = Diagnostic.Create(Rule, location, containerName, parameter.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
