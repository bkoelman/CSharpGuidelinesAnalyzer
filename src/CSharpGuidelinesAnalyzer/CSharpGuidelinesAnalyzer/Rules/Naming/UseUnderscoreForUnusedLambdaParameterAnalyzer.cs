using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseUnderscoreForUnusedLambdaParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Unused lambda parameter should be renamed to underscore(s)";
    private const string MessageFormat = "Unused {0} parameter '{1}' should be renamed to underscore(s)";
    private const string Description = "Use an underscore for irrelevant lambda parameters.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1739";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly SyntaxKind[] AnonymousFunctionKinds =
    [
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.AnonymousMethodExpression
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeAnonymousFunction, AnonymousFunctionKinds);
    }

    private static void AnalyzeAnonymousFunction(SyntaxNodeAnalysisContext context)
    {
        var anonymousFunction = (AnonymousFunctionExpressionSyntax)context.Node;

        if (anonymousFunction.Body == null)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(anonymousFunction, context.CancellationToken).Symbol is IMethodSymbol method)
        {
            if (method.Parameters.Any(IsRegularParameter))
            {
                AnalyzeParameterUsage(method.Parameters, anonymousFunction.Body, context);
            }
        }
    }

    private static void AnalyzeParameterUsage(ImmutableArray<IParameterSymbol> parameters, SyntaxNode bodySyntax,
        SyntaxNodeAnalysisContext context)
    {
        DataFlowAnalysis? dataFlowAnalysis = TryAnalyzeDataFlow(bodySyntax, context.SemanticModel);

        if (dataFlowAnalysis == null)
        {
            return;
        }

        foreach (IParameterSymbol parameter in parameters)
        {
            if (IsRegularParameter(parameter) && !IsParameterUsed(parameter, dataFlowAnalysis))
            {
                string functionKind = context.Node is AnonymousMethodExpressionSyntax ? "anonymous method" : "lambda";

                var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], functionKind, parameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsRegularParameter(IParameterSymbol parameter)
    {
        return !parameter.IsSynthesized() && !ConsistsOfUnderscoresOnly(parameter.Name);
    }

    private static bool ConsistsOfUnderscoresOnly(string identifierName)
    {
        foreach (char ch in identifierName)
        {
            if (ch != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static DataFlowAnalysis? TryAnalyzeDataFlow(SyntaxNode bodySyntax, SemanticModel semanticModel)
    {
        return semanticModel.SafeAnalyzeDataFlow(bodySyntax);
    }

    private static bool IsParameterUsed(IParameterSymbol parameter, DataFlowAnalysis dataFlowAnalysis)
    {
        return dataFlowAnalysis.ReadInside.Contains(parameter) || dataFlowAnalysis.WrittenInside.Contains(parameter) ||
            dataFlowAnalysis.Captured.Contains(parameter);
    }
}
