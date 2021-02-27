using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamePropertyWithAnAffirmativePhraseAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Name of public or internal boolean identifier should start with a verb";
        private const string MessageFormat = "The name of {0} boolean {1} '{2}' should start with a verb.";
        private const string Description = "Properly name properties.";

        public const string DiagnosticId = "AV1715";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, false, Description, Category.GetHelpLinkUri(DiagnosticId));

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds = ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsWhitelist = ImmutableArray.Create("Are", "Be", "Is", "Was", "Were", "Has", "Have", "Can", "Could",
            "Shall", "Should", "May", "Might", "Will", "Need", "Needs", "Allow", "Allows", "Support", "Supports", "Do", "Does", "Did", "Hide", "Hides",
            "Contain", "Contains", "Require", "Requires", "Return", "Returns", "Starts", "Consists", "Targets");

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeMemberAction = context => context.SkipEmptyName(AnalyzeMember);

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeParameterAction = context => context.SkipEmptyName(AnalyzeParameter);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMemberAction, MemberSymbolKinds);
            context.RegisterSyntaxNodeAction(AnalyzeParameterAction, SyntaxKind.Parameter);
        }

        private static void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (!IsMemberAccessible(context.Symbol) || context.Symbol.IsPropertyOrEventAccessor() || IsOperator(context.Symbol) || context.Symbol.IsOverride ||
                context.Symbol.IsSynthesized())
            {
                return;
            }

            ITypeSymbol type = context.Symbol.GetSymbolType();

            if (!type.IsBooleanOrNullableBoolean())
            {
                return;
            }

            if (!IsWhitelisted(context.Symbol.Name) && !context.Symbol.IsInterfaceImplementation())
            {
                ReportAt(context, context.Symbol);
            }
        }

        private static bool IsOperator([NotNull] ISymbol symbol)
        {
            var method = symbol as IMethodSymbol;

            MethodKind? kind = method?.MethodKind;
            return kind == MethodKind.UserDefinedOperator || kind == MethodKind.BuiltinOperator;
        }

        private static bool IsMemberAccessible([NotNull] ISymbol symbol)
        {
            return symbol.DeclaredAccessibility != Accessibility.Private && symbol.IsSymbolAccessibleFromRoot();
        }

        private static void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (!IsParameterAccessible(parameter) || parameter.ContainingSymbol.IsOverride || !parameter.Type.IsBooleanOrNullableBoolean() ||
                parameter.IsSynthesized())
            {
                return;
            }

            if (!IsWhitelisted(parameter.Name) && !parameter.IsInterfaceImplementation())
            {
                ReportAt(context, parameter);
            }
        }

        private static bool IsParameterAccessible([NotNull] IParameterSymbol parameter)
        {
            ISymbol containingMember = parameter.ContainingSymbol;
            return IsMemberAccessible(containingMember);
        }

        private static bool IsWhitelisted([NotNull] string identifierName)
        {
            return identifierName.StartsWithWordInList(WordsWhitelist);
        }

        private static void ReportAt(SymbolAnalysisContext context, [NotNull] ISymbol symbol)
        {
            Accessibility accessibility = symbol is IParameterSymbol parameterSymbol
                ? parameterSymbol.ContainingSymbol.DeclaredAccessibility
                : symbol.DeclaredAccessibility;

            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], accessibility.ToText().ToLowerInvariant(),
                symbol.GetKind().ToLowerInvariant(), symbol.Name));
        }
    }
}
