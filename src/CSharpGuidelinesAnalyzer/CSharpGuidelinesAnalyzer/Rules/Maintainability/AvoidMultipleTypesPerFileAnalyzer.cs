using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidMultipleTypesPerFileAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "File contains multiple types";
    private const string MessageFormat = "File '{0}' contains additional type '{1}'";
    private const string Description = "Limit the contents of a source code file to one type.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1507";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSemanticModelAction(AnalyzeSemanticModel);
    }

    private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
    {
        SyntaxNode syntaxRoot = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        var walker = new TopLevelTypeSyntaxWalker();
        walker.Visit(syntaxRoot);

        context.CancellationToken.ThrowIfCancellationRequested();

        ReportWalkerResult(walker, context);
    }

    private static void ReportWalkerResult(TopLevelTypeSyntaxWalker walker, SemanticModelAnalysisContext context)
    {
        if (walker.TopLevelTypeDeclarations.Count > 1)
        {
            SyntaxNode firstTypeSyntax = walker.TopLevelTypeDeclarations.First();
            string firstTypeName = GetTypeName(firstTypeSyntax);

            foreach (SyntaxNode extraTypeSyntax in walker.TopLevelTypeDeclarations.Skip(1).Where(typeSyntax => GetTypeName(typeSyntax) != firstTypeName))
            {
                ReportType(context, extraTypeSyntax);
            }
        }
    }

    private static string GetTypeName(SyntaxNode syntax)
    {
        switch (syntax)
        {
            case BaseTypeDeclarationSyntax typeSyntax:
            {
                return typeSyntax.Identifier.ValueText;
            }
            case DelegateDeclarationSyntax delegateSyntax:
            {
                return delegateSyntax.Identifier.ValueText;
            }
            default:
            {
                throw new NotSupportedException($"Unknown type declaration '{syntax.GetType().Name}'.");
            }
        }
    }

    private static void ReportType(SemanticModelAnalysisContext context, SyntaxNode typeSyntax)
    {
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax, context.CancellationToken);

        if (symbol != null && !symbol.IsSynthesized())
        {
            string? fileName = Path.GetFileName(context.SemanticModel.SyntaxTree.FilePath);

            if (!string.IsNullOrEmpty(fileName))
            {
                string typeName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], fileName, typeName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private sealed class TopLevelTypeSyntaxWalker : SyntaxWalker
    {
        private bool isInType;

        public IList<SyntaxNode> TopLevelTypeDeclarations { get; } = [];

        public override void Visit(SyntaxNode node)
        {
            if (IsTypeDeclaration(node))
            {
                VisitTypeDeclaration(node);
            }
            else
            {
                base.Visit(node);
            }
        }

        private void VisitTypeDeclaration(SyntaxNode node)
        {
            if (!isInType)
            {
                TopLevelTypeDeclarations.Add(node);

                isInType = true;

                base.Visit(node);

                isInType = false;
            }
            else
            {
                base.Visit(node);
            }
        }

        private bool IsTypeDeclaration(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.ClassDeclaration) || node.IsKind(SyntaxKind.StructDeclaration) || node.IsKind(SyntaxKind.EnumDeclaration) ||
                node.IsKind(SyntaxKind.InterfaceDeclaration) || node.IsKind(SyntaxKind.DelegateDeclaration);
        }
    }
}
