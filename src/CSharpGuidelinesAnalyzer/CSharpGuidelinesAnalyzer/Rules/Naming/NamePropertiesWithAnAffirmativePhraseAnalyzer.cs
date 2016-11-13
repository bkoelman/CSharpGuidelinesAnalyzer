using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamePropertiesWithAnAffirmativePhraseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1715";

        private const string Title = "The name of boolean identifiers should start with a verb";
        private const string MessageFormat = "The name of boolean {0} '{1}' should start with a verb.";
        private const string Description = "Properly name properties.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds = ImmutableArray.Create(
            SymbolKind.Property, SymbolKind.Method, SymbolKind.Field);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsWhitelist = ImmutableArray.Create("Are", "Is", "Was", "Were",
            "Has", "Have", "Can", "Could", "Shall", "Should", "May", "Might", "Will", "Need", "Needs", "Allow", "Allows",
            "Support", "Supports", "Do", "Does", "Did", "Hide", "Hides", "Contain", "Contains", "Require", "Requires",
            "Return", "Returns", "Starts", "Consists", "Targets");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
            context.RegisterSyntaxNodeAction(c => AnalyzeParameter(c.ToSymbolContext()), SyntaxKind.Parameter);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(AnalyzeVariableDeclaration, OperationKind.VariableDeclaration);
                }
            });
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor())
            {
                return;
            }

            if (context.Symbol.IsOverride)
            {
                return;
            }

            ITypeSymbol type = GetMemberType(context.Symbol);
            if (!IsBooleanOrNullableBoolean(type))
            {
                return;
            }

            if (!IsWhitelisted(context.Symbol.Name))
            {
                if (context.Symbol.IsInterfaceImplementation())
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0],
                    LowerCaseKind(context.Symbol.Kind), context.Symbol.Name));
            }
        }

        [NotNull]
        private ITypeSymbol GetMemberType([NotNull] ISymbol symbol)
        {
            var property = symbol as IPropertySymbol;
            if (property != null)
            {
                return property.Type;
            }

            var method = symbol as IMethodSymbol;
            if (method != null)
            {
                return method.ReturnType;
            }

            var field = symbol as IFieldSymbol;
            if (field != null)
            {
                return field.Type;
            }

            throw new InvalidOperationException($"Unexpected type '{symbol.GetType()}'.");
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (parameter.Name.Length == 0)
            {
                return;
            }

            if (parameter.ContainingSymbol.IsOverride)
            {
                return;
            }

            if (!IsBooleanOrNullableBoolean(parameter.Type))
            {
                return;
            }

            if (!IsWhitelisted(parameter.Name))
            {
                if (parameter.IsInterfaceImplementation())
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], LowerCaseKind(parameter.Kind),
                    parameter.Name));
            }
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration) context.Operation;

            if (declaration.Variable.Name.Length == 0)
            {
                return;
            }

            if (!IsBooleanOrNullableBoolean(declaration.Variable.Type))
            {
                return;
            }

            if (!IsWhitelisted(declaration.Variable.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Variable.Locations[0], "variable",
                    declaration.Variable.Name));
            }
        }

        private bool IsBooleanOrNullableBoolean([NotNull] ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Boolean || type.IsNullableBoolean();
        }

        private bool IsWhitelisted([NotNull] string identifierName)
        {
            return identifierName.StartsWithAnyWordOf(WordsWhitelist, TextMatchMode.AllowLowerCaseMatch);
        }

        [NotNull]
        private string LowerCaseKind(SymbolKind kind)
        {
            return kind.ToString().ToLowerInvariant();
        }
    }
}