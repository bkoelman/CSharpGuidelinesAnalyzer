using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMembersWithMoreThanThreeParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1561";
        private const int MaxParameterLength = 3;

        private const string Title = "Method or constructor contains more than three parameters";
        private const string MessageFormat = "{0} contains more than three parameters.";
        private const string Description = "Don't allow methods and constructors with more than three parameters.";
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

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeProperty), SymbolKind.Property);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMethod), SymbolKind.Method);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol) context.Symbol;

            if (property.IsIndexer && ExceedsMaximumLength(property.Parameters))
            {
                ReportDiagnostic(context, property, "Indexer");
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol) context.Symbol;

            if (!method.IsPropertyOrEventAccessor() && ExceedsMaximumLength(method.Parameters))
            {
                string name = IsConstructor(method)
                    ? "Constructor for '" + method.ContainingType.Name + "'"
                    : "Method '" + method.Name + "'";

                ReportDiagnostic(context, method, name);
            }
        }

        private static bool IsConstructor([NotNull] IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Constructor;
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if (IsDelegate(type) && ExceedsMaximumLength(type.DelegateInvokeMethod?.Parameters))
            {
                ReportDiagnostic(context, type, "Delegate '" + type.Name + "'");
            }
        }

        private static bool IsDelegate([NotNull] INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Delegate;
        }

        private static bool ExceedsMaximumLength([CanBeNull] [ItemNotNull] IEnumerable<IParameterSymbol> parameters)
        {
            return parameters != null && parameters.Count() > MaxParameterLength;
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, [NotNull] ISymbol symbol,
            [NotNull] string name)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}