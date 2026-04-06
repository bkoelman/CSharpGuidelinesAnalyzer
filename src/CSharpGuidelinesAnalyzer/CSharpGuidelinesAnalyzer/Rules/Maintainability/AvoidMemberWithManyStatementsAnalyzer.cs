using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using CSharpGuidelinesAnalyzer.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidMemberWithManyStatementsAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMaxStatementCount = 7;

    private const string Title = "Member or local function contains too many statements";
    private const string MessageFormat = "{0} '{1}' contains {2} statements, which exceeds the maximum of {3} statements";
    private const string Description = "Methods should not exceed a predefined number of statements.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1500";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly AnalyzerSettingKey MaxStatementCountKey = new(DiagnosticId, "MaxStatementCount");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        ArgumentNullException.ThrowIfNull(startContext);

        var settingsReader = new AnalyzerSettingsReader(startContext.Options, startContext.CancellationToken);

        startContext.RegisterCodeBlockAction(context => AnalyzeCodeBlock(context, settingsReader));
    }

    private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context, AnalyzerSettingsReader settingsReader)
    {
        if (context.OwningSymbol is INamedTypeSymbol || context.OwningSymbol.IsSynthesized() || IsPrimaryConstructorInitializer(context.CodeBlock))
        {
            return;
        }

        int maxStatementCount = GetMaxStatementCountFromSettings(settingsReader, context.CodeBlock.SyntaxTree);

        var statementWalker = new StatementWalker(context.CancellationToken);
        statementWalker.Visit(context.CodeBlock);

        if (statementWalker.StatementCount > maxStatementCount)
        {
            ReportAtContainingSymbol(statementWalker.StatementCount, maxStatementCount, context);
        }
    }

    private static bool IsPrimaryConstructorInitializer(SyntaxNode codeBlock)
    {
        return codeBlock is BaseTypeDeclarationSyntax;
    }

    private static int GetMaxStatementCountFromSettings(AnalyzerSettingsReader settingsReader, SyntaxTree syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(settingsReader);
        ArgumentNullException.ThrowIfNull(syntaxTree);

        return settingsReader.TryGetInt32(syntaxTree, MaxStatementCountKey, 0, 255) ?? DefaultMaxStatementCount;
    }

    private static void ReportAtContainingSymbol(int statementCount, int maxStatementCount, CodeBlockAnalysisContext context)
    {
        string kind = GetMemberKind(context.OwningSymbol, context.CancellationToken);
        string memberName = context.OwningSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        Location location = GetMemberLocation(context.OwningSymbol, context.SemanticModel, context.CancellationToken);

        var diagnostic = Diagnostic.Create(Rule, location, kind, memberName, statementCount, maxStatementCount);
        context.ReportDiagnostic(diagnostic);
    }

    private static string GetMemberKind(ISymbol member, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(member);

        foreach (SyntaxNode syntax in member.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(cancellationToken)))
        {
            if (syntax is VariableDeclaratorSyntax or PropertyDeclarationSyntax)
            {
                return "Initializer for";
            }
        }

        return member.GetKind();
    }

    private static Location GetMemberLocation(ISymbol member, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (ArrowExpressionClauseSyntax arrowExpressionClause in member.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax(cancellationToken)).OfType<ArrowExpressionClauseSyntax>())
        {
            ISymbol parentSymbol = semanticModel.GetDeclaredSymbol(arrowExpressionClause.Parent);

            if (parentSymbol != null && parentSymbol.Locations.Any())
            {
                return parentSymbol.Locations[0];
            }
        }

        return member.Locations[0];
    }

    private sealed class StatementWalker(CancellationToken cancellationToken) : CSharpSyntaxWalker
    {
        public int StatementCount { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsStatement(node))
            {
                StatementCount++;
            }

            base.Visit(node);
        }

        private bool IsStatement(SyntaxNode node)
        {
            return !node.IsMissing && node is StatementSyntax && !IsExcludedStatement(node);
        }

        private bool IsExcludedStatement(SyntaxNode node)
        {
            return node is BlockSyntax or LabeledStatementSyntax or LocalFunctionStatementSyntax;
        }
    }
}
