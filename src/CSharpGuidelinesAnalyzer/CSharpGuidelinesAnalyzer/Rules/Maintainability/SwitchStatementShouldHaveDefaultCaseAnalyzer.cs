using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SwitchStatementShouldHaveDefaultCaseAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Non-exhaustive switch statement requires a default case clause";
        private const string MessageFormat = "Non-exhaustive switch statement requires a default case clause";
        private const string Description = "Always add a default block after the last case in a switch statement.";

        public const string DiagnosticId = "AV1536";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        [ItemCanBeNull]
        private static readonly ISymbol[] NullSymbolArray =
        {
            null
        };

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        [NotNull]
        private static readonly Action<OperationAnalysisContext, INamedTypeSymbol> AnalyzeSwitchStatementAction = (context, systemBoolean) =>
            context.SkipInvalid(_ => AnalyzeSwitchStatement(context, systemBoolean));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            INamedTypeSymbol systemBoolean = KnownTypes.SystemBoolean(startContext.Compilation);

            if (systemBoolean != null)
            {
                startContext.RegisterOperationAction(context => AnalyzeSwitchStatementAction(context, systemBoolean), OperationKind.Switch);
            }
        }

        private static void AnalyzeSwitchStatement(OperationAnalysisContext context, [NotNull] INamedTypeSymbol systemBoolean)
        {
            var switchStatement = (ISwitchOperation)context.Operation;

            if (HasDefaultOrPatternCase(switchStatement))
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            AnalyzeSwitchExhaustiveness(switchStatement, systemBoolean, context);
        }

        private static void AnalyzeSwitchExhaustiveness([NotNull] ISwitchOperation switchStatement, [NotNull] INamedTypeSymbol systemBoolean,
            OperationAnalysisContext context)
        {
            var analysisContext = new SwitchAnalysisContext(switchStatement, systemBoolean, context);

            if (IsSwitchExhaustive(analysisContext) == false)
            {
                Location location = switchStatement.TryGetLocationForKeyword() ?? switchStatement.Syntax.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }

        private static bool HasDefaultOrPatternCase([NotNull] ISwitchOperation switchStatement)
        {
            return switchStatement.Cases.SelectMany(@case => @case.Clauses).Any(IsDefaultOrPatternCase);
        }

        private static bool IsDefaultOrPatternCase([NotNull] ICaseClauseOperation clause)
        {
            return clause.CaseKind == CaseKind.Default || clause.CaseKind == CaseKind.Pattern;
        }

        [CanBeNull]
        private static bool? IsSwitchExhaustive([NotNull] SwitchAnalysisContext analysisContext)
        {
            IdentifierInfo identifierInfo = analysisContext.SwitchStatement.Value.TryGetIdentifierInfo();

            return identifierInfo != null ? IsSwitchExhaustive(analysisContext, identifierInfo) : null;
        }

        [CanBeNull]
        private static bool? IsSwitchExhaustive([NotNull] SwitchAnalysisContext analysisContext, [NotNull] IdentifierInfo identifierInfo)
        {
            return IsSwitchExhaustiveForBooleanTypes(identifierInfo, analysisContext) ?? IsSwitchExhaustiveForEnumerationTypes(identifierInfo, analysisContext);
        }

        [CanBeNull]
        private static bool? IsSwitchExhaustiveForBooleanTypes([NotNull] IdentifierInfo identifierInfo, [NotNull] SwitchAnalysisContext analysisContext)
        {
            bool isBoolean = identifierInfo.Type.SpecialType == SpecialType.System_Boolean;
            bool isNullableBoolean = identifierInfo.Type.IsNullableBoolean();

            if (isBoolean || isNullableBoolean)
            {
                ImmutableArray<ISymbol> possibleValues = isBoolean
                    ? ImmutableArray.Create(analysisContext.BooleanTrue, analysisContext.BooleanFalse)
                    : ImmutableArray.Create(analysisContext.BooleanTrue, analysisContext.BooleanFalse, null);

                return HasCaseClausesFor(possibleValues, analysisContext);
            }

            return null;
        }

        [CanBeNull]
        private static bool? IsSwitchExhaustiveForEnumerationTypes([NotNull] IdentifierInfo identifierInfo, [NotNull] SwitchAnalysisContext analysisContext)
        {
            bool isEnumeration = identifierInfo.Type.BaseType != null && identifierInfo.Type.BaseType.SpecialType == SpecialType.System_Enum;

            bool isNullableEnumeration = identifierInfo.Type.IsNullableEnumeration();

            if (isEnumeration || isNullableEnumeration)
            {
                ITypeSymbol enumType = isEnumeration ? (INamedTypeSymbol)identifierInfo.Type : ((INamedTypeSymbol)identifierInfo.Type).TypeArguments[0];

                ISymbol[] possibleValues = isEnumeration
                    ? enumType.GetMembers().OfType<IFieldSymbol>().Cast<ISymbol>().ToArray()
                    : enumType.GetMembers().OfType<IFieldSymbol>().Concat(NullSymbolArray).ToArray();

                return HasCaseClausesFor(possibleValues, analysisContext);
            }

            return null;
        }

        [CanBeNull]
        private static bool? HasCaseClausesFor([NotNull] [ItemCanBeNull] ICollection<ISymbol> expectedValues, [NotNull] SwitchAnalysisContext analysisContext)
        {
            var collector = new CaseClauseCollector();
            ICollection<ISymbol> caseClauseValues = collector.TryGetSymbolsForCaseClauses(analysisContext);

            return caseClauseValues == null ? null : HasCaseClauseForExpectedValues(expectedValues, caseClauseValues);
        }

        [CanBeNull]
        private static bool? HasCaseClauseForExpectedValues([NotNull] [ItemCanBeNull] ICollection<ISymbol> expectedValues,
            [NotNull] [ItemCanBeNull] ICollection<ISymbol> caseClauseValues)
        {
            foreach (ISymbol expectedValue in expectedValues)
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
            [NotNull]
            [ItemCanBeNull]
            private readonly HashSet<ISymbol> caseClauseValues = new HashSet<ISymbol>();

            [CanBeNull]
            [ItemCanBeNull]
            public ICollection<ISymbol> TryGetSymbolsForCaseClauses([NotNull] SwitchAnalysisContext analysisContext)
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

            private bool ProcessAsConversion([NotNull] SwitchAnalysisContext analysisContext, [NotNull] ISingleValueCaseClauseOperation caseClause)
            {
                var conversion = caseClause.Value as IConversionOperation;
                var memberSyntax = conversion?.Syntax as MemberAccessExpressionSyntax;

                IFieldSymbol field = analysisContext.GetFieldOrNull(memberSyntax);

                if (field != null)
                {
                    caseClauseValues.Add(field);
                    return true;
                }

                return false;
            }

            private bool ProcessAsLiteralSyntax([NotNull] SwitchAnalysisContext analysisContext, [NotNull] ISingleValueCaseClauseOperation caseClause)
            {
                if (caseClause.Value.Syntax is LiteralExpressionSyntax literalSyntax)
                {
                    if (ProcessLiteralSyntaxAsTrueKeyword(analysisContext, literalSyntax) ||
                        ProcessLiteralSyntaxAsFalseKeyword(analysisContext, literalSyntax) || ProcessLiteralSyntaxAsNullKeyword(literalSyntax))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool ProcessLiteralSyntaxAsTrueKeyword([NotNull] SwitchAnalysisContext analysisContext, [NotNull] LiteralExpressionSyntax literalSyntax)
            {
                if (literalSyntax.Token.IsKind(SyntaxKind.TrueKeyword))
                {
                    caseClauseValues.Add(analysisContext.BooleanTrue);
                    return true;
                }

                return false;
            }

            private bool ProcessLiteralSyntaxAsFalseKeyword([NotNull] SwitchAnalysisContext analysisContext, [NotNull] LiteralExpressionSyntax literalSyntax)
            {
                if (literalSyntax.Token.IsKind(SyntaxKind.FalseKeyword))
                {
                    caseClauseValues.Add(analysisContext.BooleanFalse);
                    return true;
                }

                return false;
            }

            private bool ProcessLiteralSyntaxAsNullKeyword([NotNull] LiteralExpressionSyntax literalSyntax)
            {
                if (literalSyntax.Token.IsKind(SyntaxKind.NullKeyword))
                {
                    caseClauseValues.Add(null);
                    return true;
                }

                return false;
            }

            private bool ProcessAsField([NotNull] ISingleValueCaseClauseOperation caseClause)
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
            [NotNull]
            private readonly Compilation compilation;

            public CancellationToken CancellationToken { get; }

            [NotNull]
            public ISwitchOperation SwitchStatement { get; }

            [NotNull]
            public ISymbol BooleanTrue { get; }

            [NotNull]
            public ISymbol BooleanFalse { get; }

            public SwitchAnalysisContext([NotNull] ISwitchOperation switchStatement, [NotNull] INamedTypeSymbol systemBoolean, OperationAnalysisContext context)
            {
                Guard.NotNull(switchStatement, nameof(switchStatement));
                Guard.NotNull(systemBoolean, nameof(systemBoolean));

                SwitchStatement = switchStatement;
                compilation = context.Compilation;
                CancellationToken = context.CancellationToken;

                BooleanTrue = systemBoolean.GetMembers("TrueString").Single();
                BooleanFalse = systemBoolean.GetMembers("FalseString").Single();
            }

            [CanBeNull]
            public IFieldSymbol GetFieldOrNull([CanBeNull] MemberAccessExpressionSyntax memberSyntax)
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
}
