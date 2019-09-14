using System;
using System.Collections.Immutable;
using System.Linq;
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
        public const string DiagnosticId = "AV2220";

        private const string Title = "Simple query should be replaced by extension method call";
        private const string MessageFormat = "Simple query should be replaced by extension method call.";
        private const string Description = "Avoid LINQ query syntax for simple expressions.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeQueryExpressionAction = AnalyzeQueryExpression;

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeQueryExpressionAction, SyntaxKind.QueryExpression);
        }

        private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
        {
            var queryExpression = (QueryExpressionSyntax)context.Node;

            if (queryExpression.Body.IsMissing || queryExpression.Body.Continuation != null)
            {
                return;
            }

            int complexity = CalculateComplexity(queryExpression);

            if (complexity <= 1 && !IsIncomplete(queryExpression))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, queryExpression.GetLocation()));
            }
        }

        private static int CalculateComplexity([NotNull] QueryExpressionSyntax queryExpression)
        {
            int complexity = 0;

            if (HasCastInFromClause(queryExpression))
            {
                complexity++;
            }

            if (ProjectsIntoGroup(queryExpression.Body.SelectOrGroup) ||
                ProjectsIntoAnonymousType(queryExpression.Body.SelectOrGroup))
            {
                complexity++;
            }

            complexity += GetComplexityForClauses(queryExpression);

            return complexity;
        }

        private static bool HasCastInFromClause([NotNull] QueryExpressionSyntax queryExpression)
        {
            return queryExpression.FromClause.Type != null;
        }

        private static bool ProjectsIntoGroup([NotNull] SelectOrGroupClauseSyntax selectOrGroupClause)
        {
            return selectOrGroupClause is GroupClauseSyntax;
        }

        private static bool ProjectsIntoAnonymousType([NotNull] SelectOrGroupClauseSyntax selectOrGroupClause)
        {
            return selectOrGroupClause is SelectClauseSyntax selectClause &&
                selectClause.Expression is AnonymousObjectCreationExpressionSyntax;
        }

        private static int GetComplexityForClauses([NotNull] QueryExpressionSyntax queryExpression)
        {
            int complexity = 0;

            foreach (QueryClauseSyntax bodyClause in queryExpression.Body.Clauses)
            {
                complexity += GetComplexityForClause(bodyClause);
            }

            return complexity;
        }

        private static int GetComplexityForClause([NotNull] QueryClauseSyntax bodyClause)
        {
            if (bodyClause is OrderByClauseSyntax orderByClause)
            {
                return orderByClause.Orderings.Count;
            }

            return bodyClause is LetClauseSyntax ? 2 : 1;
        }

        private static bool IsIncomplete([NotNull] SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodesAndSelf().Any(node => node.IsMissing);
        }
    }
}
