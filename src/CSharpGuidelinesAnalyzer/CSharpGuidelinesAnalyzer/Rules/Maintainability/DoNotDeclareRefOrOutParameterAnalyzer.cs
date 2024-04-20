using System;
using System.Collections.Immutable;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotDeclareRefOrOutParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Do not declare a parameter as ref or out";
    private const string MessageFormat = "Parameter '{0}' is declared as ref or out";
    private const string Description = "Don't use ref or out parameters.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1562";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [CanBeNull]
    private static readonly PropertyInfo IsRefLikeTypeProperty = typeof(ITypeSymbol).GetRuntimeProperty("IsRefLikeType");

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

        if (parameter.ContainingSymbol.IsDeconstructor() || parameter.IsSynthesized())
        {
            return;
        }

        if (!IsRefOrOutParameter(parameter) || IsOutParameterInTryMethod(parameter))
        {
            return;
        }

        AnalyzeRefParameter(parameter, context);
    }

    private static bool IsRefOrOutParameter([NotNull] IParameterSymbol parameter)
    {
        return parameter.RefKind is RefKind.Ref or RefKind.Out;
    }

    private static bool IsOutParameterInTryMethod([NotNull] IParameterSymbol parameter)
    {
        return parameter is { RefKind: RefKind.Out, ContainingSymbol: IMethodSymbol method } && method.Name.StartsWith("Try", StringComparison.Ordinal);
    }

    private static void AnalyzeRefParameter([NotNull] IParameterSymbol parameter, SymbolAnalysisContext context)
    {
        if (IsRefStruct(parameter.Type))
        {
            return;
        }

        ISymbol containingMember = parameter.ContainingSymbol;

        if (!containingMember.IsOverride && !containingMember.HidesBaseMember(context.CancellationToken) && !parameter.IsInterfaceImplementation())
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsRefStruct([NotNull] ITypeSymbol type)
    {
        return IsRefLikeTypeProperty != null && type.TypeKind == TypeKind.Struct && (bool)IsRefLikeTypeProperty.GetValue(type);
    }
}