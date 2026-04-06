using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeShouldHaveASinglePurposeAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Type name contains the word 'and', which suggests it has multiple purposes";
    private const string MessageFormat = "Type '{0}' contains the word 'and', which suggests it has multiple purposes";
    private const string Description = "A class or interface should have a single purpose.";
    private const string BlacklistWord = "and";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1000";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly SyntaxKind[] TypeDeclarationKinds =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.DelegateDeclaration
    ];

    private static readonly TypeIdentifierResolver IdentifierResolver = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, TypeDeclarationKinds);
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
            Location location = identifier.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ContainsBlacklistedWord(string name)
    {
        return name.ContainsWordInTheMiddle(BlacklistWord);
    }

    private sealed class TypeIdentifierResolver : CSharpSyntaxVisitor<SyntaxToken>
    {
        public override SyntaxToken VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return node.Identifier;
        }

        public override SyntaxToken VisitStructDeclaration(StructDeclarationSyntax node)
        {
            return node.Identifier;
        }

        public override SyntaxToken VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            return node.Identifier;
        }

        public override SyntaxToken VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            return node.Identifier;
        }

        public override SyntaxToken VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            return node.Identifier;
        }
    }
}
