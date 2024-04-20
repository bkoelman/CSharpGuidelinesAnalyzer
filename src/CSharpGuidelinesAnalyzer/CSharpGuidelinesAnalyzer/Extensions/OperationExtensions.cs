using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class OperationExtensions
{
    [CanBeNull]
    private static readonly PropertyInfo OperationSemanticModelProperty = typeof(IOperation).GetRuntimeProperty("SemanticModel");

    [CanBeNull]
    public static IdentifierInfo TryGetIdentifierInfo([CanBeNull] this IOperation identifier)
    {
        var visitor = new IdentifierVisitor();
        return visitor.Visit(identifier, null);
    }

    [CanBeNull]
    public static Location TryGetLocationForKeyword([NotNull] this IOperation operation,
        DoWhileLoopLookupKeywordStrategy doWhileLoopLookupStrategy = DoWhileLoopLookupKeywordStrategy.PreferDoKeyword,
        TryFinallyLookupKeywordStrategy tryFinallyLookupKeywordStrategy = TryFinallyLookupKeywordStrategy.PreferTryKeyword)
    {
        if (operation.IsImplicit)
        {
            return null;
        }

        var visitor = new OperationLocationVisitor(doWhileLoopLookupStrategy, tryFinallyLookupKeywordStrategy);
        return visitor.Visit(operation, null);
    }

    public static bool HasErrors([NotNull] this IOperation operation, [NotNull] Compilation compilation, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(operation, nameof(operation));
        Guard.NotNull(compilation, nameof(compilation));

        if (operation.Syntax == null)
        {
            return true;
        }

        SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);

        return model.GetDiagnostics(operation.Syntax.Span, cancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    public static bool IsStatement([NotNull] this IOperation operation)
    {
        if (operation.Type != null || HasConstantValue(operation))
        {
            return false;
        }

        if (operation is IBlockOperation or ILabeledOperation)
        {
            return false;
        }

        if (operation.Parent is IBlockOperation)
        {
            return true;
        }

        return OperationIsStatementBecauseItExistsInBodyOfParent(operation);
    }

    private static bool OperationIsStatementBecauseItExistsInBodyOfParent([NotNull] IOperation operation)
    {
        return OperationExistsInBodyOfForLoop(operation) || OperationExistsInBodyOfForEachLoop(operation) || OperationExistsInBodyOfWhileLoop(operation) ||
            OperationExistsInBodyOfIfStatement(operation) || OperationExistsInBodyOfTryFinallyStatement(operation) ||
            OperationExistsInBodyOfCatchClause(operation) || OperationExistsInBodyOfCaseClause(operation) || OperationExistsInBodyOfLockStatement(operation) ||
            OperationExistsInBodyOfLabel(operation) || OperationExistsInBodyOfUsingStatement(operation);
    }

    private static bool OperationExistsInBodyOfForLoop([NotNull] IOperation operation)
    {
        return operation.Parent is IForLoopOperation parentForLoop && IsOperationInBodyOfParent(operation, parentForLoop.Body);
    }

    private static bool OperationExistsInBodyOfForEachLoop([NotNull] IOperation operation)
    {
        return operation.Parent is IForEachLoopOperation parentForEachLoop && IsOperationInBodyOfParent(operation, parentForEachLoop.Body);
    }

    private static bool OperationExistsInBodyOfWhileLoop([NotNull] IOperation operation)
    {
        return operation.Parent is IWhileLoopOperation parentWhileLoop && IsOperationInBodyOfParent(operation, parentWhileLoop.Body);
    }

    private static bool OperationExistsInBodyOfIfStatement([NotNull] IOperation operation)
    {
        if (operation.Parent is IConditionalOperation parentConditional && parentConditional.IsStatement())
        {
            if (IsOperationInBodyOfParent(operation, parentConditional.WhenTrue) || IsOperationInBodyOfParent(operation, parentConditional.WhenFalse))
            {
                return true;
            }
        }

        return false;
    }

    private static bool OperationExistsInBodyOfTryFinallyStatement([NotNull] IOperation operation)
    {
        if (operation.Parent is ITryOperation parentTry)
        {
            if (IsOperationInBodyOfParent(operation, parentTry.Body) || IsOperationInBodyOfParent(operation, parentTry.Finally))
            {
                return true;
            }
        }

        return false;
    }

    private static bool OperationExistsInBodyOfCatchClause([NotNull] IOperation operation)
    {
        return operation.Parent is ICatchClauseOperation parentCatchClause && IsOperationInBodyOfParent(operation, parentCatchClause.Handler);
    }

    private static bool OperationExistsInBodyOfCaseClause([NotNull] IOperation operation)
    {
        return operation.Parent is ISwitchCaseOperation parentSwitchCase && parentSwitchCase.Body.Contains(operation);
    }

    private static bool OperationExistsInBodyOfLockStatement([NotNull] IOperation operation)
    {
        return operation.Parent is ILockOperation parentLock && IsOperationInBodyOfParent(operation, parentLock.Body);
    }

    private static bool OperationExistsInBodyOfLabel([NotNull] IOperation operation)
    {
        return operation.Parent is ILabeledOperation parentLabel && IsOperationInBodyOfParent(operation, parentLabel.Operation);
    }

    private static bool OperationExistsInBodyOfUsingStatement([NotNull] IOperation operation)
    {
        return operation.Parent is IUsingOperation parentUsing && IsOperationInBodyOfParent(operation, parentUsing.Body);
    }

    private static bool HasConstantValue([NotNull] IOperation operation)
    {
        return operation.ConstantValue is { HasValue: true, Value: null };
    }

    private static bool IsOperationInBodyOfParent([NotNull] IOperation operation, [NotNull] IOperation parentOperationBody)
    {
        if (parentOperationBody is IBlockOperation bodyBlockOperation)
        {
            return bodyBlockOperation.Operations.Contains(operation);
        }

        return operation.Equals(parentOperationBody);
    }

    [NotNull]
    public static IOperation SkipTypeConversions([NotNull] this IOperation operation)
    {
        IOperation currentOperation = operation;

        while (currentOperation is IConversionOperation conversion)
        {
            currentOperation = conversion.Operand;
        }

        return currentOperation;
    }

    [CanBeNull]
    public static IMethodSymbol TryGetContainingMethod([NotNull] this IOperation operation, [NotNull] Compilation compilation)
    {
        SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
        return model.GetEnclosingSymbol(operation.Syntax.GetLocation().SourceSpan.Start) as IMethodSymbol;
    }

    [NotNull]
    public static SemanticModel GetSemanticModel([NotNull] this IOperation operation, [NotNull] Compilation compilation)
    {
        if (OperationSemanticModelProperty != null)
        {
            return (SemanticModel)OperationSemanticModelProperty.GetMethod.Invoke(operation, []);
        }

        return compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
    }

    private sealed class IdentifierVisitor : OperationVisitor<object, IdentifierInfo>
    {
        [NotNull]
        public override IdentifierInfo VisitLocalReference([NotNull] ILocalReferenceOperation operation, [CanBeNull] object argument)
        {
            string longName = operation.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.Local.Name, longName);

            return new IdentifierInfo(identifierName, operation.Local.Type);
        }

        [NotNull]
        public override IdentifierInfo VisitParameterReference([NotNull] IParameterReferenceOperation operation, [CanBeNull] object argument)
        {
            var name = new IdentifierName(operation.Parameter.Name,
                /* CSharpShortErrorMessageFormat returns 'int', ie. without parameter name */
                operation.Parameter.Name);

            return new IdentifierInfo(name, operation.Parameter.Type);
        }

        [NotNull]
        public override IdentifierInfo VisitFieldReference([NotNull] IFieldReferenceOperation operation, [CanBeNull] object argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Field.Type);
        }

        [NotNull]
        public override IdentifierInfo VisitEventReference([NotNull] IEventReferenceOperation operation, [CanBeNull] object argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Event.Type);
        }

        [NotNull]
        public override IdentifierInfo VisitPropertyReference([NotNull] IPropertyReferenceOperation operation, [CanBeNull] object argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Property.Type);
        }

        [NotNull]
        private IdentifierInfo CreateForMemberReferenceExpression([NotNull] IMemberReferenceOperation operation, [NotNull] ITypeSymbol memberType)
        {
            string longName = operation.Member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.Member.Name, longName);

            return new IdentifierInfo(identifierName, memberType);
        }

        [NotNull]
        public override IdentifierInfo VisitInvocation([NotNull] IInvocationOperation operation, [CanBeNull] object argument)
        {
            string longName = operation.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.TargetMethod.Name, longName);

            return new IdentifierInfo(identifierName, operation.TargetMethod.ReturnType);
        }
    }

    private sealed class OperationLocationVisitor(DoWhileLoopLookupKeywordStrategy doWhileStrategy, TryFinallyLookupKeywordStrategy tryFinallyStrategy)
        : ExplicitOperationVisitor<object, Location>
    {
        private readonly DoWhileLoopLookupKeywordStrategy doWhileStrategy = doWhileStrategy;
        private readonly TryFinallyLookupKeywordStrategy tryFinallyStrategy = tryFinallyStrategy;

        [NotNull]
        public override Location VisitEmpty([NotNull] IEmptyOperation operation, [CanBeNull] object argument)
        {
            var syntax = (EmptyStatementSyntax)operation.Syntax;
            return syntax.SemicolonToken.GetLocation();
        }

        [NotNull]
        public override Location VisitWhileLoop([NotNull] IWhileLoopOperation operation, [CanBeNull] object argument)
        {
            if (operation.Syntax is DoStatementSyntax doSyntax)
            {
                return doWhileStrategy == DoWhileLoopLookupKeywordStrategy.PreferDoKeyword
                    ? doSyntax.DoKeyword.GetLocation()
                    : doSyntax.WhileKeyword.GetLocation();
            }

            if (operation.Syntax is WhileStatementSyntax whileSyntax)
            {
                return whileSyntax.WhileKeyword.GetLocation();
            }

            return base.VisitWhileLoop(operation, argument);
        }

        [NotNull]
        public override Location VisitForLoop([NotNull] IForLoopOperation operation, [CanBeNull] object argument)
        {
            var syntax = (ForStatementSyntax)operation.Syntax;
            return syntax.ForKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitForEachLoop([NotNull] IForEachLoopOperation operation, [CanBeNull] object argument)
        {
            var syntax = (CommonForEachStatementSyntax)operation.Syntax;
            return syntax.ForEachKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitReturn([NotNull] IReturnOperation operation, [CanBeNull] object argument)
        {
            if (operation.Syntax is ReturnStatementSyntax returnSyntax)
            {
                return returnSyntax.ReturnKeyword.GetLocation();
            }

            if (operation.Syntax is YieldStatementSyntax yieldSyntax)
            {
                return GetLocationForYieldStatement(yieldSyntax);
            }

            return base.VisitReturn(operation, argument);
        }

        [NotNull]
        private static Location GetLocationForYieldStatement([NotNull] YieldStatementSyntax yieldSyntax)
        {
            int start = yieldSyntax.YieldKeyword.GetLocation().SourceSpan.Start;
            int end = yieldSyntax.ReturnOrBreakKeyword.GetLocation().SourceSpan.End;
            TextSpan sourceSpan = TextSpan.FromBounds(start, end);

            return Location.Create(yieldSyntax.SyntaxTree, sourceSpan);
        }

        [NotNull]
        public override Location VisitBranch([NotNull] IBranchOperation operation, [CanBeNull] object argument)
        {
            switch (operation.BranchKind)
            {
                case BranchKind.Continue:
                {
                    return VisitContinueStatement(operation);
                }
                case BranchKind.Break:
                {
                    return VisitBreakStatement(operation);
                }
                case BranchKind.GoTo:
                {
                    return VisitGoToStatement(operation);
                }
                default:
                {
                    return base.VisitBranch(operation, argument);
                }
            }
        }

        [NotNull]
        private static Location VisitContinueStatement([NotNull] IBranchOperation operation)
        {
            var syntax = (ContinueStatementSyntax)operation.Syntax;
            return syntax.ContinueKeyword.GetLocation();
        }

        [NotNull]
        private static Location VisitBreakStatement([NotNull] IBranchOperation operation)
        {
            var syntax = (BreakStatementSyntax)operation.Syntax;
            return syntax.BreakKeyword.GetLocation();
        }

        [NotNull]
        private static Location VisitGoToStatement([NotNull] IBranchOperation operation)
        {
            var syntax = (GotoStatementSyntax)operation.Syntax;
            return syntax.GotoKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitConditional([NotNull] IConditionalOperation operation, [CanBeNull] object argument)
        {
            if (operation.IsStatement())
            {
                var syntax = (IfStatementSyntax)operation.Syntax;
                return syntax.IfKeyword.GetLocation();
            }

            return base.VisitConditional(operation, argument);
        }

        [NotNull]
        public override Location VisitUsing([NotNull] IUsingOperation operation, [CanBeNull] object argument)
        {
            var syntax = (UsingStatementSyntax)operation.Syntax;
            return syntax.UsingKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitLock([NotNull] ILockOperation operation, [CanBeNull] object argument)
        {
            var syntax = (LockStatementSyntax)operation.Syntax;
            return syntax.LockKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitSwitch([NotNull] ISwitchOperation operation, [CanBeNull] object argument)
        {
            var syntax = (SwitchStatementSyntax)operation.Syntax;
            return syntax.SwitchKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitTry([NotNull] ITryOperation operation, [CanBeNull] object argument)
        {
            var trySyntax = (TryStatementSyntax)operation.Syntax;

            if (tryFinallyStrategy == TryFinallyLookupKeywordStrategy.PreferTryKeyword)
            {
                return trySyntax.TryKeyword.GetLocation();
            }

            FinallyClauseSyntax finallySyntax = TryGetFinallySyntax(operation);

            if (finallySyntax != null)
            {
                return finallySyntax.FinallyKeyword.GetLocation();
            }

            return base.VisitTry(operation, argument);
        }

        [CanBeNull]
        private static FinallyClauseSyntax TryGetFinallySyntax([NotNull] ITryOperation operation)
        {
            var finallySyntax = operation.Finally.Syntax as FinallyClauseSyntax;

            if (finallySyntax == null)
            {
                // Bug workaround for https://github.com/dotnet/roslyn/issues/27208
                if (operation.Finally.Syntax is BlockSyntax finallyBlockSyntax)
                {
                    finallySyntax = finallyBlockSyntax.Parent as FinallyClauseSyntax;
                }
            }

            return finallySyntax;
        }

        [NotNull]
        public override Location VisitCatchClause([NotNull] ICatchClauseOperation operation, [CanBeNull] object argument)
        {
            var syntax = (CatchClauseSyntax)operation.Syntax;
            return syntax.CatchKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitThrow([NotNull] IThrowOperation operation, [CanBeNull] object argument)
        {
            if (operation.IsStatement())
            {
                var syntax = (ThrowStatementSyntax)operation.Syntax;
                return syntax.ThrowKeyword.GetLocation();
            }

            return base.VisitThrow(operation, argument);
        }

        [NotNull]
        public override Location VisitSingleValueCaseClause([NotNull] ISingleValueCaseClauseOperation operation, [CanBeNull] object argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        [NotNull]
        public override Location VisitDefaultCaseClause([NotNull] IDefaultCaseClauseOperation operation, [CanBeNull] object argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        [NotNull]
        public override Location VisitPatternCaseClause([NotNull] IPatternCaseClauseOperation operation, [CanBeNull] object argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        [NotNull]
        public override Location VisitAwait([NotNull] IAwaitOperation operation, [CanBeNull] object argument)
        {
            var syntax = (AwaitExpressionSyntax)operation.Syntax;
            return syntax.AwaitKeyword.GetLocation();
        }

        [NotNull]
        public override Location VisitSizeOf([NotNull] ISizeOfOperation operation, [CanBeNull] object argument)
        {
            var syntax = (SizeOfExpressionSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        [NotNull]
        public override Location VisitTypeOf([NotNull] ITypeOfOperation operation, [CanBeNull] object argument)
        {
            var syntax = (TypeOfExpressionSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        [NotNull]
        public override Location VisitNameOf([NotNull] INameOfOperation operation, [CanBeNull] object argument)
        {
            if (operation.Syntax is InvocationExpressionSyntax { Expression: IdentifierNameSyntax expressionSyntax })
            {
                return expressionSyntax.GetLocation();
            }

            return base.VisitNameOf(operation, argument);
        }

        [NotNull]
        public override Location VisitLocalFunction([NotNull] ILocalFunctionOperation operation, [CanBeNull] object argument)
        {
            return operation.Symbol.Locations.FirstOrDefault();
        }
    }
}
