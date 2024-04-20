using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidMisleadingNameAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Identifier name is difficult to read";
    private const string MessageFormat = "{0} '{1}' has a name that is difficult to read";
    private const string Description = "Avoid short names or names that can be mistaken for other names.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1712";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, false, Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    private static readonly ImmutableArray<string> Blacklist = ImmutableArray.Create("b001", "lo", "I1", "lOl");

    [NotNull]
    private static readonly Action<OperationAnalysisContext> AnalyzeVariableDeclaratorAction = context => context.SkipInvalid(AnalyzeVariableDeclarator);

    [NotNull]
    private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeParameterAction = context => context.SkipEmptyName(AnalyzeParameter);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeVariableDeclaratorAction, OperationKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeParameterAction, SyntaxKind.Parameter);
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext context)
    {
        var declarator = (IVariableDeclaratorOperation)context.Operation;
        ILocalSymbol variable = declarator.Symbol;

        if (Blacklist.Contains(variable.Name) && !variable.IsSynthesized())
        {
            var diagnostic = Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameter = (IParameterSymbol)context.Symbol;

        if (Blacklist.Contains(parameter.Name) && !parameter.IsSynthesized())
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}