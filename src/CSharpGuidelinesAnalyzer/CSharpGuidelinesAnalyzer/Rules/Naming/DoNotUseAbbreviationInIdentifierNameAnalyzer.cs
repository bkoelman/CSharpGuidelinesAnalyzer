using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseAbbreviationInIdentifierNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1706";

        private const string Title = "Identifier contains an abbreviation or is too short";
        private const string MessageFormat = "{0} '{1}' should have a more descriptive name.";
        private const string Description = "Don't use abbreviations.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("Btn", "Ctrl", "Frm", "Chk", "Cmb",
            "Ctx", "Dg", "Pnl", "Dlg", "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw",
            "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt", "Len", "Idx", "Str");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclarator), OperationKind.VariableDeclarator);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeTuple), OperationKind.Tuple);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeAnonymousObjectCreation),
                OperationKind.AnonymousObjectCreation);

            context.RegisterSyntaxNodeAction(AnalyzeFromClause, SyntaxKind.FromClause);
            context.RegisterSyntaxNodeAction(AnalyzeJoinClause, SyntaxKind.JoinClause);
            context.RegisterSyntaxNodeAction(AnalyzeJoinIntoClause, SyntaxKind.JoinIntoClause);
            context.RegisterSyntaxNodeAction(AnalyzeQueryContinuation, SyntaxKind.QueryContinuation);
            context.RegisterSyntaxNodeAction(AnalyzeLetClause, SyntaxKind.LetClause);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.IsSynthesized())
            {
                return;
            }

            if (IsBlacklisted(type.Name) || IsSingleLetter(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.TypeKind, type.Name));
            }
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            ISymbol member = context.Symbol;

            if (member.IsPropertyOrEventAccessor() || member.IsOverride || member.IsSynthesized())
            {
                return;
            }

            if (IsBlacklisted(member.Name) || IsSingleLetter(member.Name))
            {
                if (!member.IsInterfaceImplementation())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0], member.GetKind(), member.Name));
                }
            }

            ITypeSymbol memberType = member.GetMemberType();
            AnalyzeTypeAsTuple(memberType, context.ReportDiagnostic);
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var localFunction = (ILocalFunctionOperation)context.Operation;

            if (IsBlacklisted(localFunction.Symbol.Name) || IsSingleLetter(localFunction.Symbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, localFunction.Symbol.Locations[0],
                    localFunction.Symbol.GetKind(), localFunction.Symbol.Name));
            }

            AnalyzeTypeAsTuple(localFunction.Symbol.ReturnType, context.ReportDiagnostic);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.ContainingSymbol.IsOverride || parameter.IsSynthesized())
            {
                return;
            }

            bool requiresReport = IsInLambdaExpression(parameter)
                ? IsBlacklisted(parameter.Name)
                : IsBlacklisted(parameter.Name) || IsSingleLetter(parameter.Name);

            if (requiresReport && !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }

            AnalyzeTypeAsTuple(parameter.Type, context.ReportDiagnostic);
        }

        private static bool IsInLambdaExpression([NotNull] IParameterSymbol parameter)
        {
            return parameter.ContainingSymbol is IMethodSymbol method && method.MethodKind == MethodKind.LambdaMethod;
        }

        private void AnalyzeVariableDeclarator(OperationAnalysisContext context)
        {
            var declarator = (IVariableDeclaratorOperation)context.Operation;
            ILocalSymbol variable = declarator.Symbol;

            if (!string.IsNullOrWhiteSpace(variable.Name) && !variable.IsSynthesized())
            {
                if (IsBlacklisted(variable.Name) || IsSingleLetter(variable.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name));
                }
            }

            AnalyzeTypeAsTuple(variable.Type, context.ReportDiagnostic);
        }

        private void AnalyzeTypeAsTuple([NotNull] ITypeSymbol type, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (type.IsTupleType && type is INamedTypeSymbol tupleType)
            {
                foreach (IFieldSymbol tupleElement in tupleType.TupleElements)
                {
                    bool isDefaultTupleElement = tupleElement.Equals(tupleElement.CorrespondingTupleField);
                    if (!isDefaultTupleElement)
                    {
                        if (IsBlacklisted(tupleElement.Name) || IsSingleLetter(tupleElement.Name))
                        {
                            reportDiagnostic(Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element",
                                tupleElement.Name));
                        }
                    }
                }
            }
        }

        private void AnalyzeTuple(OperationAnalysisContext context)
        {
            var tuple = (ITupleOperation)context.Operation;

            foreach (IOperation element in tuple.Elements)
            {
                ILocalSymbol tupleElement = TryGetTupleElement(element);

                if (tupleElement != null)
                {
                    if (IsBlacklisted(tupleElement.Name) || IsSingleLetter(tupleElement.Name))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element",
                            tupleElement.Name));
                    }
                }
            }
        }

        [CanBeNull]
        private ILocalSymbol TryGetTupleElement([NotNull] IOperation elementOperation)
        {
            ILocalReferenceOperation localReference = elementOperation is IDeclarationExpressionOperation declarationExpression
                ? declarationExpression.Expression as ILocalReferenceOperation
                : elementOperation as ILocalReferenceOperation;

            return localReference != null && localReference.IsDeclaration ? localReference.Local : null;
        }

        private void AnalyzeAnonymousObjectCreation(OperationAnalysisContext context)
        {
            var creationExpression = (IAnonymousObjectCreationOperation)context.Operation;

            foreach (IPropertySymbol property in creationExpression.Type.GetMembers().OfType<IPropertySymbol>())
            {
                if (IsBlacklisted(property.Name) || IsSingleLetter(property.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], "Property", property.Name));
                }
            }
        }

        private void AnalyzeFromClause(SyntaxNodeAnalysisContext context)
        {
            var fromClause = (FromClauseSyntax)context.Node;
            AnalyzeRangeVariable(fromClause.Identifier, context);
        }

        private void AnalyzeJoinClause(SyntaxNodeAnalysisContext context)
        {
            var joinClause = (JoinClauseSyntax)context.Node;
            AnalyzeRangeVariable(joinClause.Identifier, context);
        }

        private void AnalyzeJoinIntoClause(SyntaxNodeAnalysisContext context)
        {
            var joinIntoClause = (JoinIntoClauseSyntax)context.Node;
            AnalyzeRangeVariable(joinIntoClause.Identifier, context);
        }

        private void AnalyzeQueryContinuation(SyntaxNodeAnalysisContext context)
        {
            var queryContinuation = (QueryContinuationSyntax)context.Node;
            AnalyzeRangeVariable(queryContinuation.Identifier, context);
        }

        private void AnalyzeLetClause(SyntaxNodeAnalysisContext context)
        {
            var letClause = (LetClauseSyntax)context.Node;
            AnalyzeRangeVariable(letClause.Identifier, context);
        }

        private static void AnalyzeRangeVariable(SyntaxToken identifierToken, SyntaxNodeAnalysisContext context)
        {
            string rangeVariableName = identifierToken.ValueText;

            if (string.IsNullOrEmpty(rangeVariableName))
            {
                return;
            }

            if (IsBlacklisted(rangeVariableName) || IsSingleLetter(rangeVariableName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, identifierToken.GetLocation(), "Range variable",
                    rangeVariableName));
            }
        }

        private static bool IsBlacklisted([NotNull] string name)
        {
            return name.GetWordsInList(WordsBlacklist).Any();
        }

        private static bool IsSingleLetter([NotNull] string name)
        {
            return name.Length == 1 && char.IsLetter(name[0]);
        }
    }
}
