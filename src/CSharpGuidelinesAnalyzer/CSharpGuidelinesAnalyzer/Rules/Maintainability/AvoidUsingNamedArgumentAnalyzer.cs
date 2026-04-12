using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
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

    private static IDictionary<IParameterSymbol, bool> GetParameterUsageMap(IInvocationOperation invocation)
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
            !argument.IsImplicit && argument.Parameter != null && parameterUsageMap.ContainsKey(argument.Parameter)))
        {
            if (argumentInMap.Parameter != null)
            {
                parameterUsageMap[argumentInMap.Parameter] = true;
            }
        }

        return parameterUsageMap;
    }

    private static bool RequiresReport(IArgumentOperation argument, IInvocationOperation invocation, IDictionary<IParameterSymbol, bool> parameterUsageMap)
    {
        if (RequiresAnalysis(argument) && argument.Parameter != null)
        {
            ICollection<IParameterSymbol> precedingParameters = GetPrecedingParameters(argument.Parameter, invocation.TargetMethod);

            if (AreParametersUsed(precedingParameters, parameterUsageMap))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RequiresAnalysis(IArgumentOperation argument)
    {
        return !argument.IsImplicit && argument.Parameter != null && !argument.Parameter.Type.IsBooleanOrNullableBoolean() && IsNamedArgument(argument);
    }

    private static bool IsNamedArgument(IArgumentOperation argument)
    {
        var syntax = argument.Syntax as ArgumentSyntax;
        return syntax?.NameColon != null;
    }

    private static ICollection<IParameterSymbol> GetPrecedingParameters(IParameterSymbol parameter, IMethodSymbol method)
    {
        return method.Parameters.TakeWhile(nextParameter => !nextParameter.IsEqualTo(parameter)).ToList();
    }

    private static bool AreParametersUsed(ICollection<IParameterSymbol> parameters, IDictionary<IParameterSymbol, bool> parameterUsageMap)
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

    private static void ReportArgument(IArgumentOperation argument, Action<Diagnostic> reportDiagnostic)
    {
        var syntax = (ArgumentSyntax)argument.Syntax;

        if (argument.Parameter != null && syntax.NameColon != null)
        {
            string? methodText = argument.Parameter.NullableContainingSymbol?.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            if (methodText != null)
            {
                Location location = syntax.NameColon.GetLocation();

                var diagnostic = Diagnostic.Create(Rule, location, argument.Parameter.Name, methodText);
                reportDiagnostic(diagnostic);
            }
        }
    }
}
