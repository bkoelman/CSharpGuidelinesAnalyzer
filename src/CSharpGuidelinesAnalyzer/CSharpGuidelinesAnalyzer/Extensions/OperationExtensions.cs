using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class OperationExtensions
    {
        [CanBeNull]
        public static IdentifierInfo TryGetIdentifierInfo([CanBeNull] this IOperation identifier)
        {
            var visitor = new IdentifierVisitor();
            return visitor.Visit(identifier, null);
        }

        private sealed class IdentifierVisitor : OperationVisitor<object, IdentifierInfo>
        {
            [NotNull]
            public override IdentifierInfo VisitLocalReferenceExpression([NotNull] ILocalReferenceExpression operation,
                [CanBeNull] object argument)
            {
                return new IdentifierInfo(operation.Local.Name,
                    operation.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    operation.Local.Type, "Variable");
            }

            [NotNull]
            public override IdentifierInfo VisitParameterReferenceExpression(
                [NotNull] IParameterReferenceExpression operation, [CanBeNull] object argument)
            {
                return new IdentifierInfo(operation.Parameter.Name,
                    /* CSharpShortErrorMessageFormat returns 'ref int', ie. without parameter name */
                    operation.Parameter.Name, operation.Parameter.Type, operation.Parameter.Kind.ToString());
            }

            [NotNull]
            public override IdentifierInfo VisitFieldReferenceExpression([NotNull] IFieldReferenceExpression operation,
                [CanBeNull] object argument)
            {
                return CreateForMemberReferenceExpression(operation, operation.Field.Type);
            }

            [NotNull]
            public override IdentifierInfo VisitEventReferenceExpression([NotNull] IEventReferenceExpression operation,
                [CanBeNull] object argument)
            {
                return CreateForMemberReferenceExpression(operation, operation.Event.Type);
            }

            [NotNull]
            public override IdentifierInfo VisitPropertyReferenceExpression(
                [NotNull] IPropertyReferenceExpression operation, [CanBeNull] object argument)
            {
                return CreateForMemberReferenceExpression(operation, operation.Property.Type);
            }

            [NotNull]
            public override IdentifierInfo VisitIndexedPropertyReferenceExpression(
                [NotNull] IIndexedPropertyReferenceExpression operation, [CanBeNull] object argument)
            {
                return CreateForMemberReferenceExpression(operation, operation.Property.Type);
            }

            [NotNull]
            private IdentifierInfo CreateForMemberReferenceExpression([NotNull] IMemberReferenceExpression operation,
                [NotNull] ITypeSymbol memberType)
            {
                return new IdentifierInfo(operation.Member.Name,
                    operation.Member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), memberType,
                    operation.Member.Kind.ToString());
            }

            [NotNull]
            public override IdentifierInfo VisitInvocationExpression([NotNull] IInvocationExpression operation,
                [CanBeNull] object argument)
            {
                return new IdentifierInfo(operation.TargetMethod.Name,
                    operation.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    operation.TargetMethod.ReturnType, operation.TargetMethod.Kind.ToString());
            }
        }
    }
}