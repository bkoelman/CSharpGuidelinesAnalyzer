using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

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
        private const string Category = "Miscellaneous Design";

        [NotNull]
        private static readonly DiagnosticDescriptor OperationRule = new DiagnosticDescriptor(DiagnosticId, Title,
            OperationMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor QueryRule = new DiagnosticDescriptor(DiagnosticId, Title, QueryMessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

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

            context.RegisterConditionalOperationBlockAction(c => c.SkipInvalid(AnalyzeCodeBlock));
        }

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context)
        {
            if (!(context.OwningSymbol is IMethodSymbol method) || method.ReturnsVoid || !ReturnsEnumerable(method))
            {
                return;
            }

            var variableEvaluationCache = new Dictionary<ILocalSymbol, EvaluationResult>();

            foreach (IReturnStatement returnStatement in context.OperationBlocks.SelectMany(b =>
                b.DescendantsAndSelf().OfType<IReturnStatement>()))
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

        private void AnalyzeReturnStatement([NotNull] IReturnStatement returnStatement, OperationBlockAnalysisContext context,
            [NotNull] IDictionary<ILocalSymbol, EvaluationResult> variableEvaluationCache)
        {
            if (!ReturnsConstant(returnStatement) && !IsYieldBreak(returnStatement))
            {
                var analyzer = new ReturnValueAnalyzer(context, variableEvaluationCache);
                analyzer.Analyze(returnStatement);
            }
        }

        private bool IsYieldBreak([NotNull] IReturnStatement returnStatement)
        {
            return returnStatement.ReturnedValue == null;
        }

        private static bool ReturnsConstant([NotNull] IReturnStatement returnStatement)
        {
            return returnStatement.ReturnedValue is ILiteralExpression;
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

            public void Analyze([NotNull] IReturnStatement returnStatement)
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

                return visitor.Result ?? (expression.Syntax is QueryExpressionSyntax
                    ? EvaluationResult.Query
                    : AnalyzeMemberInvocation(expression));
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

                public override void VisitLocalReferenceExpression([NotNull] ILocalReferenceExpression operation)
                {
                    Result = owner.AnalyzeLocalReference(operation);
                }

                public override void VisitConditionalChoiceExpression([NotNull] IConditionalChoiceExpression operation)
                {
                    Result = owner.AnalyzeConditionalChoice(operation);
                }
            }

            [NotNull]
            private EvaluationResult AnalyzeLocalReference([NotNull] ILocalReferenceExpression local)
            {
                var assignmentWalker = new VariableAssignmentWalker(local.Local, this);
                assignmentWalker.VisitMethod();

                return assignmentWalker.Result;
            }

            [NotNull]
            private EvaluationResult AnalyzeConditionalChoice([NotNull] IConditionalChoiceExpression conditional)
            {
                EvaluationResult trueResult = AnalyzeExpression(conditional.IfTrueValue);
                EvaluationResult falseResult = AnalyzeExpression(conditional.IfFalseValue);

                return EvaluationResult.Unify(trueResult, falseResult);
            }

            [NotNull]
            private static EvaluationResult AnalyzeMemberInvocation([NotNull] IOperation expression)
            {
                var invocationWalker = new MemberInvocationWalker();
                invocationWalker.Visit(expression);

                return invocationWalker.Result;
            }

            private void ReportDiagnosticAt([NotNull] IReturnStatement returnStatement, [NotNull] string operationName)
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
                public override void VisitInvocationExpression([NotNull] IInvocationExpression operation)
                {
                    base.VisitInvocationExpression(operation);

                    if (operation.Instance == null)
                    {
                        if (IsExecutionDeferred(operation) || IsExecutionImmediate(operation))
                        {
                            return;
                        }
                    }

                    Result.SetUnknown();
                }

                private bool IsExecutionDeferred([NotNull] IInvocationExpression operation)
                {
                    if (LinqOperatorsDeferred.Contains(operation.TargetMethod.Name))
                    {
                        Result.SetDeferred(operation.TargetMethod.Name);
                        return true;
                    }

                    return false;
                }

                private bool IsExecutionImmediate([NotNull] IInvocationExpression operation)
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

                public override void VisitVariableDeclarationStatement([NotNull] IVariableDeclarationStatement operation)
                {
                    base.VisitVariableDeclarationStatement(operation);

                    foreach (IVariableDeclaration variable in operation.Declarations)
                    {
                        if (currentLocal.Equals(variable.Variables.Single()))
                        {
                            AnalyzeAssignmentValue(variable.Initializer);
                        }
                    }
                }

                public override void VisitAssignmentExpression([NotNull] IAssignmentExpression operation)
                {
                    base.VisitAssignmentExpression(operation);

                    if (operation.Target is ILocalReferenceExpression targetLocal && currentLocal.Equals(targetLocal.Local))
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
