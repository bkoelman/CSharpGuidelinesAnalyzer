using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidBooleanParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Parameter in public or internal member is of type bool or bool?";
    private const string MessageFormat = "Parameter '{0}' is of type '{1}'";
    private const string Description = "Avoid signatures that take a bool parameter.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1564";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameter = (IParameterSymbol)context.Symbol;

        if (parameter.ContainingSymbol.IsDeconstructor() || parameter.IsSynthesized())
        {
            return;
        }

        if (IsParameterAccessible(parameter) && parameter.Type.IsBooleanOrNullableBoolean())
        {
            AnalyzeBooleanParameter(parameter, context);
        }
    }

    private static bool IsParameterAccessible([NotNull] IParameterSymbol parameter)
    {
        ISymbol containingMember = parameter.ContainingSymbol;

        return containingMember.DeclaredAccessibility != Accessibility.Private && containingMember.IsSymbolAccessibleFromRoot();
    }

    private static void AnalyzeBooleanParameter([NotNull] IParameterSymbol parameter, SymbolAnalysisContext context)
    {
        ISymbol containingMember = parameter.ContainingSymbol;

        if (!containingMember.IsOverride && !containingMember.HidesBaseMember(context.CancellationToken) && !parameter.IsInterfaceImplementation() &&
            !IsDisposablePattern(parameter))
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, parameter.Type);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsDisposablePattern([NotNull] IParameterSymbol parameter)
    {
        if (parameter is { Name: "disposing", ContainingSymbol: IMethodSymbol { Name: "Dispose" } containingMethod })
        {
            if (containingMethod.IsVirtual && containingMethod.DeclaredAccessibility == Accessibility.Protected)
            {
                return true;
            }
        }

        return false;
    }
}
