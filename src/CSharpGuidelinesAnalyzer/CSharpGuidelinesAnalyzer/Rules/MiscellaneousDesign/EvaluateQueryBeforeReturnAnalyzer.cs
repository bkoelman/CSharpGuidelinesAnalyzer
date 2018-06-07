using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EvaluateQueryBeforeReturnAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1250";

        private const string Title = "Evaluate LINQ query before returning it";

        private const string OperationMessageFormat =
            "{0} '{1}' returns the result of a call to '{2}', which uses deferred execution.";

        private const string QueryMessageFormat = "{0} '{1}' returns the result of a query, which uses deferred execution.";
        private const string QueryableMessageFormat = "{0} '{1}' returns an IQueryable, which uses deferred execution.";
        private const string Description = "Evaluate the result of a LINQ expression before returning it.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor OperationRule = new DiagnosticDescriptor(DiagnosticId, Title,
            OperationMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor QueryRule = new DiagnosticDescriptor(DiagnosticId, Title, QueryMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor QueryableRule = new DiagnosticDescriptor(DiagnosticId, Title,
            QueryableMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(OperationRule, QueryRule, QueryableRule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> LinqOperatorsDeferred = ImmutableArray.Create("Aggregate", "All", "Any",
            "Cast", "Concat", "Contains", "DefaultIfEmpty", "Except", "GroupBy", "GroupJoin", "Intersect", "Join", "OfType",
            "OrderBy", "OrderByDescending", "Range", "Repeat", "Reverse", "Select", "SelectMany", "SequenceEqual", "Skip",
            "SkipWhile", "Take", "TakeWhile", "ThenBy", "ThenByDescending", "Union", "Where", "Zip");

        [ItemNotNull]
        private static readonly ImmutableArray<string> LinqOperatorsImmediate = ImmutableArray.Create("Average", "Count",
            "Distinct", "ElementAt", "ElementAtOrDefault", "Empty", "First", "FirstOrDefault", "Last", "LastOrDefault",
            "LongCount", "Max", "Min", "Single", "SingleOrDefault", "Sum", "ToArray", "ToImmutableArray", "ToDictionary",
            "ToList", "ToLookup");

        [ItemNotNull]
        private static readonly ImmutableArray<string> LinqOperatorsTransparent =
            ImmutableArray.Create("AsEnumerable", "AsQueryable");

        private const string QueryOperationName = "<*>Query";
        private const string QueryableOperationName = "<*>Queryable";

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var sequenceTypeInfo = new SequenceTypeInfo(startContext.Compilation);

                startContext.RegisterOperationBlockAction(c => c.SkipInvalid(_ => AnalyzeCodeBlock(c, sequenceTypeInfo)));
            });
        }

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context, [NotNull] SequenceTypeInfo sequenceTypeInfo)
        {
            if (!IsInMethodThatReturnsEnumerable(context.OwningSymbol, sequenceTypeInfo))
            {
                return;
            }

            var collector = new ReturnStatementCollector(sequenceTypeInfo, context);
            collector.VisitBlocks(context.OperationBlocks);

            AnalyzeReturnStatements(collector.ReturnStatements, context);
        }

        private static bool IsInMethodThatReturnsEnumerable([NotNull] ISymbol owningSymbol,
            [NotNull] SequenceTypeInfo sequenceTypeInfo)
        {
            return owningSymbol is IMethodSymbol method && !method.ReturnsVoid &&
                sequenceTypeInfo.IsEnumerable(method.ReturnType);
        }

        private void AnalyzeReturnStatements([NotNull] [ItemNotNull] IList<IReturnOperation> returnStatements,
            OperationBlockAnalysisContext context)
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

        private static void ReportDiagnosticAt([NotNull] IReturnOperation returnStatement, [NotNull] string operationName,
            OperationBlockAnalysisContext context)
        {
            Location location = returnStatement.GetLocationForKeyword();
            ISymbol containingMember = context.OwningSymbol.GetContainingMember();
            string memberName = containingMember.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            Diagnostic diagnostic = CreateDiagnosticFor(operationName, location, containingMember, memberName);
            context.ReportDiagnostic(diagnostic);
        }

        [NotNull]
        private static Diagnostic CreateDiagnosticFor([NotNull] string operationName, [CanBeNull] Location location,
            [NotNull] ISymbol containingMember, [NotNull] string memberName)
        {
            switch (operationName)
            {
                case QueryOperationName:
                {
                    return Diagnostic.Create(QueryRule, location, containingMember.GetKind(), memberName);
                }
                case QueryableOperationName:
                {
                    return Diagnostic.Create(QueryableRule, location, containingMember.GetKind(), memberName);
                }
                default:
                {
                    return Diagnostic.Create(OperationRule, location, containingMember.GetKind(), memberName, operationName);
                }
            }
        }

        /// <summary>
        /// Scans for return statements, skipping over anonymous methods and local functions, whose compile-time type allows for deferred
        /// execution.
        /// </summary>
        private sealed class ReturnStatementCollector : OperationWalker
        {
            [NotNull]
            private readonly SequenceTypeInfo sequenceTypeInfo;

            private readonly OperationBlockAnalysisContext context;

            private int scopeDepth;

            [NotNull]
            [ItemNotNull]
            public IList<IReturnOperation> ReturnStatements { get; } = new List<IReturnOperation>();

            public ReturnStatementCollector([NotNull] SequenceTypeInfo sequenceTypeInfo, OperationBlockAnalysisContext context)
            {
                Guard.NotNull(sequenceTypeInfo, nameof(sequenceTypeInfo));

                this.sequenceTypeInfo = sequenceTypeInfo;
                this.context = context;
            }

            public void VisitBlocks([ItemNotNull] ImmutableArray<IOperation> blocks)
            {
                foreach (IOperation block in blocks)
                {
                    Visit(block);
                }
            }

            public override void VisitLocalFunction([NotNull] ILocalFunctionOperation operation)
            {
                scopeDepth++;
                base.VisitLocalFunction(operation);
                scopeDepth--;
            }

            public override void VisitAnonymousFunction([NotNull] IAnonymousFunctionOperation operation)
            {
                scopeDepth++;
                base.VisitAnonymousFunction(operation);
                scopeDepth--;
            }

            public override void VisitReturn([NotNull] IReturnOperation operation)
            {
                if (scopeDepth == 0 && !operation.IsImplicit && operation.ReturnedValue != null &&
                    !ReturnsConstant(operation.ReturnedValue) && MethodSignatureTypeIsEnumerable(operation.ReturnedValue))
                {
                    ITypeSymbol returnValueType = operation.ReturnedValue.SkipTypeConversions().Type;

                    if (sequenceTypeInfo.IsQueryable(returnValueType))
                    {
                        ReportDiagnosticAt(operation, QueryableOperationName, context);
                    }
                    else if (sequenceTypeInfo.IsNonQueryableSequenceType(returnValueType))
                    {
                        ReturnStatements.Add(operation);
                    }
                }

                base.VisitReturn(operation);
            }

            private static bool ReturnsConstant([NotNull] IOperation returnValue)
            {
                return returnValue.ConstantValue.HasValue;
            }

            private bool MethodSignatureTypeIsEnumerable([NotNull] IOperation returnValue)
            {
                return sequenceTypeInfo.IsEnumerable(returnValue.Type);
            }
        }

        /// <summary>
        /// Analyzes the filtered set of return values in a method.
        /// </summary>
        private sealed class ReturnValueAnalyzer
        {
            private readonly OperationBlockAnalysisContext context;

            [NotNull]
            private readonly IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache =
                new Dictionary<ILocalSymbol, EvaluationResult>();

            public ReturnValueAnalyzer(OperationBlockAnalysisContext context)
            {
                this.context = context;
            }

            public void Analyze([NotNull] IReturnOperation returnStatement)
            {
                EvaluationResult result = AnalyzeExpression(returnStatement.ReturnedValue);

                if (result.IsConclusive && result.IsDeferred)
                {
                    ReportDiagnosticAt(returnStatement, result.DeferredOperationName, context);
                }
            }

            [NotNull]
            private EvaluationResult AnalyzeExpression([NotNull] IOperation expression)
            {
                Guard.NotNull(expression, nameof(expression));

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
                [NotNull]
                private readonly ReturnValueAnalyzer owner;

                public ExpressionWalker([NotNull] ReturnValueAnalyzer owner)
                {
                    Guard.NotNull(owner, nameof(owner));
                    this.owner = owner;
                }

                public override void VisitConversion([NotNull] IConversionOperation operation)
                {
                    Visit(operation.Operand);
                }

                public override void VisitInvocation([NotNull] IInvocationOperation operation)
                {
                    base.VisitInvocation(operation);

                    if (operation.Instance == null)
                    {
                        if (IsExecutionDeferred(operation) || IsExecutionImmediate(operation) ||
                            IsExecutionTransparent(operation))
                        {
                            return;
                        }
                    }

                    Result.SetUnknown();
                }

                private bool IsExecutionDeferred([NotNull] IInvocationOperation operation)
                {
                    if (LinqOperatorsDeferred.Contains(operation.TargetMethod.Name))
                    {
                        if (operation.TargetMethod.ContainingType.SpecialType != SpecialType.System_String)
                        {
                            Result.SetDeferred(operation.TargetMethod.Name);
                            return true;
                        }
                    }

                    return false;
                }

                private bool IsExecutionImmediate([NotNull] IInvocationOperation operation)
                {
                    if (LinqOperatorsImmediate.Contains(operation.TargetMethod.Name))
                    {
                        Result.SetImmediate();
                        return true;
                    }

                    return false;
                }

                private bool IsExecutionTransparent([NotNull] IInvocationOperation operation)
                {
                    return LinqOperatorsTransparent.Contains(operation.TargetMethod.Name);
                }

                public override void VisitLocalReference([NotNull] ILocalReferenceOperation operation)
                {
                    if (IsInvokingDelegateVariable(operation))
                    {
                        return;
                    }

                    var assignmentWalker = new VariableAssignmentWalker(operation.Local, operation.Syntax.GetLocation(), owner);
                    assignmentWalker.VisitBlockBody();

                    Result.CopyFrom(assignmentWalker.Result);
                }

                private static bool IsInvokingDelegateVariable([NotNull] ILocalReferenceOperation operation)
                {
                    return operation.Parent is IInvocationOperation;
                }

                public override void VisitConditional([NotNull] IConditionalOperation operation)
                {
                    EvaluationResult trueResult = owner.AnalyzeExpression(operation.WhenTrue);

                    if (operation.WhenFalse == null)
                    {
                        Result.CopyFrom(trueResult);
                    }
                    else
                    {
                        EvaluationResult falseResult = owner.AnalyzeExpression(operation.WhenFalse);

                        Result.CopyFrom(EvaluationResult.Unify(trueResult, falseResult));
                    }
                }

                public override void VisitCoalesce([NotNull] ICoalesceOperation operation)
                {
                    EvaluationResult valueResult = owner.AnalyzeExpression(operation.Value);
                    EvaluationResult alternativeResult = owner.AnalyzeExpression(operation.WhenNull);

                    Result.CopyFrom(EvaluationResult.Unify(valueResult, alternativeResult));
                }

                public override void VisitTranslatedQuery([NotNull] ITranslatedQueryOperation operation)
                {
                    Result.CopyFrom(EvaluationResult.Query);
                }

                public override void VisitObjectCreation([NotNull] IObjectCreationOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitDynamicObjectCreation([NotNull] IDynamicObjectCreationOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitArrayCreation([NotNull] IArrayCreationOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitArrayElementReference([NotNull] IArrayElementReferenceOperation operation)
                {
                    Result.SetUnknown();
                }

                public override void VisitAnonymousObjectCreation([NotNull] IAnonymousObjectCreationOperation operation)
                {
                    Result.SetUnknown();
                }

                public override void VisitObjectOrCollectionInitializer(
                    [NotNull] IObjectOrCollectionInitializerOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitCollectionElementInitializer([NotNull] ICollectionElementInitializerOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitDefaultValue([NotNull] IDefaultValueOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitDynamicInvocation([NotNull] IDynamicInvocationOperation operation)
                {
                    Result.SetUnknown();
                }

                public override void VisitDynamicMemberReference([NotNull] IDynamicMemberReferenceOperation operation)
                {
                    Result.SetUnknown();
                }

                public override void VisitNameOf([NotNull] INameOfOperation operation)
                {
                    Result.SetUnknown();
                }

                public override void VisitLiteral([NotNull] ILiteralOperation operation)
                {
                    Result.SetImmediate();
                }

                public override void VisitThrow([NotNull] IThrowOperation operation)
                {
                    Result.SetImmediate();
                }
            }

            /// <summary>
            /// Evaluates all assignments to a specific variable in a code block, storing its intermediate states.
            /// </summary>
            private sealed class VariableAssignmentWalker : AbstractEvaluatingOperationWalker
            {
                [NotNull]
                private readonly ILocalSymbol currentLocal;

                [NotNull]
                private readonly Location maxLocation;

                [NotNull]
                private readonly ReturnValueAnalyzer owner;

                public VariableAssignmentWalker([NotNull] ILocalSymbol local, [NotNull] Location maxLocation,
                    [NotNull] ReturnValueAnalyzer owner)
                {
                    Guard.NotNull(local, nameof(local));
                    Guard.NotNull(maxLocation, nameof(maxLocation));
                    Guard.NotNull(owner, nameof(owner));

                    currentLocal = local;
                    this.maxLocation = maxLocation;
                    this.owner = owner;
                }

                public void VisitBlockBody()
                {
                    if (owner.variableEvaluationCache.ContainsKey(currentLocal))
                    {
                        EvaluationResult resultFromCache = owner.variableEvaluationCache[currentLocal];
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

                public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
                {
                    base.VisitVariableDeclarator(operation);

                    if (currentLocal.Equals(operation.Symbol) && EndsBeforeMaxLocation(operation))
                    {
                        IVariableInitializerOperation initializer = operation.GetVariableInitializer();
                        if (initializer != null)
                        {
                            AnalyzeAssignmentValue(initializer.Value);
                        }
                    }
                }

                public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
                {
                    base.VisitSimpleAssignment(operation);

                    if (operation.Target is ILocalReferenceOperation targetLocal && currentLocal.Equals(targetLocal.Local) &&
                        EndsBeforeMaxLocation(operation))
                    {
                        AnalyzeAssignmentValue(operation.Value);
                    }
                }

                public override void VisitDeconstructionAssignment([NotNull] IDeconstructionAssignmentOperation operation)
                {
                    base.VisitDeconstructionAssignment(operation);

                    if (operation.Target is ITupleOperation tupleOperation && EndsBeforeMaxLocation(operation))
                    {
                        foreach (IOperation element in tupleOperation.Elements)
                        {
                            if (element is ILocalReferenceOperation targetLocal && currentLocal.Equals(targetLocal.Local))
                            {
                                UpdateResult(EvaluationResult.Unknown);
                            }
                        }
                    }
                }

                private bool EndsBeforeMaxLocation([NotNull] IOperation operation)
                {
                    return operation.Syntax.GetLocation().SourceSpan.End < maxLocation.SourceSpan.Start;
                }

                private void AnalyzeAssignmentValue([NotNull] IOperation assignedValue)
                {
                    Guard.NotNull(assignedValue, nameof(assignedValue));

                    EvaluationResult result = owner.AnalyzeExpression(assignedValue);
                    UpdateResult(result);
                }

                private void UpdateResult([NotNull] EvaluationResult result)
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
            [NotNull]
            public EvaluationResult Result { get; } = new EvaluationResult();
        }

        private sealed class EvaluationResult
        {
            private EvaluationState evaluationState;

            [CanBeNull]
            private string deferredOperationNameOrNull;

            [NotNull]
            public string DeferredOperationName
            {
                get
                {
                    if (evaluationState != EvaluationState.Deferred)
                    {
                        throw new InvalidOperationException("Operation name is not available in non-deferred states.");
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    return deferredOperationNameOrNull;
                }
            }

            public bool IsConclusive => evaluationState != EvaluationState.Initial;

            public bool IsDeferred => evaluationState == EvaluationState.Deferred;

            [NotNull]
            public static readonly EvaluationResult Query = new EvaluationResult(EvaluationState.Deferred, QueryOperationName);

            [NotNull]
            public static readonly EvaluationResult Unknown = new EvaluationResult(EvaluationState.Unknown, null);

            public EvaluationResult()
            {
            }

            private EvaluationResult(EvaluationState state, [CanBeNull] string deferredOperationNameOrNull)
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

            public void SetDeferred([NotNull] string operationName)
            {
                Guard.NotNullNorWhiteSpace(operationName, nameof(operationName));

                evaluationState = EvaluationState.Deferred;
                deferredOperationNameOrNull = operationName;
            }

            public void CopyFrom([NotNull] EvaluationResult result)
            {
                Guard.NotNull(result, nameof(result));

                evaluationState = result.evaluationState;
                deferredOperationNameOrNull = result.deferredOperationNameOrNull;
            }

            [NotNull]
            public static EvaluationResult Unify([NotNull] EvaluationResult first, [NotNull] EvaluationResult second)
            {
                Guard.NotNull(first, nameof(first));
                Guard.NotNull(second, nameof(second));

                if (first.IsConclusive && first.IsDeferred)
                {
                    return first;
                }

                if (second.IsConclusive && second.IsDeferred)
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
            [ItemNotNull]
            private readonly ImmutableArray<INamedTypeSymbol> queryableTypes;

            [ItemNotNull]
            private readonly ImmutableArray<INamedTypeSymbol> otherSequenceTypes;

            public SequenceTypeInfo([NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                queryableTypes = GetQueryableTypes(compilation);
                otherSequenceTypes = GetOtherSequenceTypes(compilation);
            }

            [ItemNotNull]
            private ImmutableArray<INamedTypeSymbol> GetQueryableTypes([NotNull] Compilation compilation)
            {
                ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>(4);

                AddTypeToBuilder(KnownTypes.SystemLinqIQueryableT(compilation), builder);
                AddTypeToBuilder(KnownTypes.SystemLinqIOrderedQueryableT(compilation), builder);
                AddTypeToBuilder(KnownTypes.SystemLinqIQueryable(compilation), builder);
                AddTypeToBuilder(KnownTypes.SystemLinqIOrderedQueryable(compilation), builder);

                return !builder.Any() ? ImmutableArray<INamedTypeSymbol>.Empty : builder.ToImmutable();
            }

            [ItemNotNull]
            private ImmutableArray<INamedTypeSymbol> GetOtherSequenceTypes([NotNull] Compilation compilation)
            {
                ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>(3);

                AddTypeToBuilder(KnownTypes.SystemLinqIOrderedEnumerableT(compilation), builder);
                AddTypeToBuilder(KnownTypes.SystemLinqIGroupingTKeyTElement(compilation), builder);
                AddTypeToBuilder(KnownTypes.SystemLinqILookupTKeyTElement(compilation), builder);

                return !builder.Any() ? ImmutableArray<INamedTypeSymbol>.Empty : builder.ToImmutable();
            }

            private void AddTypeToBuilder([CanBeNull] INamedTypeSymbol type,
                [NotNull] [ItemNotNull] ImmutableArray<INamedTypeSymbol>.Builder builder)
            {
                if (type != null)
                {
                    builder.Add(type);
                }
            }

            public bool IsEnumerable([NotNull] ITypeSymbol type)
            {
                return type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
                    type.SpecialType == SpecialType.System_Collections_IEnumerable;
            }

            public bool IsQueryable([NotNull] ITypeSymbol type)
            {
                Guard.NotNull(type, nameof(type));

                return queryableTypes.Contains(type.OriginalDefinition);
            }

            public bool IsNonQueryableSequenceType([NotNull] ITypeSymbol type)
            {
                Guard.NotNull(type, nameof(type));

                return IsEnumerable(type) || otherSequenceTypes.Contains(type.OriginalDefinition);
            }
        }
    }
}
