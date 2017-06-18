using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class OperationExtensions
    {
        [NotNull]
        private static readonly ConcurrentDictionary<Type, MethodInfo> OperationCompilerGeneratedCache =
            new ConcurrentDictionary<Type, MethodInfo>();

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
                var name = new IdentifierName(operation.Local.Name,
                    operation.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat));
                return new IdentifierInfo(name, operation.Local.Type, "Variable");
            }

            [NotNull]
            public override IdentifierInfo VisitParameterReferenceExpression([NotNull] IParameterReferenceExpression operation,
                [CanBeNull] object argument)
            {
                var name = new IdentifierName(operation.Parameter.Name,
#pragma warning disable AV2310 // Code blocks should not contain inline comments
                    /* CSharpShortErrorMessageFormat returns 'ref int', ie. without parameter name */
#pragma warning restore AV2310 // Code blocks should not contain inline comments
                    operation.Parameter.Name);
                return new IdentifierInfo(name, operation.Parameter.Type, operation.Parameter.Kind.ToString());
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
            public override IdentifierInfo VisitPropertyReferenceExpression([NotNull] IPropertyReferenceExpression operation,
                [CanBeNull] object argument)
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
                var name = new IdentifierName(operation.Member.Name,
                    operation.Member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat));
                return new IdentifierInfo(name, memberType, operation.Member.Kind.ToString());
            }

            [NotNull]
            public override IdentifierInfo VisitInvocationExpression([NotNull] IInvocationExpression operation,
                [CanBeNull] object argument)
            {
                var name = new IdentifierName(operation.TargetMethod.Name,
                    operation.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat));
                return new IdentifierInfo(name, operation.TargetMethod.ReturnType, operation.TargetMethod.Kind.ToString());
            }
        }

        [CanBeNull]
        public static Location GetLocationForKeyword([NotNull] this IOperation operation)
        {
            var visitor = new OperationLocationVisitor();
            return visitor.Visit(operation, null);
        }

        private sealed class OperationLocationVisitor : OperationVisitor<object, Location>
        {
            [NotNull]
            public override Location VisitWhileUntilLoopStatement([NotNull] IWhileUntilLoopStatement operation,
                [CanBeNull] object argument)
            {
                var doSyntax = operation.Syntax as DoStatementSyntax;
                if (doSyntax != null)
                {
                    return doSyntax.DoKeyword.GetLocation();
                }

                var whileSyntax = operation.Syntax as WhileStatementSyntax;
                if (whileSyntax != null)
                {
                    return whileSyntax.WhileKeyword.GetLocation();
                }

                throw ExceptionFactory.Unreachable();
            }

            [NotNull]
            public override Location VisitForLoopStatement([NotNull] IForLoopStatement operation, [CanBeNull] object argument)
            {
                var syntax = (ForStatementSyntax)operation.Syntax;
                return syntax.ForKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitForEachLoopStatement([NotNull] IForEachLoopStatement operation,
                [CanBeNull] object argument)
            {
                var syntax = (ForEachStatementSyntax)operation.Syntax;
                return syntax.ForEachKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitReturnStatement([NotNull] IReturnStatement operation, [CanBeNull] object argument)
            {
                return GetLocationForReturnOrYield(operation);
            }

            [NotNull]
            public override Location VisitYieldBreakStatement([NotNull] IReturnStatement operation, [CanBeNull] object argument)
            {
                return GetLocationForReturnOrYield(operation);
            }

            [NotNull]
            private static Location GetLocationForReturnOrYield([NotNull] IReturnStatement operation)
            {
                var returnSyntax = operation.Syntax as ReturnStatementSyntax;
                if (returnSyntax != null)
                {
                    return returnSyntax.ReturnKeyword.GetLocation();
                }

                var yieldSyntax = operation.Syntax as YieldStatementSyntax;
                if (yieldSyntax != null)
                {
                    return GetLocationForYieldStatement(yieldSyntax);
                }

                throw ExceptionFactory.Unreachable();
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
            public override Location VisitIfStatement([NotNull] IIfStatement operation, [CanBeNull] object argument)
            {
                var syntax = (IfStatementSyntax)operation.Syntax;
                return syntax.IfKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitUsingStatement([NotNull] IUsingStatement operation, [CanBeNull] object argument)
            {
                var syntax = (UsingStatementSyntax)operation.Syntax;
                return syntax.UsingKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitLockStatement([NotNull] ILockStatement operation, [CanBeNull] object argument)
            {
                var syntax = (LockStatementSyntax)operation.Syntax;
                return syntax.LockKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitSwitchStatement([NotNull] ISwitchStatement operation, [CanBeNull] object argument)
            {
                var syntax = (SwitchStatementSyntax)operation.Syntax;
                return syntax.SwitchKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitThrowStatement([NotNull] IThrowStatement operation, [CanBeNull] object argument)
            {
                var syntax = (ThrowStatementSyntax)operation.Syntax;
                return syntax.ThrowKeyword.GetLocation();
            }

            [NotNull]
            public override Location VisitSingleValueCaseClause([NotNull] ISingleValueCaseClause operation,
                [CanBeNull] object argument)
            {
                var syntax = (SwitchLabelSyntax)operation.Syntax;
                return syntax.Keyword.GetLocation();
            }
        }

        public static bool IsCompilerGenerated([CanBeNull] this IOperation operation)
        {
            Type type = operation?.GetType();
            if (type != null)
            {
                MethodInfo compilerGeneratedGetter = GetOperationCompilerGeneratedGetterFor(type);
                if (compilerGeneratedGetter != null)
                {
                    return (bool)compilerGeneratedGetter.Invoke(operation, null);
                }
            }

            return false;
        }

        [CanBeNull]
        private static MethodInfo GetOperationCompilerGeneratedGetterFor([NotNull] Type type)
        {
            MethodInfo compilerGeneratedGetter;
            if (!OperationCompilerGeneratedCache.TryGetValue(type, out compilerGeneratedGetter))
            {
                PropertyInfo property = type.GetRuntimeProperty("WasCompilerGenerated");
                compilerGeneratedGetter = property?.GetMethod;

                OperationCompilerGeneratedCache.TryAdd(type, compilerGeneratedGetter);
            }

            return compilerGeneratedGetter;
        }

        public static bool IsInvalid([NotNull] this IOperation operation, [NotNull] Compilation compilation,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NotNull(operation, nameof(operation));
            Guard.NotNull(compilation, nameof(compilation));

            if (operation.Syntax != null)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);

                if (!model.GetDiagnostics(operation.Syntax.Span, cancellationToken).Any(IsError))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsError([NotNull] Diagnostic diagnostic)
        {
            return diagnostic.Severity == DiagnosticSeverity.Error;
        }
    }
}
