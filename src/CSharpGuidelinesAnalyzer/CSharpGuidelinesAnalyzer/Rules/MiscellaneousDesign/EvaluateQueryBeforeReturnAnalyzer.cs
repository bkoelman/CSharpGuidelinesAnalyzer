using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EvaluateQueryBeforeReturnAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Evaluate LINQ query before returning it";
    private const string OperationMessageFormat = "{0} '{1}' returns the result of a call to '{2}', which uses deferred execution";
    private const string QueryMessageFormat = "{0} '{1}' returns the result of a query, which uses deferred execution";
    private const string QueryableMessageFormat = "{0} '{1}' returns an IQueryable, which uses deferred execution";
    private const string Description = "Evaluate the result of a LINQ expression before returning it.";
    private const string QueryOperationName = "<*>Query";
    private const string QueryableOperationName = "<*>Queryable";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1250";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    private static readonly DiagnosticDescriptor OperationRule = new(DiagnosticId, Title, OperationMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor QueryRule = new(DiagnosticId, Title, QueryMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning,
        true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor QueryableRule = new(DiagnosticId, Title, QueryableMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<string> LinqOperatorsDeferred = ImmutableArray.Create("Aggregate", "All", "Any", "Cast", "Concat", "Contains",
        "DefaultIfEmpty", "Except", "GroupBy", "GroupJoin", "Intersect", "Join", "OfType", "OrderBy", "OrderByDescending", "Range", "Repeat", "Reverse",
        "Select", "SelectMany", "SequenceEqual", "Skip", "SkipWhile", "Take", "TakeWhile", "ThenBy", "ThenByDescending", "Union", "Where", "Zip");

    private static readonly ImmutableArray<string> LinqOperatorsImmediate = ImmutableArray.Create("Average", "Count", "Distinct", "ElementAt",
        "ElementAtOrDefault", "Empty", "First", "FirstOrDefault", "Last", "LastOrDefault", "LongCount", "Max", "Min", "Single", "SingleOrDefault", "Sum",
        "ToArray", "ToImmutableArray", "ToDictionary", "ToList", "ToLookup");

    private static readonly ImmutableArray<string> LinqOperatorsTransparent = ImmutableArray.Create("AsEnumerable", "AsQueryable");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(OperationRule, QueryRule, QueryableRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        var sequenceTypeInfo = new SequenceTypeInfo(startContext.Compilation);

        startContext.SafeRegisterOperationBlockAction(context => AnalyzeCodeBlock(context, sequenceTypeInfo));
    }

    private static void AnalyzeCodeBlock(OperationBlockAnalysisContext context, SequenceTypeInfo sequenceTypeInfo)
    {
        if (context.OwningSymbol.DeclaredAccessibility != Accessibility.Public || !IsInMethodThatReturnsEnumerable(context.OwningSymbol, sequenceTypeInfo))
        {
            return;
        }

        var collector = new ReturnStatementCollector(sequenceTypeInfo, context);
        collector.VisitBlocks(context.OperationBlocks);

        AnalyzeReturnStatements(collector.ReturnStatements, context);
    }

    private static bool IsInMethodThatReturnsEnumerable(ISymbol owningSymbol, SequenceTypeInfo sequenceTypeInfo)
    {
        return owningSymbol is IMethodSymbol { ReturnsVoid: false } method && sequenceTypeInfo.IsEnumerable(method.ReturnType);
    }

    private static void AnalyzeReturnStatements(IList<IReturnOperation> returnStatements, OperationBlockAnalysisContext context)
    {
        if (returnStatements.Any())
        {
            foreach (IReturnOperation returnStatement in returnStatements)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var analyzer = new ReturnValueAnalyzer(context);
                analyzer.Analyze(returnStatement);
            }
        }
    }

    private static void ReportDiagnosticAt(IReturnOperation returnStatement, string operationName, OperationBlockAnalysisContext context)
    {
        Location? location = returnStatement.TryGetLocationForKeyword();

        if (location != null)
        {
            ISymbol containingMember = context.OwningSymbol.GetContainingMember();
            string memberName = containingMember.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            (DiagnosticDescriptor rule, object[] messageArguments) = GetArgumentsForReport(operationName, containingMember, memberName);

            var diagnostic = Diagnostic.Create(rule, location, messageArguments);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static (DiagnosticDescriptor rule, object[] messageArguments) GetArgumentsForReport(string operationName, ISymbol containingMember,
        string memberName)
    {
        switch (operationName)
        {
            case QueryOperationName:
            {
                return (QueryRule, [
                    containingMember.GetKind(),
                    memberName
                ]);
            }
            case QueryableOperationName:
            {
                return (QueryableRule, [
                    containingMember.GetKind(),
                    memberName
                ]);
            }
            default:
            {
                return (OperationRule, [
                    containingMember.GetKind(),
                    memberName,
                    operationName
                ]);
            }
        }
    }

    /// <summary>
    /// Scans for return statements, skipping over anonymous methods and local functions, whose compile-time type allows for deferred execution.
    /// </summary>
    private sealed class ReturnStatementCollector : ExplicitOperationWalker
    {
        private readonly SequenceTypeInfo sequenceTypeInfo;

        private readonly OperationBlockAnalysisContext context;

        private int scopeDepth;

        public IList<IReturnOperation> ReturnStatements { get; } = [];

        public ReturnStatementCollector(SequenceTypeInfo sequenceTypeInfo, OperationBlockAnalysisContext context)
        {
            ArgumentNullException.ThrowIfNull(sequenceTypeInfo);

            this.sequenceTypeInfo = sequenceTypeInfo;
            this.context = context;
        }

        public void VisitBlocks(ImmutableArray<IOperation> blocks)
        {
            foreach (IOperation block in blocks)
            {
                Visit(block);
            }
        }

        public override void VisitLocalFunction(ILocalFunctionOperation operation)
        {
            scopeDepth++;
            base.VisitLocalFunction(operation);
            scopeDepth--;
        }

        public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
        {
            scopeDepth++;
            base.VisitAnonymousFunction(operation);
            scopeDepth--;
        }

        public override void VisitReturn(IReturnOperation operation)
        {
            if (scopeDepth == 0 && operation.ReturnedValue != null && !ReturnsConstant(operation.ReturnedValue) &&
                MethodSignatureTypeIsEnumerable(operation.ReturnedValue))
            {
                ITypeSymbol? returnValueType = operation.ReturnedValue.SkipTypeConversions().Type;

                if (returnValueType != null)
                {
                    if (sequenceTypeInfo.IsQueryable(returnValueType))
                    {
                        ReportDiagnosticAt(operation, QueryableOperationName, context);
                    }
                    else if (sequenceTypeInfo.IsNonQueryableSequenceType(returnValueType))
                    {
                        ReturnStatements.Add(operation);
                    }
                    // ReSharper disable once RedundantIfElseBlock
                    else
                    {
                        // No action required.
                    }
                }
            }

            base.VisitReturn(operation);
        }

        private static bool ReturnsConstant(IOperation returnValue)
        {
            return returnValue.ConstantValue.HasValue;
        }

        private bool MethodSignatureTypeIsEnumerable(IOperation returnValue)
        {
            return returnValue.Type != null && sequenceTypeInfo.IsEnumerable(returnValue.Type);
        }
    }

    /// <summary>
    /// Analyzes the filtered set of return values in a method.
    /// </summary>
    private sealed class ReturnValueAnalyzer(OperationBlockAnalysisContext context)
    {
        private readonly OperationBlockAnalysisContext context = context;

        private readonly IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache = new Dictionary<ILocalSymbol, EvaluationResult>();

        public void Analyze(IReturnOperation returnStatement)
        {
            if (returnStatement.ReturnedValue != null)
            {
                EvaluationResult result = AnalyzeExpression(returnStatement.ReturnedValue);

                if (result.IsConclusive && result.IsDeferred)
                {
                    ReportDiagnosticAt(returnStatement, result.DeferredOperationName, context);
                }
            }
        }

        private EvaluationResult AnalyzeExpression(IOperation expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            context.CancellationToken.ThrowIfCancellationRequested();

            var walker = new ExpressionWalker(this);
            walker.Visit(expression);

            return walker.Result;
        }

        /// <summary>
        /// Runs flow analysis on the return value expression of a return statement.
        /// </summary>
        private sealed class ExpressionWalker : AbstractEvaluatingOperationWalker
        {
            private readonly ReturnValueAnalyzer owner;

            public ExpressionWalker(ReturnValueAnalyzer owner)
            {
                ArgumentNullException.ThrowIfNull(owner);
                this.owner = owner;
            }

            public override void VisitConversion(IConversionOperation operation)
            {
                Visit(operation.Operand);
            }

            public override void VisitInvocation(IInvocationOperation operation)
            {
                base.VisitInvocation(operation);

                if (operation.Instance == null)
                {
                    if (IsExecutionDeferred(operation) || IsExecutionImmediate(operation) || IsExecutionTransparent(operation))
                    {
                        return;
                    }
                }

                Result.SetUnknown();
            }

            private bool IsExecutionDeferred(IInvocationOperation operation)
            {
                if (LinqOperatorsDeferred.Contains(operation.TargetMethod.Name))
                {
                    if (operation.TargetMethod.NullableContainingType is not { SpecialType: SpecialType.System_String })
                    {
                        Result.SetDeferred(operation.TargetMethod.Name);
                        return true;
                    }
                }

                return false;
            }

            private bool IsExecutionImmediate(IInvocationOperation operation)
            {
                if (LinqOperatorsImmediate.Contains(operation.TargetMethod.Name))
                {
                    Result.SetImmediate();
                    return true;
                }

                return false;
            }

            private bool IsExecutionTransparent(IInvocationOperation operation)
            {
                return LinqOperatorsTransparent.Contains(operation.TargetMethod.Name);
            }

            public override void VisitLocalReference(ILocalReferenceOperation operation)
            {
                if (IsInvokingDelegateVariable(operation))
                {
                    return;
                }

                Location location = operation.Syntax.GetLocation();

                var assignmentWalker = new VariableAssignmentWalker(operation.Local, location, owner);
                assignmentWalker.VisitBlockBody();

                Result.CopyFrom(assignmentWalker.Result);
            }

            private static bool IsInvokingDelegateVariable(ILocalReferenceOperation operation)
            {
                return operation.Parent is IInvocationOperation;
            }

            public override void VisitConditional(IConditionalOperation operation)
            {
                EvaluationResult trueResult = owner.AnalyzeExpression(operation.WhenTrue);

                if (operation.WhenFalse == null || trueResult.IsDeferred)
                {
                    Result.CopyFrom(trueResult);
                }
                else
                {
                    EvaluationResult falseResult = owner.AnalyzeExpression(operation.WhenFalse);

                    EvaluationResult unified = EvaluationResult.Unify(trueResult, falseResult);
                    Result.CopyFrom(unified);
                }
            }

            public override void VisitCoalesce(ICoalesceOperation operation)
            {
                EvaluationResult valueResult = owner.AnalyzeExpression(operation.Value);

                if (valueResult.IsDeferred)
                {
                    Result.CopyFrom(valueResult);
                }
                else
                {
                    EvaluationResult alternativeResult = owner.AnalyzeExpression(operation.WhenNull);

                    EvaluationResult unified = EvaluationResult.Unify(valueResult, alternativeResult);
                    Result.CopyFrom(unified);
                }
            }

            public override void VisitTranslatedQuery(ITranslatedQueryOperation operation)
            {
                Result.CopyFrom(EvaluationResult.Query);
            }

            public override void VisitObjectCreation(IObjectCreationOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitArrayCreation(IArrayCreationOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitArrayElementReference(IArrayElementReferenceOperation operation)
            {
                Result.SetUnknown();
            }

            public override void VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation)
            {
                Result.SetUnknown();
            }

            public override void VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitDefaultValue(IDefaultValueOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitDynamicInvocation(IDynamicInvocationOperation operation)
            {
                Result.SetUnknown();
            }

            public override void VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation)
            {
                Result.SetUnknown();
            }

            public override void VisitNameOf(INameOfOperation operation)
            {
                Result.SetUnknown();
            }

            public override void VisitLiteral(ILiteralOperation operation)
            {
                Result.SetImmediate();
            }

            public override void VisitThrow(IThrowOperation operation)
            {
                Result.SetImmediate();
            }
        }

        /// <summary>
        /// Evaluates all assignments to a specific variable in a code block, storing its intermediate states.
        /// </summary>
        private sealed class VariableAssignmentWalker : AbstractEvaluatingOperationWalker
        {
            private readonly ILocalSymbol currentLocal;

            private readonly Location maxLocation;

            private readonly ReturnValueAnalyzer owner;

            public VariableAssignmentWalker(ILocalSymbol local, Location maxLocation, ReturnValueAnalyzer owner)
            {
                ArgumentNullException.ThrowIfNull(local);
                ArgumentNullException.ThrowIfNull(maxLocation);
                ArgumentNullException.ThrowIfNull(owner);

                currentLocal = local;
                this.maxLocation = maxLocation;
                this.owner = owner;
            }

            public void VisitBlockBody()
            {
                if (owner.variableEvaluationCache.TryGetValue(currentLocal, out EvaluationResult? resultFromCache))
                {
                    Result.CopyFrom(resultFromCache);
                }
                else
                {
                    foreach (IOperation operation in owner.context.OperationBlocks)
                    {
                        Visit(operation);
                    }
                }
            }

            public override void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
            {
                base.VisitVariableDeclarator(operation);

                if (currentLocal.IsEqualTo(operation.Symbol) && EndsBeforeMaxLocation(operation))
                {
                    IVariableInitializerOperation? initializer = operation.GetVariableInitializer();

                    if (initializer != null)
                    {
                        AnalyzeAssignmentValue(initializer.Value);
                    }
                }
            }

            public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
            {
                base.VisitSimpleAssignment(operation);

                if (operation.Target is ILocalReferenceOperation targetLocal && currentLocal.IsEqualTo(targetLocal.Local) && EndsBeforeMaxLocation(operation))
                {
                    AnalyzeAssignmentValue(operation.Value);
                }
            }

            public override void VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation)
            {
                base.VisitDeconstructionAssignment(operation);

                if (operation.Target is ITupleOperation tupleOperation && EndsBeforeMaxLocation(operation))
                {
                    foreach (IOperation element in tupleOperation.Elements)
                    {
                        if (element is ILocalReferenceOperation targetLocal && currentLocal.IsEqualTo(targetLocal.Local))
                        {
                            UpdateResult(EvaluationResult.Unknown);
                        }
                    }
                }
            }

            private bool EndsBeforeMaxLocation(IOperation operation)
            {
                return operation.Syntax.GetLocation().SourceSpan.End < maxLocation.SourceSpan.Start;
            }

            private void AnalyzeAssignmentValue(IOperation assignedValue)
            {
                ArgumentNullException.ThrowIfNull(assignedValue);

                EvaluationResult result = owner.AnalyzeExpression(assignedValue);
                UpdateResult(result);
            }

            private void UpdateResult(EvaluationResult result)
            {
                if (result.IsConclusive)
                {
                    Result.CopyFrom(result);

                    owner.variableEvaluationCache[currentLocal] = Result;
                }
            }
        }
    }

    private abstract class AbstractEvaluatingOperationWalker : OperationWalker
    {
        public EvaluationResult Result { get; } = new();
    }

    private sealed class EvaluationResult
    {
        public static readonly EvaluationResult Query = new(EvaluationState.Deferred, QueryOperationName);

        public static readonly EvaluationResult Unknown = new(EvaluationState.Unknown, null);

        private EvaluationState evaluationState;

        private string? deferredOperationNameOrNull;

        public string DeferredOperationName
        {
            get
            {
                if (evaluationState != EvaluationState.Deferred)
                {
                    throw new InvalidOperationException("Operation name is not available in non-deferred states.");
                }

                return deferredOperationNameOrNull!;
            }
        }

        public bool IsConclusive => evaluationState != EvaluationState.Initial;

        public bool IsDeferred => evaluationState == EvaluationState.Deferred;

        public EvaluationResult()
        {
        }

        private EvaluationResult(EvaluationState state, string? deferredOperationNameOrNull)
        {
            evaluationState = state;
            this.deferredOperationNameOrNull = deferredOperationNameOrNull;
        }

        public void SetImmediate()
        {
            evaluationState = EvaluationState.Immediate;
        }

        public void SetUnknown()
        {
            evaluationState = EvaluationState.Unknown;
        }

        public void SetDeferred(string operationName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

            evaluationState = EvaluationState.Deferred;
            deferredOperationNameOrNull = operationName;
        }

        public void CopyFrom(EvaluationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            evaluationState = result.evaluationState;
            deferredOperationNameOrNull = result.deferredOperationNameOrNull;
        }

        public static EvaluationResult Unify(EvaluationResult first, EvaluationResult second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            if (first is { IsConclusive: true, IsDeferred: true })
            {
                return first;
            }

            if (second is { IsConclusive: true, IsDeferred: true })
            {
                return second;
            }

            return first.IsConclusive ? first : second;
        }

        public override string ToString()
        {
            return evaluationState.ToString();
        }

        private enum EvaluationState
        {
            Initial,
            Unknown,
            Immediate,
            Deferred
        }
    }

    private sealed class SequenceTypeInfo
    {
        private readonly ImmutableArray<INamedTypeSymbol> queryableTypes;

        private readonly ImmutableArray<INamedTypeSymbol> otherSequenceTypes;

        public SequenceTypeInfo(Compilation compilation)
        {
            ArgumentNullException.ThrowIfNull(compilation);

            queryableTypes = GetQueryableTypes(compilation);
            otherSequenceTypes = GetOtherSequenceTypes(compilation);
        }

        private ImmutableArray<INamedTypeSymbol> GetQueryableTypes(Compilation compilation)
        {
            INamedTypeSymbol?[] types =
            [
                KnownTypes.SystemLinqIQueryableT(compilation),
                KnownTypes.SystemLinqIOrderedQueryableT(compilation),
                KnownTypes.SystemLinqIQueryable(compilation),
                KnownTypes.SystemLinqIOrderedQueryable(compilation)
            ];

            return types.Where(type => type != null).Cast<INamedTypeSymbol>().ToImmutableArray();
        }

        private ImmutableArray<INamedTypeSymbol> GetOtherSequenceTypes(Compilation compilation)
        {
            INamedTypeSymbol?[] types =
            [
                KnownTypes.SystemLinqIOrderedEnumerableT(compilation),
                KnownTypes.SystemLinqIGroupingTKeyTElement(compilation),
                KnownTypes.SystemLinqILookupTKeyTElement(compilation)
            ];

            return types.Where(type => type != null).Cast<INamedTypeSymbol>().ToImmutableArray();
        }

        public bool IsEnumerable(ITypeSymbol type)
        {
            return type.IsEnumerableInterface();
        }

        public bool IsQueryable(ITypeSymbol type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return queryableTypes.Contains(type.OriginalDefinition);
        }

        public bool IsNonQueryableSequenceType(ITypeSymbol type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return IsEnumerable(type) || otherSequenceTypes.Contains(type.OriginalDefinition);
        }
    }
}
