using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
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

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SpecialType> SimpleTypes = ImmutableArray.Create(SpecialType.System_Boolean, SpecialType.System_Char,
        SpecialType.System_SByte, SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_UInt16, SpecialType.System_Int32,
        SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Decimal, SpecialType.System_Single,
        SpecialType.System_Double, SpecialType.System_IntPtr, SpecialType.System_UIntPtr, SpecialType.System_DateTime);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.SafeRegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        context.SafeRegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
        context.SafeRegisterOperationAction(AnalyzeLocalFunction, OperationKind.LocalFunction);
        context.SafeRegisterOperationAction(AnalyzeAnonymousFunction, OperationKind.AnonymousFunction);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (ShouldSkip(method) || method.IsPropertyOrEventAccessor())
        {
            return;
        }

        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        BaseAnalysisContext<IMethodSymbol> methodContext = context.Wrap(method);
        InnerAnalyzeMethod(methodContext, collector);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;

        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        AnalyzeAccessorMethod(property.GetMethod, collector, context);
        AnalyzeAccessorMethod(property.SetMethod, collector, context);

        FilterDuplicateLocations(collector.Diagnostics);
    }

    private static void AnalyzeEvent(SymbolAnalysisContext context)
    {
        var @event = (IEventSymbol)context.Symbol;

        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        AnalyzeAccessorMethod(@event.AddMethod, collector, context);
        AnalyzeAccessorMethod(@event.RemoveMethod, collector, context);

        FilterDuplicateLocations(collector.Diagnostics);
    }

    private static void AnalyzeAccessorMethod([CanBeNull] IMethodSymbol accessorMethod, [NotNull] DiagnosticCollector collector, SymbolAnalysisContext context)
    {
        if (accessorMethod == null || ShouldSkip(accessorMethod))
        {
            return;
        }

        BaseAnalysisContext<IMethodSymbol> methodContext = context.Wrap(accessorMethod);
        InnerAnalyzeMethod(methodContext, collector);
    }

    private static void FilterDuplicateLocations([NotNull] [ItemNotNull] ICollection<Diagnostic> diagnostics)
    {
        while (true)
        {
            if (!RemoveNextDuplicate(diagnostics))
            {
                return;
            }
        }
    }

    private static bool RemoveNextDuplicate([NotNull] [ItemNotNull] ICollection<Diagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in diagnostics)
        {
            Diagnostic[] duplicates = diagnostics.Where(nextDiagnostic =>
                !ReferenceEquals(nextDiagnostic, diagnostic) && nextDiagnostic.Location == diagnostic.Location).ToArray();

            if (duplicates.Any())
            {
                RemoveRange(diagnostics, duplicates);

                return true;
            }
        }

        return false;
    }

    private static void RemoveRange<T>([NotNull] [ItemNotNull] ICollection<T> source, [NotNull] [ItemNotNull] ICollection<T> elementsToRemove)
    {
        foreach (T elementToRemove in elementsToRemove)
        {
            source.Remove(elementToRemove);
        }
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context)
    {
        var localFunction = (ILocalFunctionOperation)context.Operation;

        if (ShouldSkip(localFunction.Symbol))
        {
            return;
        }

        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        BaseAnalysisContext<IMethodSymbol> methodContext = context.Wrap(localFunction.Symbol);
        InnerAnalyzeMethod(methodContext, collector);
    }

    private static void AnalyzeAnonymousFunction(OperationAnalysisContext context)
    {
        var anonymousFunction = (IAnonymousFunctionOperation)context.Operation;

        if (ShouldSkip(anonymousFunction.Symbol))
        {
            return;
        }

        using var collector = new DiagnosticCollector(context.ReportDiagnostic);

        BaseAnalysisContext<IMethodSymbol> methodContext = context.Wrap(anonymousFunction.Symbol);
        InnerAnalyzeMethod(methodContext, collector);
    }

    private static bool ShouldSkip([NotNull] IMethodSymbol method)
    {
        return method.IsAbstract || method.IsSynthesized() || !method.Parameters.Any();
    }

    private static void InnerAnalyzeMethod(BaseAnalysisContext<IMethodSymbol> context, [NotNull] DiagnosticCollector collector)
    {
        SyntaxNode bodySyntax = context.Target.TryGetBodySyntaxForMethod(context.CancellationToken);

        if (bodySyntax == null)
        {
            return;
        }

        BaseAnalysisContext<ImmutableArray<IParameterSymbol>> analysisContext = context.WithTarget(context.Target.Parameters);
        AnalyzeParametersInMethod(analysisContext, bodySyntax, collector);
    }

    private static void AnalyzeParametersInMethod(BaseAnalysisContext<ImmutableArray<IParameterSymbol>> context, [NotNull] SyntaxNode bodySyntax,
        [NotNull] DiagnosticCollector collector)
    {
        IGrouping<bool, IParameterSymbol>[] parameterGrouping = context.Target
            .Where(parameter => parameter.RefKind == RefKind.None && !parameter.IsSynthesized()).GroupBy(IsUserDefinedStruct).ToArray();

        ICollection<IParameterSymbol> ordinaryParameters = parameterGrouping.Where(group => !group.Key).SelectMany(group => group).ToArray();

        if (ordinaryParameters.Any())
        {
            BaseAnalysisContext<ICollection<IParameterSymbol>> analysisContext = context.WithTarget(ordinaryParameters);
            AnalyzeOrdinaryParameters(analysisContext, bodySyntax, collector);
        }

        ICollection<IParameterSymbol> structParameters = parameterGrouping.Where(group => group.Key).SelectMany(group => group).ToArray();

        if (structParameters.Any())
        {
            BaseAnalysisContext<ICollection<IParameterSymbol>> analysisContext = context.WithTarget(structParameters);
            AnalyzeStructParameters(analysisContext, bodySyntax, collector);
        }
    }

    private static bool IsUserDefinedStruct([NotNull] IParameterSymbol parameter)
    {
        return parameter.Type.TypeKind == TypeKind.Struct && !IsSimpleType(parameter.Type);
    }

    private static bool IsSimpleType([NotNull] ITypeSymbol type)
    {
        return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T || SimpleTypes.Contains(type.SpecialType);
    }

    private static void AnalyzeOrdinaryParameters(BaseAnalysisContext<ICollection<IParameterSymbol>> context, [NotNull] SyntaxNode bodySyntax,
        [NotNull] DiagnosticCollector collector)
    {
        DataFlowAnalysis dataFlowAnalysis = TryAnalyzeDataFlow(bodySyntax, context.Compilation);

        if (dataFlowAnalysis == null)
        {
            return;
        }

        foreach (IParameterSymbol parameter in context.Target)
        {
            if (dataFlowAnalysis.WrittenInside.Contains(parameter))
            {
                var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name);
                collector.Add(diagnostic);
            }
        }
    }

    [CanBeNull]
    private static DataFlowAnalysis TryAnalyzeDataFlow([NotNull] SyntaxNode bodySyntax, [NotNull] Compilation compilation)
    {
        SemanticModel model = compilation.GetSemanticModel(bodySyntax.SyntaxTree);
        return model.SafeAnalyzeDataFlow(bodySyntax);
    }

    private static void AnalyzeStructParameters(BaseAnalysisContext<ICollection<IParameterSymbol>> context, [NotNull] SyntaxNode bodySyntax,
        [NotNull] DiagnosticCollector collector)
    {
        // A user-defined struct can reassign its 'this' parameter on invocation. That's why the compiler dataflow
        // analysis reports all access as writes. Because that's not very practical, we run our own assignment analysis.

        SemanticModel model = context.Compilation.GetSemanticModel(bodySyntax.SyntaxTree);
        IOperation bodyOperation = model.GetOperation(bodySyntax);

        if (bodyOperation == null || bodyOperation.HasErrors(context.Compilation, context.CancellationToken))
        {
            return;
        }

        CollectAssignedStructParameters(context.Target, bodyOperation, collector);
    }

    private static void CollectAssignedStructParameters([NotNull] [ItemNotNull] ICollection<IParameterSymbol> parameters, [NotNull] IOperation bodyOperation,
        [NotNull] DiagnosticCollector collector)
    {
        var walker = new AssignmentWalker(parameters);
        walker.Visit(bodyOperation);

        foreach (IParameterSymbol parameter in walker.ParametersAssigned)
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name);
            collector.Add(diagnostic);
        }
    }

    private sealed class AssignmentWalker : ExplicitOperationWalker
    {
        [NotNull]
        private readonly IDictionary<IParameterSymbol, bool> seenAssignmentPerParameter = new Dictionary<IParameterSymbol, bool>();

        [NotNull]
        [ItemNotNull]
        public ICollection<IParameterSymbol> ParametersAssigned => seenAssignmentPerParameter.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();

        public AssignmentWalker([NotNull] [ItemNotNull] ICollection<IParameterSymbol> parameters)
        {
            Guard.NotNull(parameters, nameof(parameters));

            foreach (IParameterSymbol parameter in parameters)
            {
                seenAssignmentPerParameter[parameter] = false;
            }
        }

        public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitSimpleAssignment(operation);
        }

        public override void VisitCompoundAssignment([NotNull] ICompoundAssignmentOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitCompoundAssignment(operation);
        }

        public override void VisitIncrementOrDecrement([NotNull] IIncrementOrDecrementOperation operation)
        {
            RegisterAssignmentToParameter(operation.Target);

            base.VisitIncrementOrDecrement(operation);
        }

        public override void VisitDeconstructionAssignment([NotNull] IDeconstructionAssignmentOperation operation)
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

        public override void VisitArgument([NotNull] IArgumentOperation operation)
        {
            if (operation.Parameter.RefKind is RefKind.Ref or RefKind.Out)
            {
                RegisterAssignmentToParameter(operation.Value);
            }

            base.VisitArgument(operation);
        }

        private void RegisterAssignmentToParameter([NotNull] IOperation operation)
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
