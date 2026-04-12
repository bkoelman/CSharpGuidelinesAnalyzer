using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SwitchStatementShouldHaveDefaultCaseAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Non-exhaustive switch statement requires a default case clause";
    private const string MessageFormat = "Non-exhaustive switch statement requires a default case clause";
    private const string Description = "Always add a default block after the last case in a switch statement.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1536";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer
    private static readonly ISymbol?[] NullSymbolArray = [null];
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        INamedTypeSymbol systemBoolean = KnownTypes.SystemBoolean(startContext.Compilation);

        startContext.SafeRegisterOperationAction(context => AnalyzeSwitchStatement(context, systemBoolean), OperationKind.Switch);
    }

    private static void AnalyzeSwitchStatement(OperationAnalysisContext context, INamedTypeSymbol systemBoolean)
    {
        var switchStatement = (ISwitchOperation)context.Operation;

        if (HasDefaultOrPatternCase(switchStatement))
        {
            return;
        }

        context.CancellationToken.ThrowIfCancellationRequested();

        AnalyzeSwitchExhaustiveness(switchStatement, systemBoolean, context);
    }

    private static void AnalyzeSwitchExhaustiveness(ISwitchOperation switchStatement, INamedTypeSymbol systemBoolean, OperationAnalysisContext context)
    {
        var analysisContext = new SwitchAnalysisContext(switchStatement, systemBoolean, context);

        if (IsSwitchExhaustive(analysisContext) == false)
        {
            Location location = switchStatement.TryGetLocationForKeyword() ?? switchStatement.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasDefaultOrPatternCase(ISwitchOperation switchStatement)
    {
        return switchStatement.Cases.SelectMany(@case => @case.Clauses).Any(IsDefaultOrPatternCase);
    }

    private static bool IsDefaultOrPatternCase(ICaseClauseOperation clause)
    {
        return clause.CaseKind is CaseKind.Default or CaseKind.Pattern;
    }

    private static bool? IsSwitchExhaustive(SwitchAnalysisContext analysisContext)
    {
        IdentifierInfo? identifierInfo = analysisContext.SwitchStatement.Value.TryGetIdentifierInfo();

        return identifierInfo != null ? IsSwitchExhaustive(analysisContext, identifierInfo) : null;
    }

    private static bool? IsSwitchExhaustive(SwitchAnalysisContext analysisContext, IdentifierInfo identifierInfo)
    {
        return IsSwitchExhaustiveForBooleanTypes(identifierInfo, analysisContext) ?? IsSwitchExhaustiveForEnumerationTypes(identifierInfo, analysisContext);
    }

    private static bool? IsSwitchExhaustiveForBooleanTypes(IdentifierInfo identifierInfo, SwitchAnalysisContext analysisContext)
    {
        bool isBoolean = identifierInfo.Type.SpecialType == SpecialType.System_Boolean;
        bool isNullableBoolean = identifierInfo.Type.IsNullableBoolean();

        if (isBoolean || isNullableBoolean)
        {
            ImmutableArray<ISymbol?> possibleValues = isBoolean
                ? ImmutableArray.Create<ISymbol?>(analysisContext.BooleanTrue, analysisContext.BooleanFalse)
                : ImmutableArray.Create(analysisContext.BooleanTrue, analysisContext.BooleanFalse, null);

            return HasCaseClausesFor(possibleValues, analysisContext);
        }

        return null;
    }

    private static bool? IsSwitchExhaustiveForEnumerationTypes(IdentifierInfo identifierInfo, SwitchAnalysisContext analysisContext)
    {
        bool isEnumeration = identifierInfo.Type.BaseType is { SpecialType: SpecialType.System_Enum };

        bool isNullableEnumeration = identifierInfo.Type.IsNullableEnumeration();

        if (isEnumeration || isNullableEnumeration)
        {
            ITypeSymbol enumType = isEnumeration ? (INamedTypeSymbol)identifierInfo.Type : ((INamedTypeSymbol)identifierInfo.Type).TypeArguments[0];

            ISymbol?[] possibleValues = isEnumeration
                ? enumType.GetMembers().OfType<IFieldSymbol>().Cast<ISymbol>().ToArray()
                : enumType.GetMembers().OfType<IFieldSymbol>().Concat(NullSymbolArray).ToArray();

            return HasCaseClausesFor(possibleValues, analysisContext);
        }

        return null;
    }

    private static bool? HasCaseClausesFor(ICollection<ISymbol?> expectedValues, SwitchAnalysisContext analysisContext)
    {
        var collector = new CaseClauseCollector();
        ICollection<ISymbol?>? caseClauseValues = collector.TryGetSymbolsForCaseClauses(analysisContext);

        return caseClauseValues == null ? null : HasCaseClauseForExpectedValues(expectedValues, caseClauseValues);
    }

    private static bool? HasCaseClauseForExpectedValues(ICollection<ISymbol?> expectedValues, ICollection<ISymbol?> caseClauseValues)
    {
        foreach (ISymbol? expectedValue in expectedValues)
        {
            if (!caseClauseValues.Contains(expectedValue))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class CaseClauseCollector
    {
        private readonly HashSet<ISymbol?> caseClauseValues = [];

        public ICollection<ISymbol?>? TryGetSymbolsForCaseClauses(SwitchAnalysisContext analysisContext)
        {
            IEnumerable<ISingleValueCaseClauseOperation> caseClauses =
                analysisContext.SwitchStatement.Cases.SelectMany(@case => @case.Clauses.OfType<ISingleValueCaseClauseOperation>());

            foreach (ISingleValueCaseClauseOperation caseClause in caseClauses)
            {
                analysisContext.CancellationToken.ThrowIfCancellationRequested();

                if (ProcessAsLiteralSyntax(analysisContext, caseClause) || ProcessAsField(caseClause) || ProcessAsConversion(analysisContext, caseClause))
                {
                    continue;
                }

                // Switch statements with non-constant case expressions are not supported
                // because they make exhaustiveness analysis non-trivial.

#pragma warning disable AV1135 // Do not return null for strings, collections or tasks
                return null;
#pragma warning restore AV1135 // Do not return null for strings, collections or tasks
            }

            return caseClauseValues;
        }

        private bool ProcessAsConversion(SwitchAnalysisContext analysisContext, ISingleValueCaseClauseOperation caseClause)
        {
            var conversion = caseClause.Value as IConversionOperation;
            var memberSyntax = conversion?.Syntax as MemberAccessExpressionSyntax;

            IFieldSymbol? field = analysisContext.GetFieldOrNull(memberSyntax);

            if (field != null)
            {
                caseClauseValues.Add(field);
                return true;
            }

            return false;
        }

        private bool ProcessAsLiteralSyntax(SwitchAnalysisContext analysisContext, ISingleValueCaseClauseOperation caseClause)
        {
            if (caseClause.Value.Syntax is LiteralExpressionSyntax literalSyntax)
            {
                if (ProcessLiteralSyntaxAsTrueKeyword(analysisContext, literalSyntax) || ProcessLiteralSyntaxAsFalseKeyword(analysisContext, literalSyntax) ||
                    ProcessLiteralSyntaxAsNullKeyword(literalSyntax))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ProcessLiteralSyntaxAsTrueKeyword(SwitchAnalysisContext analysisContext, LiteralExpressionSyntax literalSyntax)
        {
            if (literalSyntax.Token.IsKind(SyntaxKind.TrueKeyword))
            {
                caseClauseValues.Add(analysisContext.BooleanTrue);
                return true;
            }

            return false;
        }

        private bool ProcessLiteralSyntaxAsFalseKeyword(SwitchAnalysisContext analysisContext, LiteralExpressionSyntax literalSyntax)
        {
            if (literalSyntax.Token.IsKind(SyntaxKind.FalseKeyword))
            {
                caseClauseValues.Add(analysisContext.BooleanFalse);
                return true;
            }

            return false;
        }

        private bool ProcessLiteralSyntaxAsNullKeyword(LiteralExpressionSyntax literalSyntax)
        {
            if (literalSyntax.Token.IsKind(SyntaxKind.NullKeyword))
            {
                caseClauseValues.Add(null);
                return true;
            }

            return false;
        }

        private bool ProcessAsField(ISingleValueCaseClauseOperation caseClause)
        {
            if (caseClause.Value is IFieldReferenceOperation enumField)
            {
                caseClauseValues.Add(enumField.Field);
                return true;
            }

            return false;
        }
    }

    private sealed class SwitchAnalysisContext
    {
        private readonly Compilation compilation;

        public CancellationToken CancellationToken { get; }

        public ISwitchOperation SwitchStatement { get; }

        public ISymbol BooleanTrue { get; }

        public ISymbol BooleanFalse { get; }

        public SwitchAnalysisContext(ISwitchOperation switchStatement, INamedTypeSymbol systemBoolean, OperationAnalysisContext context)
        {
            ArgumentNullException.ThrowIfNull(switchStatement);
            ArgumentNullException.ThrowIfNull(systemBoolean);

            SwitchStatement = switchStatement;
            compilation = context.Compilation;
            CancellationToken = context.CancellationToken;

            BooleanTrue = systemBoolean.GetMembers("TrueString").Single();
            BooleanFalse = systemBoolean.GetMembers("FalseString").Single();
        }

        public IFieldSymbol? GetFieldOrNull(MemberAccessExpressionSyntax? memberSyntax)
        {
            if (memberSyntax != null)
            {
                SemanticModel model = compilation.GetSemanticModel(memberSyntax.SyntaxTree);
                return model.GetSymbolInfo(memberSyntax, CancellationToken).Symbol as IFieldSymbol;
            }

            return null;
        }
    }
}
