using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotAssignToParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Parameter value should not be overwritten in method body";
    private const string MessageFormat = "The value of parameter '{0}' is overwritten in its method body";
    private const string Description = "Don't use parameters as temporary variables.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1568";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SpecialType> SimpleTypes = ImmutableArray.Create(SpecialType.System_Boolean, SpecialType.System_Char,
        SpecialType.System_SByte, SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_UInt16, SpecialType.System_Int32,
        SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Decimal, SpecialType.System_Single,
        SpecialType.System_Double, SpecialType.System_IntPtr, SpecialType.System_UIntPtr, SpecialType.System_DateTime);

    private static readonly ImmutableArray<SyntaxKind> ParameterSyntaxKinds = [SyntaxKind.Parameter];

    private static readonly ImmutableArray<SyntaxKind> PropertyIndexerEventSyntaxKinds =
    [
        SyntaxKind.PropertyDeclaration,
        SyntaxKind.IndexerDeclaration,
        SyntaxKind.EventDeclaration
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeParameter, ParameterSyntaxKinds);
        context.RegisterSyntaxNodeAction(AnalyzePropertyOrIndexerOrEvent, PropertyIndexerEventSyntaxKinds);
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is IParameterSymbol parameterSymbol)
        {
            if (parameterSymbol is { RefKind: RefKind.None, ContainingSymbol: IMethodSymbol { IsAbstract: false } containingMethod } &&
                !containingMethod.IsSynthesized())
            {
                SyntaxNode? bodySyntax = containingMethod.TryGetBodySyntaxForMethod(context.CancellationToken);

                if (bodySyntax != null)
                {
                    using var collector = new DiagnosticCollector(context.ReportDiagnostic);
                    AnalyzeDataFlow([parameterSymbol], bodySyntax, collector, context);
                }
            }
        }
    }

    private static void AnalyzePropertyOrIndexerOrEvent(SyntaxNodeAnalysisContext context)
    {
        var propertySyntax = (BasePropertyDeclarationSyntax)context.Node;
        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        if (context.SemanticModel.GetDeclaredSymbol(propertySyntax) is IPropertySymbol propertySymbol)
        {
            AnalyzeAccessorMethod(propertySymbol.GetMethod, collector, context);
            AnalyzeAccessorMethod(propertySymbol.SetMethod, collector, context);
        }
        else if (context.SemanticModel.GetDeclaredSymbol(propertySyntax) is IEventSymbol eventSymbol)
        {
            AnalyzeAccessorMethod(eventSymbol.AddMethod, collector, context);
            AnalyzeAccessorMethod(eventSymbol.RemoveMethod, collector, context);
        }
    }

    private static void AnalyzeAccessorMethod(IMethodSymbol? accessorMethod, DiagnosticCollector collector, SyntaxNodeAnalysisContext context)
    {
        if (accessorMethod is { Parameters.Length: > 0 } && !accessorMethod.IsSynthesized())
        {
            SyntaxNode? bodySyntax = accessorMethod.TryGetBodySyntaxForMethod(context.CancellationToken);

            if (bodySyntax != null)
            {
                AnalyzeDataFlow(accessorMethod.Parameters, bodySyntax, collector, context);
            }
        }
    }

    private static void AnalyzeDataFlow(ImmutableArray<IParameterSymbol> parameterSymbols, SyntaxNode bodySyntax, DiagnosticCollector collector,
        SyntaxNodeAnalysisContext context)
    {
        if (IsTypeUserDefinedStruct(parameterSymbols[0]))
        {
            // A user-defined struct can reassign its 'this' parameter on invocation. That's why the compiler dataflow
            // analysis reports all access as writes. Because that's not very practical, we run our own assignment analysis.

            CustomAnalyzeDataFlow(parameterSymbols, bodySyntax, collector, context);
        }
        else
        {
            DataFlowAnalysis? dataFlowAnalysis = context.SemanticModel.SafeAnalyzeDataFlow(bodySyntax);

            if (dataFlowAnalysis != null)
            {
                foreach (IParameterSymbol? parameterSymbol in parameterSymbols)
                {
                    if (dataFlowAnalysis.WrittenInside.Contains(parameterSymbol))
                    {
                        var diagnostic = Diagnostic.Create(Rule, parameterSymbol.Locations[0], parameterSymbol.Name);
                        collector.Add(diagnostic);
                    }
                }
            }
        }
    }

    private static void CustomAnalyzeDataFlow(ImmutableArray<IParameterSymbol> parameterSymbols, SyntaxNode bodySyntax, DiagnosticCollector collector,
        SyntaxNodeAnalysisContext context)
    {
        IOperation? bodyOperation = context.SemanticModel.GetOperation(bodySyntax);

        if (bodyOperation == null || bodyOperation.HasErrors(context.Compilation, context.CancellationToken))
        {
            return;
        }

        var walker = new AssignmentWalker(parameterSymbols);
        walker.Visit(bodyOperation);

        foreach (IParameterSymbol parameter in walker.ParametersAssigned)
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name);
            collector.Add(diagnostic);
        }
    }

    private static bool IsTypeUserDefinedStruct(IParameterSymbol parameter)
    {
        return parameter.Type.TypeKind == TypeKind.Struct && !IsSimpleType(parameter.Type);
    }

    private static bool IsSimpleType(ITypeSymbol type)
    {
        return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T || SimpleTypes.Contains(type.SpecialType);
    }

    private sealed class AssignmentWalker : ExplicitOperationWalker
    {
        private readonly IDictionary<IParameterSymbol, bool> seenAssignmentPerParameter =
            new Dictionary<IParameterSymbol, bool>(SymbolEqualityComparer.IncludeNullability);

        public ICollection<IParameterSymbol> ParametersAssigned => seenAssignmentPerParameter.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();

        public AssignmentWalker(ICollection<IParameterSymbol> parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            foreach (IParameterSymbol parameter in parameters)
            {
                seenAssignmentPerParameter[parameter] = false;
            }
        }

        public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitSimpleAssignment(operation);
        }

        public override void VisitCompoundAssignment(ICompoundAssignmentOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitCompoundAssignment(operation);
        }

        public override void VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitIncrementOrDecrement(operation);
        }

        public override void VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation)
        {
            if (operation.Target is ITupleOperation tuple)
            {
                foreach (IOperation element in tuple.Elements)
                {
                    RegisterAssignmentToParameter(element);
                }
            }

            base.VisitDeconstructionAssignment(operation);
        }

        public override void VisitCoalesceAssignment(ICoalesceAssignmentOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitCoalesceAssignment(operation);
        }

        public override void VisitArgument(IArgumentOperation operation)
        {
            if (operation.Parameter?.RefKind is RefKind.Ref or RefKind.Out)
            {
                RegisterAssignmentToParameter(operation.Value);
            }

            base.VisitArgument(operation);
        }

        private void RegisterAssignmentToParameter(IOperation operation)
        {
            if (operation is IParameterReferenceOperation parameterReference)
            {
                IParameterSymbol parameter = parameterReference.Parameter;

                if (seenAssignmentPerParameter.ContainsKey(parameter))
                {
                    seenAssignmentPerParameter[parameter] = true;
                }
            }
        }
    }
}
