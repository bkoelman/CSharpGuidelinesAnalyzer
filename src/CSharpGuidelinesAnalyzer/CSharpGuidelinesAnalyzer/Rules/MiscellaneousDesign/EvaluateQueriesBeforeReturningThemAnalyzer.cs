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
    public sealed class EvaluateQueriesBeforeReturningThemAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1250";

        private const string Title = "Evaluate LINQ queries before returning them";

        private const string OperationMessageFormat =
            "{0} '{1}' returns the result of a call to '{2}', which uses deferred execution.";

        private const string QueryMessageFormat = "{0} '{1}' returns the result of a query that uses deferred execution.";
        private const string Description = "Evaluate the result of a LINQ expression before returning it.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor OperationRule = new DiagnosticDescriptor(DiagnosticId, Title,
            OperationMessageFormat, Category.Name, DiagnosticSeverity.Warning, true, Description, Category.HelpLinkUri);

        [NotNull]
        private static readonly DiagnosticDescriptor QueryRule = new DiagnosticDescriptor(DiagnosticId, Title, QueryMessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.HelpLinkUri);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(OperationRule, QueryRule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> LinqOperatorsDeferred = ImmutableArray.Create("Aggregate", "All", "Any",
            "Cast", "Concat", "Contains", "DefaultIfEmpty", "Except", "GroupBy", "GroupJoin", "Intersect", "Join", "OfType",
            "OrderBy", "OrderByDescending", "Range", "Repeat", "Reverse", "Select", "SelectMany", "SequenceEqual", "Skip",
            "SkipWhile", "Take", "TakeWhile", "ThenBy", "ThenByDescending", "Union", "Where", "Zip");

        [ItemNotNull]
        private static readonly ImmutableArray<string> LinqOperatorsImmediate = ImmutableArray.Create("AsEnumerable", "Average",
            "Count", "Distinct", "ElementAt", "ElementAtOrDefault", "Empty", "First", "FirstOrDefault", "Last", "LastOrDefault",
            "LongCount", "Max", "Min", "Single", "SingleOrDefault", "Sum", "ToArray", "ToDictionary", "ToList", "ToLookup");

        [NotNull]
        private const string QueryOperationName = "";

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationBlockAction(c => c.SkipInvalid(AnalyzeCodeBlock));
        }

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context)
        {
            if (!(context.OwningSymbol is IMethodSymbol method) || method.ReturnsVoid || !ReturnsEnumerable(method))
            {
                return;
            }

            var variableEvaluationCache = new Dictionary<ILocalSymbol, EvaluationResult>();

            foreach (IReturnOperation returnStatement in context.OperationBlocks.SelectMany(b =>
                b.DescendantsAndSelf().OfType<IReturnOperation>()))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                AnalyzeReturnStatement(returnStatement, context, variableEvaluationCache);
            }
        }

        private static bool ReturnsEnumerable([NotNull] IMethodSymbol method)
        {
            return method.ReturnType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
                method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerable;
        }

        private void AnalyzeReturnStatement([NotNull] IReturnOperation returnStatement, OperationBlockAnalysisContext context,
            [NotNull] IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache)
        {
            if (!ReturnsConstant(returnStatement) && !IsYieldBreak(returnStatement))
            {
                var analyzer = new ReturnValueAnalyzer(context, variableEvaluationCache);
                analyzer.Analyze(returnStatement);
            }
        }

        private bool IsYieldBreak([NotNull] IReturnOperation returnStatement)
        {
            return returnStatement.ReturnedValue == null;
        }

        private static bool ReturnsConstant([NotNull] IReturnOperation returnStatement)
        {
            return returnStatement.ReturnedValue is ILiteralOperation;
        }

        private sealed class ReturnValueAnalyzer
        {
            private OperationBlockAnalysisContext context;

            [NotNull]
            private readonly IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache;

            public ReturnValueAnalyzer(OperationBlockAnalysisContext context,
                [NotNull] IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache)
            {
                this.context = context;
                this.variableEvaluationCache = variableEvaluationCache;
            }

            public void Analyze([NotNull] IReturnOperation returnStatement)
            {
                EvaluationResult result = AnalyzeExpression(returnStatement.ReturnedValue);

                if (result.IsConclusive && result.IsDeferred)
                {
                    ReportDiagnosticAt(returnStatement, result.DeferredOperationName);
                }
            }

            [NotNull]
            private EvaluationResult AnalyzeExpression([NotNull] IOperation expression)
            {
                Guard.NotNull(expression, nameof(expression));

                context.CancellationToken.ThrowIfCancellationRequested();

                var visitor = new ExpressionVisitor(this);
                visitor.Visit(expression);

                if (visitor.Result != null)
                {
                    return visitor.Result;
                }

                return expression is ITranslatedQueryOperation ? EvaluationResult.Query : AnalyzeMemberInvocation(expression);
            }

            private sealed class ExpressionVisitor : OperationVisitor
            {
                [NotNull]
                private readonly ReturnValueAnalyzer owner;

                [CanBeNull]
                public EvaluationResult Result { get; private set; }

                public ExpressionVisitor([NotNull] ReturnValueAnalyzer owner)
                {
                    Guard.NotNull(owner, nameof(owner));
                    this.owner = owner;
                }

                public override void VisitLocalReference([NotNull] ILocalReferenceOperation operation)
                {
                    Result = owner.AnalyzeLocalReference(operation);
                }

                public override void VisitConditional([NotNull] IConditionalOperation operation)
                {
                    Result = owner.AnalyzeConditional(operation);
                }
            }

            [NotNull]
            private EvaluationResult AnalyzeLocalReference([NotNull] ILocalReferenceOperation local)
            {
                var assignmentWalker = new VariableAssignmentWalker(local.Local, this);
                assignmentWalker.VisitMethod();

                return assignmentWalker.Result;
            }

            [NotNull]
            private EvaluationResult AnalyzeConditional([NotNull] IConditionalOperation conditional)
            {
                EvaluationResult trueResult = AnalyzeExpression(conditional.WhenTrue);
                EvaluationResult falseResult = AnalyzeExpression(conditional.WhenFalse);

                return EvaluationResult.Unify(trueResult, falseResult);
            }

            [NotNull]
            private static EvaluationResult AnalyzeMemberInvocation([NotNull] IOperation expression)
            {
                var invocationWalker = new MemberInvocationWalker();
                invocationWalker.Visit(expression);

                return invocationWalker.Result;
            }

            private void ReportDiagnosticAt([NotNull] IReturnOperation returnStatement, [NotNull] string operationName)
            {
                Location location = returnStatement.GetLocationForKeyword();
                ISymbol containingMember = context.OwningSymbol.GetContainingMember();
                string memberName = containingMember.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                Diagnostic diagnostic = operationName == QueryOperationName
                    ? Diagnostic.Create(QueryRule, location, containingMember.Kind, memberName)
                    : Diagnostic.Create(OperationRule, location, containingMember.Kind, memberName, operationName);

                context.ReportDiagnostic(diagnostic);
            }

            private abstract class LinqOperationWalker : OperationWalker
            {
                [NotNull]
                public EvaluationResult Result { get; } = new EvaluationResult();
            }

            /// <summary>
            /// Analyzes method invocations in an expression, to determine whether the final expression is based on deferred or immediate
            /// execution.
            /// </summary>
            private sealed class MemberInvocationWalker : LinqOperationWalker
            {
                public override void VisitInvocation([NotNull] IInvocationOperation operation)
                {
                    base.VisitInvocation(operation);

                    if (operation.Instance == null)
                    {
                        if (IsExecutionDeferred(operation) || IsExecutionImmediate(operation))
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
                        Result.SetDeferred(operation.TargetMethod.Name);
                        return true;
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
            }

            /// <summary>
            /// Analyzes all assignments to a specific variable, storing the final state.
            /// </summary>
            private sealed class VariableAssignmentWalker : LinqOperationWalker
            {
                [NotNull]
                private readonly ILocalSymbol currentLocal;

                [NotNull]
                private readonly ReturnValueAnalyzer owner;

                public VariableAssignmentWalker([NotNull] ILocalSymbol local, [NotNull] ReturnValueAnalyzer owner)
                {
                    Guard.NotNull(local, nameof(local));
                    Guard.NotNull(owner, nameof(owner));

                    currentLocal = local;
                    this.owner = owner;
                }

                public void VisitMethod()
                {
                    if (owner.variableEvaluationCache.ContainsKey(currentLocal))
                    {
                        Result.CopyFrom(owner.variableEvaluationCache[currentLocal]);
                    }
                    else
                    {
                        foreach (IOperation operation in owner.context.OperationBlocks)
                        {
                            Visit(operation);
                        }

                        owner.variableEvaluationCache[currentLocal] = Result;
                    }
                }

                public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
                {
                    base.VisitVariableDeclarator(operation);

                    if (currentLocal.Equals(operation.Symbol) && operation.Initializer != null)
                    {
                        AnalyzeAssignmentValue(operation.Initializer.Value);
                    }
                }

                public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
                {
                    base.VisitSimpleAssignment(operation);

                    if (operation.Target is ILocalReferenceOperation targetLocal && currentLocal.Equals(targetLocal.Local))
                    {
                        AnalyzeAssignmentValue(operation.Value);
                    }
                }

                private void AnalyzeAssignmentValue([NotNull] IOperation assignedValue)
                {
                    Guard.NotNull(assignedValue, nameof(assignedValue));

                    EvaluationResult result = owner.AnalyzeExpression(assignedValue);
                    Result.CopyIfConclusiveFrom(result);
                }
            }
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

            public void CopyIfConclusiveFrom([NotNull] EvaluationResult result)
            {
                Guard.NotNull(result, nameof(result));

                if (result.IsConclusive)
                {
                    CopyFrom(result);
                }
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
    }
}
