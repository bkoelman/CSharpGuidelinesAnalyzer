using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUsingNamedArgumentAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Avoid using non-(nullable-)boolean named arguments";
    private const string MessageFormat = "Parameter '{0}' in the call to '{1}' is invoked with a named argument";
    private const string Description = "Avoid using named arguments.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1555";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<OperationAnalysisContext> AnalyzeInvocationAction = context => context.SkipInvalid(AnalyzeInvocation);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeInvocationAction, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        IDictionary<IParameterSymbol, bool> parameterUsageMap = GetParameterUsageMap(invocation);

        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (RequiresReport(argument, invocation, parameterUsageMap))
            {
                ReportArgument(argument, context.ReportDiagnostic);
            }
        }
    }

    [NotNull]
    private static IDictionary<IParameterSymbol, bool> GetParameterUsageMap([NotNull] IInvocationOperation invocation)
    {
        var parameterUsageMap = new Dictionary<IParameterSymbol, bool>();

        foreach (IParameterSymbol parameter in invocation.TargetMethod.Parameters)
        {
            if (parameter.HasExplicitDefaultValue)
            {
                parameterUsageMap.Add(parameter, false);
            }
        }

        foreach (IArgumentOperation argumentInMap in invocation.Arguments.Where(argument =>
            !argument.IsImplicit && parameterUsageMap.ContainsKey(argument.Parameter)))
        {
            parameterUsageMap[argumentInMap.Parameter] = true;
        }

        return parameterUsageMap;
    }

    private static bool RequiresReport([NotNull] IArgumentOperation argument, [NotNull] IInvocationOperation invocation,
        [NotNull] IDictionary<IParameterSymbol, bool> parameterUsageMap)
    {
        if (RequiresAnalysis(argument))
        {
            ICollection<IParameterSymbol> precedingParameters = GetPrecedingParameters(argument.Parameter, invocation.TargetMethod);

            if (AreParametersUsed(precedingParameters, parameterUsageMap))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RequiresAnalysis([NotNull] IArgumentOperation argument)
    {
        return !argument.IsImplicit && !argument.Parameter.Type.IsBooleanOrNullableBoolean() && IsNamedArgument(argument);
    }

    private static bool IsNamedArgument([NotNull] IArgumentOperation argument)
    {
        var syntax = argument.Syntax as ArgumentSyntax;
        return syntax?.NameColon != null;
    }

    [NotNull]
    [ItemNotNull]
    private static ICollection<IParameterSymbol> GetPrecedingParameters([NotNull] IParameterSymbol parameter, [NotNull] IMethodSymbol method)
    {
        return method.Parameters.TakeWhile(nextParameter => !nextParameter.IsEqualTo(parameter)).ToList();
    }

    private static bool AreParametersUsed([NotNull] [ItemNotNull] ICollection<IParameterSymbol> parameters,
        [NotNull] IDictionary<IParameterSymbol, bool> parameterUsageMap)
    {
        foreach (IParameterSymbol parameter in parameters)
        {
            if (!parameter.HasExplicitDefaultValue)
            {
                continue;
            }

            if (!parameterUsageMap.ContainsKey(parameter) || !parameterUsageMap[parameter])
            {
                return false;
            }
        }

        return true;
    }

    private static void ReportArgument([NotNull] IArgumentOperation argument, [NotNull] Action<Diagnostic> reportDiagnostic)
    {
        var syntax = (ArgumentSyntax)argument.Syntax;
        string methodText = argument.Parameter.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        Location location = syntax.NameColon.GetLocation();

        var diagnostic = Diagnostic.Create(Rule, location, argument.Parameter.Name, methodText);
        reportDiagnostic(diagnostic);
    }
}
