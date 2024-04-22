using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
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
        else if (argumentValue is IObjectCreationOperation objectCreation)
        {
            string innerName = objectCreation.Constructor.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            ReportAt(argument, innerName, context);
        }
    }

    private static bool IsThisArgumentInExtensionMethod([NotNull] IArgumentOperation argument)
    {
        if (argument.Parameter.ContainingSymbol is IMethodSymbol { IsExtensionMethod: true } method)
        {
            IParameterSymbol thisParameter = method.Parameters.FirstOrDefault();

            if (thisParameter != null && argument.Parameter.IsEqualTo(thisParameter))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInFieldOrConstructorInitializer([NotNull] IArgumentOperation argument)
    {
        IOperation parent = argument.Parent;

        while (parent != null)
        {
            if (parent is IFieldInitializerOperation)
            {
                return true;
            }

            // IConstructorBodyOperation is unavailable in the version of Microsoft.CodeAnalysis we depend on.
            if (parent.GetType().ToString() == "Microsoft.CodeAnalysis.Operations.ConstructorBodyOperation" &&
                IsConstructor(argument.Parameter.ContainingSymbol))
            {
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static bool IsConstructor([NotNull] ISymbol symbol)
    {
        if (symbol is IMethodSymbol method)
        {
            return method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
        }

        return false;
    }

    private static bool IsObjectOrCollectionInitializer([NotNull] IArgumentOperation argument)
    {
        return argument.Parent?.Parent is IObjectOrCollectionInitializerOperation;
    }

    private static void ReportAt([NotNull] IArgumentOperation argument, [NotNull] string innerName, OperationAnalysisContext context)
    {
        string outerName = argument.Parameter.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        Location location = argument.Value.Syntax.GetLocation();

        var diagnostic = Diagnostic.Create(Rule, location, argument.Parameter.Name, outerName, innerName);
        context.ReportDiagnostic(diagnostic);
    }
}
