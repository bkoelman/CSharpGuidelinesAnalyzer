using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticClassesShouldOnlyContainExtensionMethodsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1745";

        private const string Title = "Name of extension method container class should end with 'Extensions'";
        private const string MessageFormat = "Name of extension method container class '{0}' should end with 'Extensions'.";
        private const string Description = "Group extension methods in a class suffixed with Extensions.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if (IsExtensionMethodContainer(type) && !type.Name.EndsWith("Extensions", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.Name));
            }
        }

        private static bool IsExtensionMethodContainer([NotNull] INamedTypeSymbol type)
        {
            if (!type.IsStatic || type.IsGenericType)
            {
                return false;
            }

            IMethodSymbol[] accessibleMethods = type.GetMembers().OfType<IMethodSymbol>().Where(IsPublicOrInternal).ToArray();
            bool hasRegularAccessibleMethods = accessibleMethods.Any(method => !method.IsExtensionMethod);

            return !hasRegularAccessibleMethods && accessibleMethods.Any();
        }

        private static bool IsPublicOrInternal([NotNull] IMethodSymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Public || method.DeclaredAccessibility == Accessibility.Internal;
        }
    }
}
