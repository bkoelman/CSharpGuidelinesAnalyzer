using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotNestMethodCallsAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Method argument calls a nested method";
    private const string MessageFormat = "Argument for parameter '{0}' in method call to '{1}' calls nested method '{2}'";
    private const string Description = "Write code that is easy to debug.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1580";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
    }

    private static void AnalyzeArgument(OperationAnalysisContext context)
    {
        var argument = (IArgumentOperation)context.Operation;

        if (IsThisArgumentInExtensionMethod(argument) || IsInFieldOrConstructorInitializer(argument) || IsObjectOrCollectionInitializer(argument))
        {
            return;
        }

        IOperation argumentValue = argument.Value.SkipTypeConversions();

        if (argumentValue is IInvocationOperation invocation)
        {
            string innerName = invocation.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            ReportAt(argument, innerName, context);
        }
        else if (argumentValue is IObjectCreationOperation objectCreation && objectCreation.Constructor != null)
        {
            string innerName = objectCreation.Constructor.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            ReportAt(argument, innerName, context);
        }
    }

    private static bool IsThisArgumentInExtensionMethod(IArgumentOperation argument)
    {
        if (argument.Parameter?.ContainingSymbol is IMethodSymbol { IsExtensionMethod: true } method)
        {
            IParameterSymbol? thisParameter = method.Parameters.FirstOrDefault();

            if (thisParameter != null && argument.Parameter.IsEqualTo(thisParameter))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInFieldOrConstructorInitializer(IArgumentOperation argument)
    {
        IOperation? parent = argument.Parent;

        while (parent != null)
        {
            if (parent is IFieldInitializerOperation)
            {
                return true;
            }

            if (argument.Parameter != null && parent is IConstructorBodyOperation &&
                IsConstructor(argument.Parameter.ContainingSymbol))
            {
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static bool IsConstructor(ISymbol symbol)
    {
        if (symbol is IMethodSymbol method)
        {
            return method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
        }

        return false;
    }

    private static bool IsObjectOrCollectionInitializer(IArgumentOperation argument)
    {
        return argument.Parent?.Parent is IObjectOrCollectionInitializerOperation;
    }

    private static void ReportAt(IArgumentOperation argument, string innerName, OperationAnalysisContext context)
    {
        if (argument.Parameter != null)
        {
            string outerName = argument.Parameter.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            Location location = argument.Value.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, argument.Parameter.Name, outerName, innerName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
