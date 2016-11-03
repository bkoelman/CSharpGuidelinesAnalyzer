using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMultipleTypesPerFileAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1507";

        private const string Title = "File contains multiple types.";
        private const string MessageFormat = "File '{0}' contains additional type '{1}'.";
        private const string Description = "Limit the contents of a source code file to one type.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

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

            if (walker.TopLevelTypeDeclarations.Count > 1)
            {
                foreach (SyntaxNode extraTypeSyntax in walker.TopLevelTypeDeclarations.Skip(1))
                {
                    string fileName = Path.GetFileName(context.SemanticModel.SyntaxTree.FilePath);
                    ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(extraTypeSyntax, context.CancellationToken);
                    string typeName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

                    context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], fileName, typeName));
                }
            }
        }

        private sealed class TopLevelTypeSyntaxWalker : SyntaxWalker
        {
            private bool IsInType;

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
                if (!IsInType)
                {
                    TopLevelTypeDeclarations.Add(node);

                    IsInType = true;

                    base.Visit(node);

                    IsInType = false;
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