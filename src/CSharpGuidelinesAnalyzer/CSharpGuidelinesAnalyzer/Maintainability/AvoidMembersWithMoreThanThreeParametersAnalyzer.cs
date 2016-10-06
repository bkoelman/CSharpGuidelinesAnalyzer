using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMembersWithMoreThanThreeParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1561";

        private const string Title = "Method or constructor contains more than three parameters";
        private const string MessageFormat = "{0} contains more than three parameters.";
        private const string Description = "Don't allow methods and constructors with more than three parameters.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol) context.Symbol;

            if (property.IsIndexer && property.Parameters.Length > 3)
            {
                ReportDiagnostic(context, property, "Indexer");
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol) context.Symbol;

            if (!IsPropertyAccessor(method) && method.Parameters.Length > 3)
            {
                string name = IsConstructor(method)
                    ? "Constructor for " + method.ContainingType.Name
                    : "Method " + method.Name;

                ReportDiagnostic(context, method, name);
            }
        }

        private static bool IsPropertyAccessor([NotNull] IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet;
        }

        private static bool IsConstructor([NotNull] IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Constructor;
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if (IsDelegate(type) && type.DelegateInvokeMethod?.Parameters.Length > 3)
            {
                ReportDiagnostic(context, type, "Delegate " + type.Name);
            }
        }

        private static bool IsDelegate([NotNull] INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Delegate;
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, [NotNull] ISymbol symbol,
            [NotNull] string name)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}