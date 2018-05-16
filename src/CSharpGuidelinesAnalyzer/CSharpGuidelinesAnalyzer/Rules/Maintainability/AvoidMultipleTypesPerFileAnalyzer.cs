using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMultipleTypesPerFileAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1507";

        private const string Title = "File contains multiple types";
        private const string MessageFormat = "File '{0}' contains additional type '{1}'.";
        private const string Description = "Limit the contents of a source code file to one type.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            SyntaxNode syntaxRoot = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            var walker = new TopLevelTypeSyntaxWalker();
            walker.Visit(syntaxRoot);

            context.CancellationToken.ThrowIfCancellationRequested();

            ReportWalkerResult(walker, context);
        }

        private static void ReportWalkerResult([NotNull] TopLevelTypeSyntaxWalker walker, SemanticModelAnalysisContext context)
        {
            if (walker.TopLevelTypeDeclarations.Count > 1)
            {
                foreach (SyntaxNode extraTypeSyntax in walker.TopLevelTypeDeclarations.Skip(1))
                {
                    ReportType(context, extraTypeSyntax);
                }
            }
        }

        private static void ReportType(SemanticModelAnalysisContext context, [NotNull] SyntaxNode typeSyntax)
        {
            string fileName = Path.GetFileName(context.SemanticModel.SyntaxTree.FilePath);
            ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax, context.CancellationToken);
            string typeName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], fileName, typeName));
        }

        private sealed class TopLevelTypeSyntaxWalker : SyntaxWalker
        {
            private bool isInType;

            [NotNull]
            [ItemNotNull]
            public IList<SyntaxNode> TopLevelTypeDeclarations { get; } = new List<SyntaxNode>();

            public override void Visit([NotNull] SyntaxNode node)
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

            private void VisitTypeDeclaration([NotNull] SyntaxNode node)
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

            private bool IsTypeDeclaration([NotNull] SyntaxNode node)
            {
                return node.IsKind(SyntaxKind.ClassDeclaration) || node.IsKind(SyntaxKind.StructDeclaration) ||
                    node.IsKind(SyntaxKind.EnumDeclaration) || node.IsKind(SyntaxKind.InterfaceDeclaration) ||
                    node.IsKind(SyntaxKind.DelegateDeclaration);
            }
        }
    }
}
