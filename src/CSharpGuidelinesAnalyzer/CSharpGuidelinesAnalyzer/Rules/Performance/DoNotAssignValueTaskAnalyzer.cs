using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotAssignValueTaskAnalyzer : DiagnosticAnalyzer
{
    private const SyntaxKind CoalesceAssignmentExpressionSyntaxKind = (SyntaxKind)8725;

    private const string Title = "ValueTask should be awaited before assignment";
    private const string AssignmentMessageFormat = "Assignment of ValueTask without await to '{0}'";
    private const string ArgumentMessageFormat = "Usage of ValueTask without await on parameter '{0}' of '{1}'";
    private const string Description = "Await ValueTask and ValueTask<T> directly and exactly once.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1840";

    private static readonly SyntaxKind[] AssignmentSyntaxKinds =
    [
        SyntaxKind.SimpleAssignmentExpression,
        CoalesceAssignmentExpressionSyntaxKind
    ];

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Performance;

    private static readonly DiagnosticDescriptor AssignmentRule = new(DiagnosticId, Title, AssignmentMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor ArgumentRule = new(DiagnosticId, Title, ArgumentMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssignmentRule, ArgumentRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        IList<INamedTypeSymbol> valueTaskTypes = ResolveValueTaskTypes(startContext.Compilation).ToList();

        if (valueTaskTypes.Any())
        {
            startContext.RegisterSyntaxNodeAction(context => AnalyzeInitializer(context, valueTaskTypes), SyntaxKind.EqualsValueClause);
            startContext.RegisterSyntaxNodeAction(context => AnalyzeAssignment(context, valueTaskTypes), AssignmentSyntaxKinds);
            startContext.RegisterSyntaxNodeAction(context => AnalyzeArgument(context, valueTaskTypes), SyntaxKind.Argument);
        }
    }

    private static IEnumerable<INamedTypeSymbol> ResolveValueTaskTypes(Compilation compilation)
    {
        foreach (INamedTypeSymbol? taskType in new[]
        {
            KnownTypes.SystemThreadingTasksValueTask(compilation),
            KnownTypes.SystemThreadingTasksValueTaskT(compilation),
            KnownTypes.SystemRuntimeCompilerServicesConfiguredValueTaskAwaitable(compilation),
            KnownTypes.SystemRuntimeCompilerServicesConfiguredValueTaskAwaitableT(compilation)
        })
        {
            if (taskType != null)
            {
                yield return taskType;
            }
        }
    }

    private static void AnalyzeInitializer(SyntaxNodeAnalysisContext context, IList<INamedTypeSymbol> valueTaskTypes)
    {
        var equalsValueClause = (EqualsValueClauseSyntax)context.Node;

        IOperation operation = context.SemanticModel.GetOperation(equalsValueClause, context.CancellationToken);

        if (operation != null)
        {
            AssignmentInfo? assignmentInfo = TryGetAssignmentInfoFromOperation(operation, equalsValueClause);

            if (assignmentInfo != null)
            {
                AnalyzeRightHandSideType(assignmentInfo, valueTaskTypes, context);
            }
        }
    }

    private static AssignmentInfo? TryGetAssignmentInfoFromOperation(IOperation operation, EqualsValueClauseSyntax equalsValueClause)
    {
        if (operation is ISymbolInitializerOperation symbolInitializer)
        {
            ITypeSymbol rightType = symbolInitializer.Value.SkipTypeConversions().Type;

            if (rightType != null)
            {
                Location location = equalsValueClause.EqualsToken.GetLocation();

                switch (operation)
                {
                    case IPropertyInitializerOperation propertyInitializer:
                    {
                        string leftName = propertyInitializer.InitializedProperties.First().ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                        return new AssignmentInfo(leftName, location, rightType);
                    }
                    case IFieldInitializerOperation fieldInitializer:
                    {
                        string leftName = fieldInitializer.InitializedFields.First().ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                        return new AssignmentInfo(leftName, location, rightType);
                    }
                    case IVariableInitializerOperation:
                    {
                        string leftName = ((VariableDeclaratorSyntax)equalsValueClause.Parent).Identifier.ToString();
                        return new AssignmentInfo(leftName, location, rightType);
                    }
                }
            }
        }

        return null;
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, IList<INamedTypeSymbol> valueTaskTypes)
    {
        var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

        IOperation operation = context.SemanticModel.GetOperation(assignmentExpression.Right, context.CancellationToken);

        if (operation?.Type != null)
        {
            ISymbol leftSymbol = context.SemanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;

            if (leftSymbol != null)
            {
                Location location = assignmentExpression.OperatorToken.GetLocation();
                string leftName = leftSymbol.ToString();
                var assignmentInfo = new AssignmentInfo(leftName, location, operation.Type);

                AnalyzeRightHandSideType(assignmentInfo, valueTaskTypes, context);
            }
        }
    }

    private static void AnalyzeRightHandSideType(AssignmentInfo assignmentInfo, IList<INamedTypeSymbol> valueTaskTypes,
        SyntaxNodeAnalysisContext context)
    {
        if (IsValueTask(assignmentInfo.RightType, valueTaskTypes))
        {
            var diagnostic = Diagnostic.Create(AssignmentRule, assignmentInfo.OperatorLocation, assignmentInfo.LeftName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context, IList<INamedTypeSymbol> valueTaskTypes)
    {
        var argument = (ArgumentSyntax)context.Node;

        if (context.SemanticModel.GetOperation(argument) is IArgumentOperation argumentOperation)
        {
            ITypeSymbol argumentType = argumentOperation.Value.SkipTypeConversions().Type;

            if (IsValueTask(argumentType, valueTaskTypes))
            {
                Location location = argument.GetLocation();
                string parameterName = argumentOperation.Parameter.Name;

                ISymbol method = argumentOperation.Parameter.ContainingSymbol.OriginalDefinition;
                string containerName = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                var diagnostic = Diagnostic.Create(ArgumentRule, location, parameterName, containerName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsValueTask(ITypeSymbol type, IList<INamedTypeSymbol> valueTaskTypes)
    {
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.UnwrapNullableValueType() is INamedTypeSymbol unwrappedType)
            {
                return valueTaskTypes.Any(valueTaskType => valueTaskType.IsEqualTo(unwrappedType.ConstructedFrom));
            }
        }

        return false;
    }

    private sealed class AssignmentInfo
    {
        public string LeftName { get; }

        public Location OperatorLocation { get; }

        public ITypeSymbol RightType { get; }

        public AssignmentInfo(string leftName, Location operatorLocation, ITypeSymbol rightType)
        {
            ArgumentNullException.ThrowIfNull(leftName);
            ArgumentNullException.ThrowIfNull(operatorLocation);
            ArgumentNullException.ThrowIfNull(rightType);

            LeftName = leftName;
            OperatorLocation = operatorLocation;
            RightType = rightType;
        }
    }
}
