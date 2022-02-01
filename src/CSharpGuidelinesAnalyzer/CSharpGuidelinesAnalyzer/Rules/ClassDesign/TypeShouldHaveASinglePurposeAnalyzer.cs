using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeShouldHaveASinglePurposeAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Type name contains the word 'and', which suggests it has multiple purposes";
        private const string MessageFormat = "Type '{0}' contains the word 'and', which suggests it has multiple purposes";
        private const string Description = "A class or interface should have a single purpose.";
        private const string BlacklistWord = "and";

        public const string DiagnosticId = "AV1000";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeTypeDeclarationAction = AnalyzeTypeDeclaration;

        [NotNull]
        private static readonly SyntaxKind[] TypeDeclarationKinds =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration
        };

        [NotNull]
        private static readonly TypeIdentifierResolver IdentifierResolver = new TypeIdentifierResolver();

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclarationAction, TypeDeclarationKinds);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken identifier = IdentifierResolver.Visit(context.Node);

            if (identifier == default || string.IsNullOrEmpty(identifier.ValueText))
            {
                return;
            }

            if (ContainsBlacklistedWord(identifier.ValueText))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
            }
        }

        private static bool ContainsBlacklistedWord([NotNull] string name)
        {
            return name.ContainsWordInTheMiddle(BlacklistWord);
        }

        private sealed class TypeIdentifierResolver : CSharpSyntaxVisitor<SyntaxToken>
        {
            public override SyntaxToken VisitClassDeclaration([NotNull] ClassDeclarationSyntax node)
            {
                return node.Identifier;
            }

            public override SyntaxToken VisitStructDeclaration([NotNull] StructDeclarationSyntax node)
            {
                return node.Identifier;
            }

            public override SyntaxToken VisitInterfaceDeclaration([NotNull] InterfaceDeclarationSyntax node)
            {
                return node.Identifier;
            }

            public override SyntaxToken VisitEnumDeclaration([NotNull] EnumDeclarationSyntax node)
            {
                return node.Identifier;
            }

            public override SyntaxToken VisitDelegateDeclaration([NotNull] DelegateDeclarationSyntax node)
            {
                return node.Identifier;
            }
        }
    }
}
