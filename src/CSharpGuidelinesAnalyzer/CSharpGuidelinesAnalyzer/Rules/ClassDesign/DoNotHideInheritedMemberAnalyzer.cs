using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotHideInheritedMemberAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Member hides inherited member";
    private const string MessageFormat = "'{0}' hides inherited member";
    private const string Description = "Don't suppress compiler warnings using the new keyword.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1010";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds = ImmutableArray.Create(SymbolKind.Field,
        SymbolKind.Property, SymbolKind.Method, SymbolKind.Event, SymbolKind.NamedType);

    [NotNull]
    private static readonly Action<SymbolAnalysisContext> AnalyzeMemberAction = context => context.SkipEmptyName(AnalyzeMember);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzeMemberAction, MemberSymbolKinds);
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        if (context.Symbol is INamedTypeSymbol { ContainingType: null })
        {
            return;
        }

        if (context.Symbol.IsPropertyOrEventAccessor() || context.Symbol.IsSynthesized())
        {
            return;
        }

        if (!context.Symbol.IsOverride && context.Symbol.HidesBaseMember(context.CancellationToken))
        {
            string memberName = context.Symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], memberName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}