using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class OperationExtensions
{
    public static IdentifierInfo? TryGetIdentifierInfo(this IOperation? identifier)
    {
        var visitor = new IdentifierVisitor();
        return visitor.Visit(identifier, null);
    }

    public static Location? TryGetLocationForKeyword(this IOperation operation,
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

    public static bool HasErrors(this IOperation operation, Compilation compilation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(compilation);

        // TODO: Don't invoke Compilation.GetSemanticModel.
        SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);

        return model.GetDiagnostics(operation.Syntax.Span, cancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    public static bool IsStatement(this IOperation operation)
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

    private static bool OperationIsStatementBecauseItExistsInBodyOfParent(IOperation operation)
    {
        return OperationExistsInBodyOfForLoop(operation) || OperationExistsInBodyOfForEachLoop(operation) || OperationExistsInBodyOfWhileLoop(operation) ||
            OperationExistsInBodyOfIfStatement(operation) || OperationExistsInBodyOfTryFinallyStatement(operation) ||
            OperationExistsInBodyOfCatchClause(operation) || OperationExistsInBodyOfCaseClause(operation) || OperationExistsInBodyOfLockStatement(operation) ||
            OperationExistsInBodyOfLabel(operation) || OperationExistsInBodyOfUsingStatement(operation);
    }

    private static bool OperationExistsInBodyOfForLoop(IOperation operation)
    {
        return operation.Parent is IForLoopOperation parentForLoop && IsOperationInBodyOfParent(operation, parentForLoop.Body);
    }

    private static bool OperationExistsInBodyOfForEachLoop(IOperation operation)
    {
        return operation.Parent is IForEachLoopOperation parentForEachLoop && IsOperationInBodyOfParent(operation, parentForEachLoop.Body);
    }

    private static bool OperationExistsInBodyOfWhileLoop(IOperation operation)
    {
        return operation.Parent is IWhileLoopOperation parentWhileLoop && IsOperationInBodyOfParent(operation, parentWhileLoop.Body);
    }

    private static bool OperationExistsInBodyOfIfStatement(IOperation operation)
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

    private static bool OperationExistsInBodyOfTryFinallyStatement(IOperation operation)
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

    private static bool OperationExistsInBodyOfCatchClause(IOperation operation)
    {
        return operation.Parent is ICatchClauseOperation parentCatchClause && IsOperationInBodyOfParent(operation, parentCatchClause.Handler);
    }

    private static bool OperationExistsInBodyOfCaseClause(IOperation operation)
    {
        return operation.Parent is ISwitchCaseOperation parentSwitchCase && parentSwitchCase.Body.Contains(operation);
    }

    private static bool OperationExistsInBodyOfLockStatement(IOperation operation)
    {
        return operation.Parent is ILockOperation parentLock && IsOperationInBodyOfParent(operation, parentLock.Body);
    }

    private static bool OperationExistsInBodyOfLabel(IOperation operation)
    {
        return operation.Parent is ILabeledOperation parentLabel && IsOperationInBodyOfParent(operation, parentLabel.Operation);
    }

    private static bool OperationExistsInBodyOfUsingStatement(IOperation operation)
    {
        return operation.Parent is IUsingOperation parentUsing && IsOperationInBodyOfParent(operation, parentUsing.Body);
    }

    private static bool HasConstantValue(IOperation operation)
    {
        return operation.ConstantValue is { HasValue: true, Value: null };
    }

    private static bool IsOperationInBodyOfParent(IOperation operation, IOperation? parentOperationBody)
    {
        if (parentOperationBody == null)
        {
            return false;
        }

        if (parentOperationBody is IBlockOperation bodyBlockOperation)
        {
            return bodyBlockOperation.Operations.Contains(operation);
        }

        return operation == parentOperationBody;
    }

    public static IOperation SkipTypeConversions(this IOperation operation)
    {
        IOperation currentOperation = operation;

        while (currentOperation is IConversionOperation conversion)
        {
            currentOperation = conversion.Operand;
        }

        return currentOperation;
    }

    public static IMethodSymbol? TryGetContainingMethod(this IOperation operation, Compilation compilation)
    {
        // TODO: Don't invoke Compilation.GetSemanticModel.
        SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
        return model.GetEnclosingSymbol(operation.Syntax.GetLocation().SourceSpan.Start) as IMethodSymbol;
    }

    private sealed class IdentifierVisitor : OperationVisitor<object?, IdentifierInfo>
    {
        public override IdentifierInfo VisitLocalReference(ILocalReferenceOperation operation, object? argument)
        {
            string longName = operation.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.Local.Name, longName);

            return new IdentifierInfo(identifierName, operation.Local.Type);
        }

        public override IdentifierInfo VisitParameterReference(IParameterReferenceOperation operation, object? argument)
        {
            var name = new IdentifierName(operation.Parameter.Name,
                /* CSharpShortErrorMessageFormat returns 'int', ie. without parameter name */
                operation.Parameter.Name);

            return new IdentifierInfo(name, operation.Parameter.Type);
        }

        public override IdentifierInfo VisitFieldReference(IFieldReferenceOperation operation, object? argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Field.Type);
        }

        public override IdentifierInfo VisitEventReference(IEventReferenceOperation operation, object? argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Event.Type);
        }

        public override IdentifierInfo VisitPropertyReference(IPropertyReferenceOperation operation, object? argument)
        {
            return CreateForMemberReferenceExpression(operation, operation.Property.Type);
        }

        private IdentifierInfo CreateForMemberReferenceExpression(IMemberReferenceOperation operation, ITypeSymbol memberType)
        {
            string longName = operation.Member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.Member.Name, longName);

            return new IdentifierInfo(identifierName, memberType);
        }

        public override IdentifierInfo VisitInvocation(IInvocationOperation operation, object? argument)
        {
            string longName = operation.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var identifierName = new IdentifierName(operation.TargetMethod.Name, longName);

            return new IdentifierInfo(identifierName, operation.TargetMethod.ReturnType);
        }
    }

    private sealed class OperationLocationVisitor(DoWhileLoopLookupKeywordStrategy doWhileStrategy, TryFinallyLookupKeywordStrategy tryFinallyStrategy)
        : ExplicitOperationVisitor<object?, Location>
    {
        private readonly DoWhileLoopLookupKeywordStrategy doWhileStrategy = doWhileStrategy;
        private readonly TryFinallyLookupKeywordStrategy tryFinallyStrategy = tryFinallyStrategy;

        public override Location VisitEmpty(IEmptyOperation operation, object? argument)
        {
            var syntax = (EmptyStatementSyntax)operation.Syntax;
            return syntax.SemicolonToken.GetLocation();
        }

        public override Location? VisitWhileLoop(IWhileLoopOperation operation, object? argument)
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

        public override Location VisitForLoop(IForLoopOperation operation, object? argument)
        {
            var syntax = (ForStatementSyntax)operation.Syntax;
            return syntax.ForKeyword.GetLocation();
        }

        public override Location VisitForEachLoop(IForEachLoopOperation operation, object? argument)
        {
            var syntax = (CommonForEachStatementSyntax)operation.Syntax;
            return syntax.ForEachKeyword.GetLocation();
        }

        public override Location? VisitReturn(IReturnOperation operation, object? argument)
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

        private static Location GetLocationForYieldStatement(YieldStatementSyntax yieldSyntax)
        {
            int start = yieldSyntax.YieldKeyword.GetLocation().SourceSpan.Start;
            int end = yieldSyntax.ReturnOrBreakKeyword.GetLocation().SourceSpan.End;
            TextSpan sourceSpan = TextSpan.FromBounds(start, end);

            return Location.Create(yieldSyntax.SyntaxTree, sourceSpan);
        }

        public override Location? VisitBranch(IBranchOperation operation, object? argument)
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

        private static Location VisitContinueStatement(IBranchOperation operation)
        {
            var syntax = (ContinueStatementSyntax)operation.Syntax;
            return syntax.ContinueKeyword.GetLocation();
        }

        private static Location VisitBreakStatement(IBranchOperation operation)
        {
            var syntax = (BreakStatementSyntax)operation.Syntax;
            return syntax.BreakKeyword.GetLocation();
        }

        private static Location VisitGoToStatement(IBranchOperation operation)
        {
            var syntax = (GotoStatementSyntax)operation.Syntax;
            return syntax.GotoKeyword.GetLocation();
        }

        public override Location? VisitConditional(IConditionalOperation operation, object? argument)
        {
            if (operation.IsStatement())
            {
                var syntax = (IfStatementSyntax)operation.Syntax;
                return syntax.IfKeyword.GetLocation();
            }

            return base.VisitConditional(operation, argument);
        }

        public override Location VisitUsing(IUsingOperation operation, object? argument)
        {
            var syntax = (UsingStatementSyntax)operation.Syntax;
            return syntax.UsingKeyword.GetLocation();
        }

        public override Location VisitLock(ILockOperation operation, object? argument)
        {
            var syntax = (LockStatementSyntax)operation.Syntax;
            return syntax.LockKeyword.GetLocation();
        }

        public override Location VisitSwitch(ISwitchOperation operation, object? argument)
        {
            var syntax = (SwitchStatementSyntax)operation.Syntax;
            return syntax.SwitchKeyword.GetLocation();
        }

        public override Location? VisitTry(ITryOperation operation, object? argument)
        {
            var trySyntax = (TryStatementSyntax)operation.Syntax;

            if (tryFinallyStrategy == TryFinallyLookupKeywordStrategy.PreferTryKeyword)
            {
                return trySyntax.TryKeyword.GetLocation();
            }

            FinallyClauseSyntax? finallySyntax = TryGetFinallySyntax(operation);

            if (finallySyntax != null)
            {
                return finallySyntax.FinallyKeyword.GetLocation();
            }

            return base.VisitTry(operation, argument);
        }

        private static FinallyClauseSyntax? TryGetFinallySyntax(ITryOperation operation)
        {
            var finallySyntax = operation.Finally?.Syntax as FinallyClauseSyntax;

            if (finallySyntax == null)
            {
                // Bug workaround for https://github.com/dotnet/roslyn/issues/27208
                if (operation.Finally?.Syntax is BlockSyntax finallyBlockSyntax)
                {
                    finallySyntax = finallyBlockSyntax.Parent as FinallyClauseSyntax;
                }
            }

            return finallySyntax;
        }

        public override Location VisitCatchClause(ICatchClauseOperation operation, object? argument)
        {
            var syntax = (CatchClauseSyntax)operation.Syntax;
            return syntax.CatchKeyword.GetLocation();
        }

        public override Location? VisitThrow(IThrowOperation operation, object? argument)
        {
            if (operation.IsStatement())
            {
                var syntax = (ThrowStatementSyntax)operation.Syntax;
                return syntax.ThrowKeyword.GetLocation();
            }

            return base.VisitThrow(operation, argument);
        }

        public override Location VisitSingleValueCaseClause(ISingleValueCaseClauseOperation operation, object? argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        public override Location VisitDefaultCaseClause(IDefaultCaseClauseOperation operation, object? argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        public override Location VisitPatternCaseClause(IPatternCaseClauseOperation operation, object? argument)
        {
            var syntax = (SwitchLabelSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        public override Location VisitAwait(IAwaitOperation operation, object? argument)
        {
            var syntax = (AwaitExpressionSyntax)operation.Syntax;
            return syntax.AwaitKeyword.GetLocation();
        }

        public override Location VisitSizeOf(ISizeOfOperation operation, object? argument)
        {
            var syntax = (SizeOfExpressionSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        public override Location VisitTypeOf(ITypeOfOperation operation, object? argument)
        {
            var syntax = (TypeOfExpressionSyntax)operation.Syntax;
            return syntax.Keyword.GetLocation();
        }

        public override Location? VisitNameOf(INameOfOperation operation, object? argument)
        {
            if (operation.Syntax is InvocationExpressionSyntax { Expression: IdentifierNameSyntax expressionSyntax })
            {
                return expressionSyntax.GetLocation();
            }

            return base.VisitNameOf(operation, argument);
        }

        public override Location? VisitLocalFunction(ILocalFunctionOperation operation, object? argument)
        {
            return operation.Symbol.Locations.FirstOrDefault();
        }
    }
}
