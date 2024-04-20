using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidQuerySyntaxForSimpleExpressionAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Simple query should be replaced by extension method call";
        private const string MessageFormat = "Simple query should be replaced by extension method call";
        private const string Description = "Avoid LINQ query syntax for simple expressions.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2220";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeQueryExpressionAction = AnalyzeQueryExpression;

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeQueryExpressionAction, SyntaxKind.QueryExpression);
        }

        private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
        {
            var queryExpression = (QueryExpressionSyntax)context.Node;

            var walker = new QueryExpressionWalker();
            walker.Visit(queryExpression);

            if (walker.SkipAlways || walker.TotalCount > 1)
            {
                return;
            }

            Location location = GetLocation(queryExpression, context.SemanticModel);
            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }

        [NotNull]
        private static Location GetLocation([NotNull] QueryExpressionSyntax queryExpression, [NotNull] SemanticModel semanticModel)
        {
            SyntaxNode parent = SkipParentParentheses(queryExpression.Parent);

            if (parent is MemberAccessExpressionSyntax memberAccess)
            {
                SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(memberAccess);

                if (symbolInfo.Symbol is IMethodSymbol methodSymbol && IsEnumerableExtensionMethod(methodSymbol))
                {
                    // Expand location to the containing .ToArray()/.ToList()/... call.
                    return memberAccess.Parent.GetLocation();
                }
            }

            return queryExpression.GetLocation();
        }

        [NotNull]
        private static SyntaxNode SkipParentParentheses([NotNull] SyntaxNode syntax)
        {
            SyntaxNode current;

            for (current = syntax; current is ParenthesizedExpressionSyntax && current.Parent != null; current = current.Parent)
            {
            }

            return current;
        }

        private static bool IsEnumerableExtensionMethod([NotNull] IMethodSymbol method)
        {
            return method is { IsExtensionMethod: true, IsGenericMethod: true } && method.Name.StartsWith("To", StringComparison.Ordinal) &&
                method.ReturnType.IsOrImplementsIEnumerable();
        }

        private sealed class QueryExpressionWalker : CSharpSyntaxWalker
        {
            private int fromCount;
            private int castsInFromCount;
            private int whereCount;
            private int groupCount;
            private int orderCount;
            private int selectCount;

            public bool SkipAlways { get; private set; }
            public int TotalCount => Math.Max(0, fromCount - 1) + castsInFromCount + whereCount + groupCount + orderCount + selectCount;

            public override void Visit([NotNull] SyntaxNode node)
            {
                if (node.IsMissing)
                {
                    SkipAlways = true;
                }
                else
                {
                    base.Visit(node);
                }
            }

            public override void VisitJoinClause([NotNull] JoinClauseSyntax node)
            {
                SkipAlways = true;
            }

            public override void VisitLetClause([NotNull] LetClauseSyntax node)
            {
                SkipAlways = true;
            }

            public override void VisitFromClause([NotNull] FromClauseSyntax node)
            {
                if (node.Type != null)
                {
                    castsInFromCount++;
                }

                fromCount++;
                base.VisitFromClause(node);
            }

            public override void VisitWhereClause([NotNull] WhereClauseSyntax node)
            {
                whereCount++;
                base.VisitWhereClause(node);
            }

            public override void VisitGroupClause([NotNull] GroupClauseSyntax node)
            {
                groupCount++;
                base.VisitGroupClause(node);
            }

            public override void VisitOrdering([NotNull] OrderingSyntax node)
            {
                orderCount++;
                base.VisitOrdering(node);
            }

            public override void VisitSelectClause([NotNull] SelectClauseSyntax node)
            {
                if (node.Expression is IdentifierNameSyntax)
                {
                    // In method syntax, .Select() can be omitted when it returns the incoming parameter unmodified.
                }
                else
                {
                    selectCount++;
                }

                base.VisitSelectClause(node);
            }
        }
    }
}
