using System;
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
    public sealed class NamespaceShouldMatchAssemblyNameAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Namespace should match with assembly name";
        private const string NamespaceMessageFormat = "Namespace '{0}' does not match with assembly name '{1}'.";

        private const string TypeInNamespaceMessageFormat =
            "Type '{0}' is declared in namespace '{1}', which does not match with assembly name '{2}'.";

        private const string GlobalTypeMessageFormat =
            "Type '{0}' is declared in global namespace, which does not match with assembly name '{1}'.";

        private const string Description = "Name assemblies after their contained namespace.";

        public const string DiagnosticId = "AV1505";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NamespaceMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor TypeInNamespaceRule = new DiagnosticDescriptor(DiagnosticId, Title,
            TypeInNamespaceMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor GlobalTypeRule = new DiagnosticDescriptor(DiagnosticId, Title,
            GlobalTypeMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeNamespaceAction = context =>
            context.SkipEmptyName(AnalyzeNamespace);

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeNamedTypeAction = context =>
            context.SkipEmptyName(AnalyzeNamedType);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NamespaceRule, TypeInNamespaceRule, GlobalTypeRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamespaceAction, SymbolKind.Namespace);
            context.RegisterSymbolAction(AnalyzeNamedTypeAction, SymbolKind.NamedType);
        }

        private static void AnalyzeNamespace(SymbolAnalysisContext context)
        {
            var namespaceSymbol = (INamespaceSymbol)context.Symbol;

            if (IsTopLevelNamespace(namespaceSymbol))
            {
                AnalyzeTopLevelNamespace(namespaceSymbol, context);
            }
        }

        private static bool IsTopLevelNamespace([NotNull] INamespaceSymbol namespaceSymbol)
        {
            return namespaceSymbol.ContainingNamespace.IsGlobalNamespace;
        }

        private static void AnalyzeTopLevelNamespace([NotNull] INamespaceSymbol namespaceSymbol, SymbolAnalysisContext context)
        {
            string reportAssemblyName = namespaceSymbol.ContainingAssembly.Name;
            string assemblyName = GetAssemblyNameWithoutCore(reportAssemblyName);

            context.CancellationToken.ThrowIfCancellationRequested();

            var visitor = new TypesInNamespaceVisitor(assemblyName, reportAssemblyName, context);
            visitor.Visit(namespaceSymbol);
        }

        [NotNull]
        private static string GetAssemblyNameWithoutCore([NotNull] string assemblyName)
        {
            if (assemblyName == "Core")
            {
                return string.Empty;
            }

            return assemblyName.EndsWith(".Core", StringComparison.Ordinal)
                ? assemblyName.Substring(0, assemblyName.Length - ".Core".Length)
                : assemblyName;
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.ContainingNamespace.IsGlobalNamespace && !type.IsSynthesized())
            {
                context.ReportDiagnostic(Diagnostic.Create(GlobalTypeRule, type.Locations[0], type.Name,
                    type.ContainingAssembly.Name));
            }
        }

        private sealed class TypesInNamespaceVisitor : SymbolVisitor
        {
            [ItemNotNull]
            private static readonly ImmutableArray<string> JetBrainsAnnotationsNamespace =
                ImmutableArray.Create("JetBrains", "Annotations");

            [NotNull]
            private static readonly char[] DotSeparator =
            {
                '.'
            };

            [ItemNotNull]
            private readonly ImmutableArray<string> assemblyNameParts;

            [NotNull]
            private readonly string reportAssemblyName;

            [NotNull]
            [ItemNotNull]
            private readonly Stack<string> namespaceNames = new Stack<string>();

            private SymbolAnalysisContext context;

            [NotNull]
            private string CurrentNamespaceName => string.Join(".", namespaceNames.Reverse());

            public TypesInNamespaceVisitor([NotNull] string assemblyName, [NotNull] string reportAssemblyName,
                SymbolAnalysisContext context)
            {
                Guard.NotNull(assemblyName, nameof(assemblyName));
                Guard.NotNullNorWhiteSpace(reportAssemblyName, nameof(reportAssemblyName));

                assemblyNameParts = assemblyName.Split(DotSeparator, StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();

                this.reportAssemblyName = reportAssemblyName;
                this.context = context;
            }

            public override void VisitNamespace([NotNull] INamespaceSymbol symbol)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                namespaceNames.Push(symbol.Name);

                if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequirePartialMatchWithAssemblyName) && !symbol.IsSynthesized())
                {
                    context.ReportDiagnostic(Diagnostic.Create(NamespaceRule, symbol.Locations[0], CurrentNamespaceName,
                        reportAssemblyName));
                }

                VisitChildren(symbol);

                namespaceNames.Pop();
            }

            private void VisitChildren([NotNull] INamespaceSymbol namespaceSymbol)
            {
                foreach (INamedTypeSymbol typeMember in namespaceSymbol.GetTypeMembers())
                {
                    VisitNamedType(typeMember);
                }

                foreach (INamespaceSymbol namespaceMember in namespaceSymbol.GetNamespaceMembers())
                {
                    VisitNamespace(namespaceMember);
                }
            }

            public override void VisitNamedType([NotNull] INamedTypeSymbol symbol)
            {
                if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequireCompleteMatchWithAssemblyName) &&
                    !symbol.IsSynthesized())
                {
                    context.ReportDiagnostic(Diagnostic.Create(TypeInNamespaceRule, symbol.Locations[0], symbol.Name,
                        CurrentNamespaceName, reportAssemblyName));
                }
            }

            private bool IsCurrentNamespaceAllowed(NamespaceMatchMode matchMode)
            {
                string[] currentNamespaceParts = namespaceNames.Reverse().ToArray();

                if (IsCurrentNamespacePartOfJetBrainsAnnotations(currentNamespaceParts))
                {
                    return true;
                }

                bool? isMatchOnParts = IsMatchOnNamespaceParts(currentNamespaceParts, matchMode);

                return isMatchOnParts == null || isMatchOnParts.Value;
            }

            private bool IsCurrentNamespacePartOfJetBrainsAnnotations([NotNull] [ItemNotNull] string[] currentNamespaceParts)
            {
                switch (currentNamespaceParts.Length)
                {
                    case 1:
                    {
                        return currentNamespaceParts[0] == JetBrainsAnnotationsNamespace[0];
                    }
                    case 2:
                    {
                        return currentNamespaceParts.SequenceEqual(JetBrainsAnnotationsNamespace);
                    }
                }

                return false;
            }

            [CanBeNull]
            private bool? IsMatchOnNamespaceParts([NotNull] [ItemNotNull] string[] currentNamespaceParts,
                NamespaceMatchMode matchMode)
            {
                if (matchMode == NamespaceMatchMode.RequireCompleteMatchWithAssemblyName &&
                    assemblyNameParts.Length > currentNamespaceParts.Length)
                {
                    return false;
                }

                int commonLength = Math.Min(currentNamespaceParts.Length, assemblyNameParts.Length);

                for (int index = 0; index < commonLength; index++)
                {
                    if (currentNamespaceParts[index] != assemblyNameParts[index])
                    {
                        return false;
                    }
                }

                return null;
            }

            private enum NamespaceMatchMode
            {
                RequirePartialMatchWithAssemblyName,
                RequireCompleteMatchWithAssemblyName
            }
        }
    }
}
