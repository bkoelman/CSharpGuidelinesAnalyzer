using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidStaticClassesAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1008";

        private const string Title = "Class should not be static";

        private const string TypeMessageFormat =
            "Class '{0}' should be non-static or its name should be suffixed with 'Extensions'.";

        private const string MemberMessageFormat = "Class '{0}' contains {1} non-extension method '{2}'.";
        private const string Description = "Avoid static classes.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(DiagnosticId, Title, TypeMessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.HelpLinkUri);

        [NotNull]
        private static readonly DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MemberMessageFormat, Category.Name, DiagnosticSeverity.Warning, true, Description, Category.HelpLinkUri);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TypeRule, MemberRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (!type.IsStatic)
            {
                return;
            }

            if (!type.Name.EndsWith("Extensions", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeRule, type.Locations[0], type.Name));
            }
            else
            {
                AnalyzeAccessibleMethods(type, context);
            }
        }

        private static void AnalyzeAccessibleMethods([NotNull] INamedTypeSymbol type, SymbolAnalysisContext context)
        {
            IEnumerable<IMethodSymbol> accessibleMethods = type.GetMembers().OfType<IMethodSymbol>().Where(IsPublicOrInternal);

            foreach (IMethodSymbol method in accessibleMethods.Where(method => !method.IsExtensionMethod))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                context.ReportDiagnostic(Diagnostic.Create(MemberRule, method.Locations[0], type.Name,
                    method.DeclaredAccessibility == Accessibility.Public ? "public" : "internal", method.Name));
            }
        }

        private static bool IsPublicOrInternal([NotNull] IMethodSymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Public || method.DeclaredAccessibility == Accessibility.Internal;
        }
    }
}
