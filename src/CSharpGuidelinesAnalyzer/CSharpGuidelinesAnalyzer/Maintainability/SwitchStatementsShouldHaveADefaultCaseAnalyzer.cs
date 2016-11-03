using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SwitchStatementsShouldHaveADefaultCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1536";

        private const string Title = "Incomplete switch statement without a default case clause";
        private const string MessageFormat = "Incomplete switch statement without a default case clause.";
        private const string Description = "Always add a default block after the last case in a switch statement.";
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

            context.RegisterCompilationStartAction(startContext =>
            {
                if (!AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    return;
                }

                INamedTypeSymbol systemBoolean = startContext.Compilation.GetTypeByMetadataName("System.Boolean");
                if (systemBoolean != null)
                {
                    startContext.RegisterOperationAction(c => AnalyzeSwitchStatement(c, systemBoolean),
                        OperationKind.SwitchStatement);
                }
            });
        }

        private void AnalyzeSwitchStatement(OperationAnalysisContext context, [NotNull] INamedTypeSymbol systemBoolean)
        {
            var switchStatement = (ISwitchStatement) context.Operation;

            if (switchStatement.IsInvalid)
            {
                return;
            }

            if (HasDefaultCase(switchStatement))
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var analysisContext = new SwitchAnalysisContext(switchStatement, context.Compilation, systemBoolean,
                context.CancellationToken);

            if (IsSwitchComplete(analysisContext) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, switchStatement.Syntax.GetLocation()));
            }
        }

        private static bool HasDefaultCase([NotNull] ISwitchStatement switchStatement)
        {
            IEnumerable<ICaseClause> caseClauses = switchStatement.Cases.SelectMany(@case => @case.Clauses);
            return caseClauses.Any(clause => clause.CaseKind == CaseKind.Default);
        }

        [CanBeNull]
        private bool? IsSwitchComplete([NotNull] SwitchAnalysisContext analysisContext)
        {
            IdentifierInfo identifierInfo = AnalysisUtilities.TryGetIdentifierInfo(analysisContext.SwitchStatement.Value);
            if (identifierInfo != null)
            {
                if (identifierInfo.Type.SpecialType == SpecialType.System_Boolean)
                {
                    return IsBooleanSwitchComplete(analysisContext);
                }

                if (AnalysisUtilities.IsNullableBoolean(identifierInfo.Type))
                {
                    return IsNullableBooleanSwitchComplete(analysisContext);
                }

                if (identifierInfo.Type.BaseType != null &&
                    identifierInfo.Type.BaseType.SpecialType == SpecialType.System_Enum)
                {
                    var enumType = (INamedTypeSymbol) identifierInfo.Type;
                    IEnumerable<IFieldSymbol> enumMembers = enumType.GetMembers().OfType<IFieldSymbol>();
                    return IsEnumSwitchComplete(analysisContext, enumMembers);
                }

                if (AnalysisUtilities.IsNullableEnum(identifierInfo.Type))
                {
                    ITypeSymbol enumType = ((INamedTypeSymbol) identifierInfo.Type).TypeArguments[0];
                    IEnumerable<IFieldSymbol> enumMembers = enumType.GetMembers().OfType<IFieldSymbol>();
                    return IsNullableEnumSwitchComplete(analysisContext, enumMembers);
                }
            }

            return null;
        }

        [CanBeNull]
        private bool? IsBooleanSwitchComplete([NotNull] SwitchAnalysisContext analysisContext)
        {
            return HasCaseClausesFor(new[] { analysisContext.BooleanTrue, analysisContext.BooleanFalse },
                analysisContext);
        }

        [CanBeNull]
        private bool? IsNullableBooleanSwitchComplete([NotNull] SwitchAnalysisContext analysisContext)
        {
            return HasCaseClausesFor(new[] { analysisContext.BooleanTrue, analysisContext.BooleanFalse, null },
                analysisContext);
        }

        [CanBeNull]
        private bool? IsEnumSwitchComplete([NotNull] SwitchAnalysisContext analysisContext,
            [NotNull] [ItemNotNull] IEnumerable<IFieldSymbol> enumMembers)
        {
            return HasCaseClausesFor(enumMembers.Cast<ISymbol>().ToArray(), analysisContext);
        }

        [CanBeNull]
        private bool? IsNullableEnumSwitchComplete([NotNull] SwitchAnalysisContext analysisContext,
            [NotNull] [ItemNotNull] IEnumerable<IFieldSymbol> enumMembers)
        {
            ISymbol[] expectedValues = enumMembers.Concat(new ISymbol[] { null }).ToArray();
            return HasCaseClausesFor(expectedValues, analysisContext);
        }

        [CanBeNull]
        private bool? HasCaseClausesFor([NotNull] [ItemCanBeNull] ICollection<ISymbol> expectedValues,
            [NotNull] SwitchAnalysisContext analysisContext)
        {
            ICollection<ISymbol> caseClauseValues = TryGetSymbolsForCaseClauses(analysisContext);
            if (caseClauseValues == null)
            {
                return null;
            }

            foreach (ISymbol expectedValue in expectedValues)
            {
                if (!caseClauseValues.Contains(expectedValue))
                {
                    return false;
                }
            }

            return true;
        }

        [CanBeNull]
        [ItemCanBeNull]
        private ICollection<ISymbol> TryGetSymbolsForCaseClauses([NotNull] SwitchAnalysisContext analysisContext)
        {
            var caseClauseValues = new HashSet<ISymbol>();

            IEnumerable<ISingleValueCaseClause> caseClauses =
                analysisContext.SwitchStatement.Cases.SelectMany(@case => @case.Clauses.OfType<ISingleValueCaseClause>());
            foreach (ISingleValueCaseClause caseClause in caseClauses)
            {
                analysisContext.CancellationToken.ThrowIfCancellationRequested();

                var literalSyntax = caseClause.Value.Syntax as LiteralExpressionSyntax;
                if (literalSyntax != null)
                {
                    if (literalSyntax.Token.IsKind(SyntaxKind.TrueKeyword))
                    {
                        caseClauseValues.Add(analysisContext.BooleanTrue);
                        continue;
                    }

                    if (literalSyntax.Token.IsKind(SyntaxKind.FalseKeyword))
                    {
                        caseClauseValues.Add(analysisContext.BooleanFalse);
                        continue;
                    }

                    if (literalSyntax.Token.IsKind(SyntaxKind.NullKeyword))
                    {
                        caseClauseValues.Add(null);
                        continue;
                    }
                }

                var enumField = caseClause.Value as IFieldReferenceExpression;
                if (enumField != null)
                {
                    caseClauseValues.Add(enumField.Field);
                    continue;
                }

                var conversion = caseClause.Value as IConversionExpression;
                if (conversion != null)
                {
                    var memberSyntax = conversion.Syntax as MemberAccessExpressionSyntax;
                    if (memberSyntax != null)
                    {
                        IFieldSymbol field = analysisContext.GetFieldOrNull(memberSyntax);
                        if (field != null)
                        {
                            caseClauseValues.Add(field);
                            continue;
                        }
                    }
                }

                // Switch statements with non-constant case expressions are not supported
                // because they make completion analysis non-trivial.
                return null;
            }

            return caseClauseValues;
        }

        private sealed class SwitchAnalysisContext
        {
            [NotNull]
            private readonly Compilation compilation;

            public CancellationToken CancellationToken { get; }

            [NotNull]
            public ISwitchStatement SwitchStatement { get; }

            [NotNull]
            public ISymbol BooleanTrue { get; }

            [NotNull]
            public ISymbol BooleanFalse { get; }

            public SwitchAnalysisContext([NotNull] ISwitchStatement switchStatement, [NotNull] Compilation compilation,
                [NotNull] INamedTypeSymbol systemBoolean, CancellationToken cancellationToken)
            {
                Guard.NotNull(switchStatement, nameof(switchStatement));
                Guard.NotNull(compilation, nameof(compilation));
                Guard.NotNull(systemBoolean, nameof(systemBoolean));

                SwitchStatement = switchStatement;
                this.compilation = compilation;
                CancellationToken = cancellationToken;

                BooleanTrue = systemBoolean.GetMembers("TrueString").Single();
                BooleanFalse = systemBoolean.GetMembers("FalseString").Single();
            }

            [CanBeNull]
            public IFieldSymbol GetFieldOrNull([NotNull] MemberAccessExpressionSyntax memberSyntax)
            {
                Guard.NotNull(memberSyntax, nameof(memberSyntax));

                SemanticModel model = compilation.GetSemanticModel(memberSyntax.SyntaxTree);
                return model.GetSymbolInfo(memberSyntax, CancellationToken).Symbol as IFieldSymbol;
            }
        }
    }
}