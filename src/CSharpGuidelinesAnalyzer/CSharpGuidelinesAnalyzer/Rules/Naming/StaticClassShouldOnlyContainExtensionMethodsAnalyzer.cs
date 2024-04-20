using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticClassShouldOnlyContainExtensionMethodsAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Name of extension method container class should end with 'Extensions'";
    private const string MessageFormat = "Name of extension method container class '{0}' should end with 'Extensions'";
    private const string Description = "Group extension methods in a class suffixed with Extensions.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1745";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<SymbolAnalysisContext> AnalyzeNamedTypeAction = context => context.SkipEmptyName(AnalyzeNamedType);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzeNamedTypeAction, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.IsSynthesized())
        {
            return;
        }

        if (IsExtensionMethodContainer(type) && !type.Name.EndsWith("Extensions", StringComparison.Ordinal))
        {
            var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
            context.ReportDiagnostic(diagnostic);
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
        return method.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
    }
}
