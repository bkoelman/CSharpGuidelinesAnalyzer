using System;
using System.Collections.Generic;
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
    public sealed class DoNotUseNumberInIdentifierNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1704";

        private const string Title = "Identifier contains one or more digits in its name";
        private const string MessageFormat = "{0} '{1}' contains one or more digits in its name.";
        private const string Description = "Don't include numbers in variables, parameters and type members.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [NotNull]
        private static readonly char[] Digits =
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            RegisterForSymbols(context);
            RegisterForOperations(context);
            RegisterForSyntax(context);
        }

        private void RegisterForSymbols([NotNull] AnalysisContext context)
        {
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
        }

        private void RegisterForOperations([NotNull] AnalysisContext context)
        {
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclarator), OperationKind.VariableDeclarator);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeTuple), OperationKind.Tuple);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeAnonymousObjectCreation), OperationKind.AnonymousObjectCreation);
        }

        private void RegisterForSyntax([NotNull] AnalysisContext context)
        {
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

            if (ContainsDigitsNonWhitelisted(type.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.TypeKind, type.Name));
            }
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            ISymbol member = context.Symbol;

            if (member.IsPropertyOrEventAccessor() || context.Symbol.IsUnitTestMethod() || member.IsSynthesized())
            {
                return;
            }

            if (ContainsDigitsNonWhitelisted(member.Name) && !member.IsOverride && !member.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0], member.GetKind(), member.Name));
            }

            ITypeSymbol memberType = member.GetMemberType();
            AnalyzeTypeAsTuple(memberType, context.ReportDiagnostic);
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var localFunction = (ILocalFunctionOperation)context.Operation;

            if (ContainsDigitsNonWhitelisted(localFunction.Symbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, localFunction.Symbol.Locations[0],
                    localFunction.Symbol.GetKind(), localFunction.Symbol.Name));
            }

            AnalyzeTypeAsTuple(localFunction.Symbol.ReturnType, context.ReportDiagnostic);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.IsSynthesized())
            {
                return;
            }

            if (ContainsDigitsNonWhitelisted(parameter.Name) && !parameter.ContainingSymbol.IsOverride &&
                !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }

            AnalyzeTypeAsTuple(parameter.Type, context.ReportDiagnostic);
        }

        private void AnalyzeVariableDeclarator(OperationAnalysisContext context)
        {
            var declarator = (IVariableDeclaratorOperation)context.Operation;
            ILocalSymbol variable = declarator.Symbol;

            if (variable.IsSynthesized())
            {
                return;
            }

            if (ContainsDigitsNonWhitelisted(variable.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name));
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

                    if (!isDefaultTupleElement && ContainsDigitsNonWhitelisted(tupleElement.Name))
                    {
                        reportDiagnostic(Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element", tupleElement.Name));
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

                if (tupleElement != null && ContainsDigitsNonWhitelisted(tupleElement.Name))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element",
                        tupleElement.Name));
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
                if (ContainsDigitsNonWhitelisted(property.Name))
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

            if (ContainsDigitsNonWhitelisted(rangeVariableName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, identifierToken.GetLocation(), "Range variable",
                    rangeVariableName));
            }
        }

        private static bool ContainsDigitsNonWhitelisted([NotNull] string text)
        {
            if (ContainsDigit(text))
            {
                string newText = RemoveWordsOnWhitelist(text);
                return ContainsDigit(newText);
            }

            return false;
        }

        [NotNull]
        private static string RemoveWordsOnWhitelist([NotNull] string text)
        {
            var tokenizer = new WordsTokenizer(text);
            List<WordToken> tokens = tokenizer.GetTokens().ToList();

            RemoveWhitelistedTokens(tokens);

            return string.Join(string.Empty, tokens.Select(t => t.Text));
        }

        private static void RemoveWhitelistedTokens([NotNull] List<WordToken> tokens)
        {
#pragma warning disable AV1530 // Loop variable should not be written to in loop body
            for (int index = 0; index < tokens.Count - 1; index++)
            {
                string thisTokenText = tokens[index].Text;
                string nextTokenText = tokens[index + 1].Text;

                if (IsSystemInt(thisTokenText, nextTokenText) || IsDimensional(thisTokenText, nextTokenText))
                {
                    tokens.RemoveRange(index, 2);
                    index--;
                }
            }
#pragma warning restore AV1530 // Loop variable should not be written to in loop body
        }

        private static bool IsDimensional([NotNull] string thisTokenText, [NotNull] string nextTokenText)
        {
            return (thisTokenText == "2" || thisTokenText == "3" || thisTokenText == "4") && nextTokenText == "D";
        }

        private static bool IsSystemInt([NotNull] string thisTokenText, [NotNull] string nextTokenText)
        {
            return thisTokenText == "Int" && (nextTokenText == "16" || nextTokenText == "32" || nextTokenText == "64");
        }

        private static bool ContainsDigit([NotNull] string text)
        {
            return text.IndexOfAny(Digits) != -1;
        }
    }
}
