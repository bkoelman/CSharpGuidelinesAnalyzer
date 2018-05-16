using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidSignatureWithManyParametersAnalyzer : GuidelineAnalyzer
    {
        // TODO: Check additional signatures, such as constructors, delegates and local functions
        // TODO: Check that parameters are not tuples
        // TODO: Check that tuple return value has at most 2 elements
        // TODO: Add exception for Deconstruct() method

        public const string DiagnosticId = "AV1561";

        private const int MaxParameterLength = 3;
        private const string MaxParameterLengthText = "3";

        private const string Title = "Signature contains more than " + MaxParameterLengthText + " parameters";
        private const string MessageFormat = "{0} contains more than " + MaxParameterLengthText + " parameters.";
        private const string Description = "Don't declare signatures with more than " + MaxParameterLengthText + " parameters.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

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
            var property = (IPropertySymbol)context.Symbol;

            if (property.IsIndexer && ExceedsMaximumLength(property.Parameters))
            {
                ReportDiagnostic(context, property, "Indexer");
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (!method.IsPropertyOrEventAccessor() && ExceedsMaximumLength(method.Parameters))
            {
                string memberName = GetMemberName(method);
                ReportDiagnostic(context, method, memberName);
            }
        }

        [NotNull]
        private static string GetMemberName([NotNull] IMethodSymbol method)
        {
            return IsConstructor(method) ? GetNameForConstructor(method) : GetNameForMethod(method);
        }

        private static bool IsConstructor([NotNull] IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Constructor;
        }

        [NotNull]
        private static string GetNameForConstructor([NotNull] IMethodSymbol method)
        {
            var builder = new StringBuilder();
            builder.Append("Constructor for '");
            builder.Append(method.ContainingType.Name);
            builder.Append("'");
            return builder.ToString();
        }

        [NotNull]
        private static string GetNameForMethod([NotNull] IMethodSymbol method)
        {
            var builder = new StringBuilder();
            builder.Append("Method '");
            builder.Append(method.Name);
            builder.Append("'");
            return builder.ToString();
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (IsDelegate(type) && ExceedsMaximumLength(type.DelegateInvokeMethod?.Parameters))
            {
                ReportDiagnostic(context, type, $"Delegate '{type.Name}'");
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

        private static void ReportDiagnostic(SymbolAnalysisContext context, [NotNull] ISymbol symbol, [NotNull] string name)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
