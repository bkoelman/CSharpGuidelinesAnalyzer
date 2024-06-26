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

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseAbbreviationInIdentifierNameAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Identifier contains an abbreviation or is too short";
    private const string MessageFormat = "{0} '{1}' should have a more descriptive name";
    private const string Description = "Don't use abbreviations.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1706";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
        ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

    [ItemNotNull]
    private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("Btn", "Ctrl", "Frm", "Chk", "Cmb", "Ctx", "Dg", "Pnl", "Dlg", "Ex",
        "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw", "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt",
        "Len", "Idx", "Str", "Doc");

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
        context.SafeRegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.SafeRegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
        context.SafeRegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private void RegisterForOperations([NotNull] AnalysisContext context)
    {
        context.SafeRegisterOperationAction(AnalyzeLocalFunction, OperationKind.LocalFunction);
        context.SafeRegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.SafeRegisterOperationAction(AnalyzeTuple, OperationKind.Tuple);
        context.SafeRegisterOperationAction(AnalyzeAnonymousObjectCreation, OperationKind.AnonymousObjectCreation);
    }

    private void RegisterForSyntax([NotNull] AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeFromClause, SyntaxKind.FromClause);
        context.RegisterSyntaxNodeAction(AnalyzeJoinClause, SyntaxKind.JoinClause);
        context.RegisterSyntaxNodeAction(AnalyzeJoinIntoClause, SyntaxKind.JoinIntoClause);
        context.RegisterSyntaxNodeAction(AnalyzeQueryContinuation, SyntaxKind.QueryContinuation);
        context.RegisterSyntaxNodeAction(AnalyzeLetClause, SyntaxKind.LetClause);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.IsSynthesized())
        {
            return;
        }

        if (IsBlacklistedOrSingleLetter(type.Name))
        {
            var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.TypeKind.Format(), type.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;

        if (member.IsPropertyOrEventAccessor() || member.IsOverride || member.IsSynthesized())
        {
            return;
        }

        if (IsBlacklistedOrSingleLetter(member.Name) && !member.IsInterfaceImplementation())
        {
            var diagnostic = Diagnostic.Create(Rule, member.Locations[0], member.GetKind(), member.Name);
            context.ReportDiagnostic(diagnostic);
        }

        ITypeSymbol memberType = member.GetSymbolType();
        AnalyzeTypeAsTuple(memberType, context.ReportDiagnostic);
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context)
    {
        var localFunction = (ILocalFunctionOperation)context.Operation;

        if (IsBlacklistedOrSingleLetter(localFunction.Symbol.Name))
        {
            var diagnostic = Diagnostic.Create(Rule, localFunction.Symbol.Locations[0], localFunction.Symbol.GetKind(), localFunction.Symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        AnalyzeTypeAsTuple(localFunction.Symbol.ReturnType, context.ReportDiagnostic);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameter = (IParameterSymbol)context.Symbol;

        if (parameter.ContainingSymbol.IsOverride || parameter.IsSynthesized())
        {
            return;
        }

        if (IsBlacklistedOrSingleLetter(parameter.Name) && !parameter.IsInterfaceImplementation())
        {
            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }

        AnalyzeTypeAsTuple(parameter.Type, context.ReportDiagnostic);
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext context)
    {
        var declarator = (IVariableDeclaratorOperation)context.Operation;
        ILocalSymbol variable = declarator.Symbol;

        if (!string.IsNullOrWhiteSpace(variable.Name) && !variable.IsSynthesized())
        {
            if (IsBlacklistedOrSingleLetter(variable.Name))
            {
                var diagnostic = Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        AnalyzeTypeAsTuple(variable.Type, context.ReportDiagnostic);
    }

    private static void AnalyzeTypeAsTuple([NotNull] ITypeSymbol type, [NotNull] Action<Diagnostic> reportDiagnostic)
    {
        if (type.IsTupleType && type is INamedTypeSymbol tupleType)
        {
            foreach (IFieldSymbol tupleElement in tupleType.TupleElements)
            {
                bool isDefaultTupleElement = tupleElement.IsEqualTo(tupleElement.CorrespondingTupleField);

                if (!isDefaultTupleElement && IsBlacklistedOrSingleLetter(tupleElement.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element", tupleElement.Name);
                    reportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeTuple(OperationAnalysisContext context)
    {
        var tuple = (ITupleOperation)context.Operation;

        foreach (IOperation element in tuple.Elements)
        {
            ILocalSymbol tupleElement = TryGetTupleElement(element);

            if (tupleElement != null && IsBlacklistedOrSingleLetter(tupleElement.Name))
            {
                var diagnostic = Diagnostic.Create(Rule, tupleElement.Locations[0], "Tuple element", tupleElement.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    [CanBeNull]
    private static ILocalSymbol TryGetTupleElement([NotNull] IOperation elementOperation)
    {
        ILocalReferenceOperation localReference = elementOperation is IDeclarationExpressionOperation declarationExpression
            ? declarationExpression.Expression as ILocalReferenceOperation
            : elementOperation as ILocalReferenceOperation;

        return localReference is { IsDeclaration: true } ? localReference.Local : null;
    }

    private static void AnalyzeAnonymousObjectCreation(OperationAnalysisContext context)
    {
        var creationExpression = (IAnonymousObjectCreationOperation)context.Operation;

        if (!creationExpression.IsImplicit)
        {
            foreach (IPropertySymbol property in creationExpression.Type.GetMembers().OfType<IPropertySymbol>())
            {
                if (IsBlacklistedOrSingleLetter(property.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, property.Locations[0], "Property", property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeFromClause(SyntaxNodeAnalysisContext context)
    {
        var fromClause = (FromClauseSyntax)context.Node;
        AnalyzeRangeVariable(fromClause.Identifier, context);
    }

    private static void AnalyzeJoinClause(SyntaxNodeAnalysisContext context)
    {
        var joinClause = (JoinClauseSyntax)context.Node;
        AnalyzeRangeVariable(joinClause.Identifier, context);
    }

    private static void AnalyzeJoinIntoClause(SyntaxNodeAnalysisContext context)
    {
        var joinIntoClause = (JoinIntoClauseSyntax)context.Node;
        AnalyzeRangeVariable(joinIntoClause.Identifier, context);
    }

    private static void AnalyzeQueryContinuation(SyntaxNodeAnalysisContext context)
    {
        var queryContinuation = (QueryContinuationSyntax)context.Node;
        AnalyzeRangeVariable(queryContinuation.Identifier, context);
    }

    private static void AnalyzeLetClause(SyntaxNodeAnalysisContext context)
    {
        var letClause = (LetClauseSyntax)context.Node;
        AnalyzeRangeVariable(letClause.Identifier, context);
    }

    private static void AnalyzeRangeVariable(SyntaxToken identifierToken, SyntaxNodeAnalysisContext context)
    {
        string rangeVariableName = identifierToken.ValueText;

        if (!string.IsNullOrEmpty(rangeVariableName) && IsBlacklistedOrSingleLetter(rangeVariableName))
        {
            Location location = identifierToken.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, "Range variable", rangeVariableName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsBlacklistedOrSingleLetter([NotNull] string name)
    {
        return IsBlacklisted(name) || IsSingleLetter(name);
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
