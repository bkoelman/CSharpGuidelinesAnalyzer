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
    public sealed class AvoidStaticClassAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1008";

        private const string Title = "Class should not be static";

        private const string TypeMessageFormat =
            "Class '{0}' should be non-static or its name should be suffixed with 'Extensions'.";

        private const string MemberMessageFormat =
            "Extension method container class '{0}' contains {1} non-extension-method '{2}'.";

        private const string Description = "Avoid static classes.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(DiagnosticId, Title, TypeMessageFormat,
            Category.Name, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MemberMessageFormat, Category.Name, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

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

            if (!type.IsStatic || type.IsSynthesized())
            {
                return;
            }

            if (!type.Name.EndsWith("Extensions", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeRule, type.Locations[0], type.Name));
            }
            else
            {
                AnalyzeTypeMembers(type, context);
            }
        }

        private static void AnalyzeTypeMembers([NotNull] INamedTypeSymbol type, SymbolAnalysisContext context)
        {
            IEnumerable<ISymbol> accessibleMembers =
                type.GetMembers().Where(IsPublicOrInternal).Where(x => !x.IsPropertyOrEventAccessor());

            foreach (ISymbol member in accessibleMembers)
            {
                AnalyzeAccessibleMember(member, type, context);
            }
        }

        private static void AnalyzeAccessibleMember([NotNull] ISymbol member, [NotNull] INamedTypeSymbol containingType,
            SymbolAnalysisContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (member.IsSynthesized())
            {
                return;
            }

            if (member is IMethodSymbol method && (method.IsExtensionMethod || method.MethodKind == MethodKind.StaticConstructor))
            {
                return;
            }

            string accessibility = member.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
            context.ReportDiagnostic(Diagnostic.Create(MemberRule, member.Locations[0], containingType.Name, accessibility,
                member.Name));
        }

        private static bool IsPublicOrInternal([NotNull] ISymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Public || method.DeclaredAccessibility == Accessibility.Internal;
        }
    }
}
