using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed partial class OperationEqualityComparer : IEqualityComparer<IOperation>
    {
        [NotNull]
        private readonly OperationEqualityVisitor visitor = new OperationEqualityVisitor();

        [NotNull]
        public static readonly IEqualityComparer<IOperation> Default = new OperationEqualityComparer();

        private OperationEqualityComparer()
        {
        }

        public bool Equals(IOperation x, IOperation y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            return visitor.Visit(x, y);
        }

        public int GetHashCode(IOperation obj)
        {
            throw new NotSupportedException();
        }

        private sealed class OperationEqualityVisitor : OperationVisitor<IOperation, bool>
        {
            [NotNull]
            private readonly DelegatingEqualityComparer<IOperation> equalityComparerForOperations;

            [NotNull]
            private readonly DelegatingEqualityComparer<ISymbol> equalityComparerForSymbols;

            public OperationEqualityVisitor()
            {
                equalityComparerForOperations = new DelegatingEqualityComparer<IOperation>((x, y) => x.Accept(this, y));
                equalityComparerForSymbols = new DelegatingEqualityComparer<ISymbol>((x, y) => x.Equals(y));
            }

            public override bool VisitAddressOf([NotNull] IAddressOfOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IAddressOfOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitAnonymousFunction([NotNull] IAnonymousFunctionOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IAnonymousFunctionOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Symbol, operation2.Symbol);
            }

            public override bool VisitAnonymousObjectCreation([NotNull] IAnonymousObjectCreationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IAnonymousObjectCreationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitArgument([NotNull] IArgumentOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IArgumentOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.ArgumentKind == operation2.ArgumentKind &&
                    AreSymbolsEqual(operation1.Parameter, operation2.Parameter) &&
                    operation1.InConversion.Equals(operation2.InConversion) &&
                    operation1.OutConversion.Equals(operation2.OutConversion);
            }

            public override bool VisitArrayCreation([NotNull] IArrayCreationOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IArrayCreationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitArrayElementReference([NotNull] IArrayElementReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IArrayElementReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitArrayInitializer([NotNull] IArrayInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IArrayInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitAwait([NotNull] IAwaitOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IAwaitOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitBinaryOperator([NotNull] IBinaryOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IBinaryOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.OperatorKind == operation2.OperatorKind &&
                    AreSymbolsEqual(operation1.OperatorMethod, operation2.OperatorMethod) &&
                    operation1.IsLifted == operation2.IsLifted && operation1.IsChecked == operation2.IsChecked &&
                    operation1.IsCompareText == operation2.IsCompareText;
            }

            public override bool VisitBlock([NotNull] IBlockOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IBlockOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolSequencesEqual(operation1.Locals, operation2.Locals);
            }

            public override bool VisitBranch([NotNull] IBranchOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IBranchOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Target, operation2.Target) && operation1.BranchKind == operation2.BranchKind;
            }

            public override bool VisitCatchClause([NotNull] ICatchClauseOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ICatchClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolSequencesEqual(operation1.Locals, operation2.Locals) &&
                    AreSymbolsEqual(operation1.ExceptionType, operation2.ExceptionType);
            }

            public override bool VisitCoalesce([NotNull] ICoalesceOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ICoalesceOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitCollectionElementInitializer([NotNull] ICollectionElementInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ICollectionElementInitializerOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.AddMethod, operation2.AddMethod) && operation1.IsDynamic == operation2.IsDynamic;
            }

            public override bool VisitCompoundAssignment([NotNull] ICompoundAssignmentOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ICompoundAssignmentOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.OperatorKind == operation2.OperatorKind &&
                    AreSymbolsEqual(operation1.OperatorMethod, operation2.OperatorMethod) &&
                    operation1.IsLifted == operation2.IsLifted && operation1.IsChecked == operation2.IsChecked &&
                    operation1.InConversion.Equals(operation2.InConversion) &&
                    operation1.OutConversion.Equals(operation2.OutConversion);
            }

            public override bool VisitConditional([NotNull] IConditionalOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IConditionalOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.IsRef == operation2.IsRef;
            }

            public override bool VisitConditionalAccess([NotNull] IConditionalAccessOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IConditionalAccessOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitConditionalAccessInstance([NotNull] IConditionalAccessInstanceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IConditionalAccessInstanceOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitConstantPattern([NotNull] IConstantPatternOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IConstantPatternOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitConversion([NotNull] IConversionOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IConversionOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.OperatorMethod, operation2.OperatorMethod) &&
                    operation1.Conversion.Equals(operation2.Conversion) && operation1.IsTryCast == operation2.IsTryCast &&
                    operation1.IsChecked == operation2.IsChecked;
            }

            public override bool VisitDeclarationExpression([NotNull] IDeclarationExpressionOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDeclarationExpressionOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDeclarationPattern([NotNull] IDeclarationPatternOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDeclarationPatternOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.DeclaredSymbol, operation2.DeclaredSymbol);
            }

            public override bool VisitDeconstructionAssignment([NotNull] IDeconstructionAssignmentOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDeconstructionAssignmentOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDefaultCaseClause([NotNull] IDefaultCaseClauseOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDefaultCaseClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.CaseKind == operation2.CaseKind;
            }

            public override bool VisitDefaultValue([NotNull] IDefaultValueOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IDefaultValueOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDelegateCreation([NotNull] IDelegateCreationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDelegateCreationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDynamicIndexerAccess([NotNull] IDynamicIndexerAccessOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDynamicIndexerAccessOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDynamicInvocation([NotNull] IDynamicInvocationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDynamicInvocationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitDynamicMemberReference([NotNull] IDynamicMemberReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDynamicMemberReferenceOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2) && operation1.MemberName == operation2.MemberName &&
                    AreSymbolSequencesEqual(operation1.TypeArguments, operation2.TypeArguments) &&
                    AreSymbolsEqual(operation1.ContainingType, operation2.ContainingType);
            }

            public override bool VisitDynamicObjectCreation([NotNull] IDynamicObjectCreationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IDynamicObjectCreationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitEmpty([NotNull] IEmptyOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IEmptyOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitEnd([NotNull] IEndOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IEndOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitEventAssignment([NotNull] IEventAssignmentOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IEventAssignmentOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.Adds == operation2.Adds;
            }

            public override bool VisitEventReference([NotNull] IEventReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IEventReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Event, operation2.Event) && AreSymbolsEqual(operation1.Member, operation2.Member);
            }

            public override bool VisitExpressionStatement([NotNull] IExpressionStatementOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IExpressionStatementOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitFieldInitializer([NotNull] IFieldInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IFieldInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolSequencesEqual(operation1.InitializedFields, operation2.InitializedFields);
            }

            public override bool VisitFieldReference([NotNull] IFieldReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IFieldReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Field, operation2.Field) &&
                    AreSymbolsEqual(operation1.Member, operation2.Member) && operation1.IsDeclaration == operation2.IsDeclaration;
            }

            public override bool VisitForEachLoop([NotNull] IForEachLoopOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IForEachLoopOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.LoopKind == operation2.LoopKind && AreSymbolSequencesEqual(operation1.Locals, operation2.Locals);
            }

            public override bool VisitForLoop([NotNull] IForLoopOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IForLoopOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.LoopKind == operation2.LoopKind && AreSymbolSequencesEqual(operation1.Locals, operation2.Locals);
            }

            public override bool VisitForToLoop([NotNull] IForToLoopOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IForToLoopOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.LoopKind == operation2.LoopKind && AreSymbolSequencesEqual(operation1.Locals, operation2.Locals);
            }

            public override bool VisitIncrementOrDecrement([NotNull] IIncrementOrDecrementOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IIncrementOrDecrementOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.OperatorMethod, operation2.OperatorMethod) &&
                    operation1.IsPostfix == operation2.IsPostfix && operation1.IsLifted == operation2.IsLifted &&
                    operation1.IsChecked == operation2.IsChecked;
            }

            public override bool VisitInstanceReference([NotNull] IInstanceReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IInstanceReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitInterpolatedString([NotNull] IInterpolatedStringOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IInterpolatedStringOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitInterpolatedStringText([NotNull] IInterpolatedStringTextOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IInterpolatedStringTextOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitInterpolation([NotNull] IInterpolationOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IInterpolationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitInvalid([NotNull] IInvalidOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IInvalidOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitInvocation([NotNull] IInvocationOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IInvocationOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.TargetMethod, operation2.TargetMethod) &&
                    operation1.IsVirtual == operation2.IsVirtual;
            }

            public override bool VisitIsPattern([NotNull] IIsPatternOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IIsPatternOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitIsType([NotNull] IIsTypeOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IIsTypeOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.TypeOperand, operation2.TypeOperand) &&
                    operation1.IsNegated == operation2.IsNegated;
            }

            public override bool VisitLabeled([NotNull] ILabeledOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ILabeledOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Label, operation2.Label);
            }

            public override bool VisitLiteral([NotNull] ILiteralOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ILiteralOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitLocalFunction([NotNull] ILocalFunctionOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ILocalFunctionOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Symbol, operation2.Symbol);
            }

            public override bool VisitLocalReference([NotNull] ILocalReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ILocalReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Local, operation2.Local) && operation1.IsDeclaration == operation2.IsDeclaration;
            }

            public override bool VisitLock([NotNull] ILockOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ILockOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitMemberInitializer([NotNull] IMemberInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IMemberInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitMethodReference([NotNull] IMethodReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IMethodReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Method, operation2.Method) && operation1.IsVirtual == operation2.IsVirtual &&
                    AreSymbolsEqual(operation1.Member, operation2.Member);
            }

            public override bool VisitNameOf([NotNull] INameOfOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is INameOfOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitObjectCreation([NotNull] IObjectCreationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IObjectCreationOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Constructor, operation2.Constructor);
            }

            public override bool VisitObjectOrCollectionInitializer([NotNull] IObjectOrCollectionInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IObjectOrCollectionInitializerOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitOmittedArgument([NotNull] IOmittedArgumentOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IOmittedArgumentOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitParameterInitializer([NotNull] IParameterInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IParameterInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Parameter, operation2.Parameter);
            }

            public override bool VisitParameterReference([NotNull] IParameterReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IParameterReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Parameter, operation2.Parameter);
            }

            public override bool VisitParenthesized([NotNull] IParenthesizedOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IParenthesizedOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitPatternCaseClause([NotNull] IPatternCaseClauseOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IPatternCaseClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Label, operation2.Label) && operation1.CaseKind == operation2.CaseKind;
            }

            public override bool VisitPropertyInitializer([NotNull] IPropertyInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IPropertyInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolSequencesEqual(operation1.InitializedProperties, operation2.InitializedProperties);
            }

            public override bool VisitPropertyReference([NotNull] IPropertyReferenceOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IPropertyReferenceOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Property, operation2.Property) &&
                    AreSymbolsEqual(operation1.Member, operation2.Member);
            }

            public override bool VisitRaiseEvent([NotNull] IRaiseEventOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IRaiseEventOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitRangeCaseClause([NotNull] IRangeCaseClauseOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IRangeCaseClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.CaseKind == operation2.CaseKind;
            }

            public override bool VisitRelationalCaseClause([NotNull] IRelationalCaseClauseOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IRelationalCaseClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.Relation == operation2.Relation && operation1.CaseKind == operation2.CaseKind;
            }

            public override bool VisitReturn([NotNull] IReturnOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IReturnOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ISimpleAssignmentOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.IsRef == operation2.IsRef;
            }

            public override bool VisitSingleValueCaseClause([NotNull] ISingleValueCaseClauseOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ISingleValueCaseClauseOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.CaseKind == operation2.CaseKind;
            }

            public override bool VisitSizeOf([NotNull] ISizeOfOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ISizeOfOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.TypeOperand, operation2.TypeOperand);
            }

            public override bool VisitStop([NotNull] IStopOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IStopOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitSwitch([NotNull] ISwitchOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ISwitchOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitSwitchCase([NotNull] ISwitchCaseOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ISwitchCaseOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitThrow([NotNull] IThrowOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IThrowOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitTranslatedQuery([NotNull] ITranslatedQueryOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ITranslatedQueryOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitTry([NotNull] ITryOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ITryOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitTuple([NotNull] ITupleOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ITupleOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitTypeOf([NotNull] ITypeOfOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is ITypeOfOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.TypeOperand, operation2.TypeOperand);
            }

            public override bool VisitTypeParameterObjectCreation([NotNull] ITypeParameterObjectCreationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is ITypeParameterObjectCreationOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitUnaryOperator([NotNull] IUnaryOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IUnaryOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.OperatorKind == operation2.OperatorKind &&
                    AreSymbolsEqual(operation1.OperatorMethod, operation2.OperatorMethod) &&
                    operation1.IsLifted == operation2.IsLifted && operation1.IsChecked == operation2.IsChecked;
            }

            public override bool VisitUsing([NotNull] IUsingOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IUsingOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitVariableDeclaration([NotNull] IVariableDeclarationOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IVariableDeclarationOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitVariableDeclarationGroup([NotNull] IVariableDeclarationGroupOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IVariableDeclarationGroupOperation operation2 &&
                    AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IVariableDeclaratorOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    AreSymbolsEqual(operation1.Symbol, operation2.Symbol);
            }

            public override bool VisitVariableInitializer([NotNull] IVariableInitializerOperation operation1,
                [CanBeNull] IOperation argument)
            {
                return argument is IVariableInitializerOperation operation2 && AreBaseOperationsEqual(operation1, operation2);
            }

            public override bool VisitWhileLoop([NotNull] IWhileLoopOperation operation1, [CanBeNull] IOperation argument)
            {
                return argument is IWhileLoopOperation operation2 && AreBaseOperationsEqual(operation1, operation2) &&
                    operation1.ConditionIsTop == operation2.ConditionIsTop &&
                    operation1.ConditionIsUntil == operation2.ConditionIsUntil && operation1.LoopKind == operation2.LoopKind &&
                    AreSymbolSequencesEqual(operation1.Locals, operation2.Locals);
            }

            private bool AreBaseOperationsEqual([CanBeNull] IOperation operation1, [CanBeNull] IOperation operation2)
            {
                if (ReferenceEquals(operation1, operation2))
                {
                    return true;
                }

                if (operation1 is null || operation2 is null)
                {
                    return false;
                }

                return operation1.Kind == operation2.Kind && operation1.GetType() == operation2.GetType() &&
                    AreSymbolsEqual(operation1.Type, operation2.Type) &&
                    operation1.ConstantValue.Equals(operation2.ConstantValue) && operation1.Language == operation2.Language &&
                    operation1.IsImplicit == operation2.IsImplicit &&
                    operation1.Children.SequenceEqual(operation2.Children, equalityComparerForOperations);
            }

            private static bool AreSymbolsEqual([CanBeNull] ISymbol symbol1, [CanBeNull] ISymbol symbol2)
            {
                if (ReferenceEquals(symbol1, symbol2))
                {
                    return true;
                }

                if (symbol1 is null)
                {
                    return false;
                }

                return symbol1.Equals(symbol2);
            }

            private bool AreSymbolSequencesEqual([NotNull] [ItemNotNull] IEnumerable<ISymbol> sequence1,
                [NotNull] [ItemNotNull] IEnumerable<ISymbol> sequence2)
            {
                return sequence1.SequenceEqual(sequence2, equalityComparerForSymbols);
            }

            private sealed class DelegatingEqualityComparer<T> : IEqualityComparer<T>
            {
                [NotNull]
                private readonly Func<T, T, bool> equalityComparison;

                public DelegatingEqualityComparer([NotNull] Func<T, T, bool> equalityComparison)
                {
                    Guard.NotNull(equalityComparison, nameof(equalityComparison));
                    this.equalityComparison = equalityComparison;
                }

                public bool Equals(T x, T y)
                {
                    return equalityComparison(x, y);
                }

                public int GetHashCode(T obj)
                {
                    return obj.GetHashCode();
                }
            }
        }
    }
}
