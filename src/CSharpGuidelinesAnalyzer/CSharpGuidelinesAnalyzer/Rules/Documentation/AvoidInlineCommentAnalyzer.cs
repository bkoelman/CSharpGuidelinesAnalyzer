using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Documentation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidInlineCommentAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Code block should not contain inline comment";
    private const string MessageFormat = "Code block should not contain inline comment";
    private const string Description = "Avoid inline comments.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2310";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Documentation;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, false,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    private static readonly ImmutableArray<string> ArrangeActAssertLines = ImmutableArray.Create("// Arrange", "// Act", "// Assert", "// Act and assert");

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCodeBlockAction(AnalyzeCodeBlock);
    }

    private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
    {
        SyntaxTriviaList trailingTrivia = context.CodeBlock.GetTrailingTrivia();
        SyntaxTrivia[] outerCommentTrivia = context.CodeBlock.GetLeadingTrivia().Concat(trailingTrivia).Where(IsComment).ToArray();

        AnalyzeCommentTrivia(outerCommentTrivia, context);
    }

    private static void AnalyzeCommentTrivia([NotNull] SyntaxTrivia[] outerCommentTrivia, CodeBlockAnalysisContext context)
    {
        foreach (SyntaxTrivia commentTrivia in context.CodeBlock.DescendantTrivia().Where(IsComment))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!outerCommentTrivia.Contains(commentTrivia) && !IsCommentInEmptyElseClause(commentTrivia))
            {
                string commentText = commentTrivia.ToString();

                if (!IsResharperDirective(commentText) && !IsArrangeActAssertUnitTestPattern(commentText))
                {
                    Location location = commentTrivia.GetLocation();

                    var diagnostic = Diagnostic.Create(Rule, location);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsComment(SyntaxTrivia trivia)
    {
        SyntaxKind kind = trivia.Kind();
        return kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia;
    }

    private static bool IsCommentInEmptyElseClause(SyntaxTrivia commentTrivia)
    {
        return commentTrivia.Token.Parent is BlockSyntax parentBlock && !parentBlock.Statements.Any() && parentBlock.Parent is ElseClauseSyntax;
    }

    private static bool IsResharperDirective([NotNull] string commentText)
    {
        return IsResharperSuppression(commentText) || IsResharperLanguageInjection(commentText) || IsResharperFormatterConfiguration(commentText);
    }

    private static bool IsResharperSuppression([NotNull] string commentText)
    {
        return commentText.Contains("// ReSharper disable ") || commentText.Contains("// ReSharper restore ");
    }

    private static bool IsResharperLanguageInjection([NotNull] string commentText)
    {
        return commentText.Contains("language=");
    }

    private static bool IsResharperFormatterConfiguration([NotNull] string commentText)
    {
        return commentText.Contains("// @formatter:");
    }

    private static bool IsArrangeActAssertUnitTestPattern([NotNull] string commentText)
    {
        return ArrangeActAssertLines.Any(line => line.Equals(commentText));
    }
}
