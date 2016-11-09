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

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            new[] { SymbolKind.Property, SymbolKind.Method, SymbolKind.Field }.ToImmutableArray();

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsWhitelist =
            new[]
            {
                "Are",
                "Is",
                "Was",
                "Were",
                "Has",
                "Have",
                "Can",
                "Could",
                "Shall",
                "Should",
                "May",
                "Might",
                "Will",
                "Need",
                "Needs",
                "Allow",
                "Allows",
                "Support",
                "Supports",
                "Do",
                "Does",
                "Did",
                "Hide",
                "Hides",
                "Contain",
                "Contains",
                "Require",
                "Requires",
                "Return",
                "Returns",
                "Starts",
                "Consists",
                "Targets"
            }.ToImmutableArray();

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

            ITypeSymbol type = GetMemberType(context.Symbol);
            if (!IsBooleanOrNullableBoolean(type))
            {
                return;
            }

            if (NameRequiresReport(context.Symbol.Name) && !context.Symbol.IsOverride &&
                !context.Symbol.IsInterfaceImplementation())
            {
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

            if (!IsBooleanOrNullableBoolean(parameter.Type))
            {
                return;
            }

            if (NameRequiresReport(parameter.Name) && !parameter.ContainingSymbol.IsOverride &&
                !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], LowerCaseKind(parameter.Kind),
                    parameter.Name));
            }
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration) context.Operation;

            if (!IsBooleanOrNullableBoolean(declaration.Variable.Type))
            {
                return;
            }

            if (NameRequiresReport(declaration.Variable.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Variable.Locations[0], "variable",
                    declaration.Variable.Name));
            }
        }

        private bool IsBooleanOrNullableBoolean([NotNull] ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Boolean || type.IsNullableBoolean();
        }

        private bool NameRequiresReport([NotNull] string identifierName)
        {
            return !identifierName.StartsWithAnyWordOf(WordsWhitelist, TextMatchMode.AllowLowerCaseMatch);
        }

        [NotNull]
        private string LowerCaseKind(SymbolKind kind)
        {
            return kind.ToString().ToLowerInvariant();
        }
    }
}