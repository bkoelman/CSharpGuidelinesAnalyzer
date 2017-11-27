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
    public sealed class NamespacesShouldMatchAssemblyNameAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1505";

        private const string Title = "Namespaces should match with assembly name";
        private const string NamespaceMessageFormat = "Namespace '{0}' does not match with assembly name '{1}'.";

        private const string TypeInNamespaceMessageFormat =
            "Type '{0}' is declared in namespace '{1}', which does not match with assembly name '{2}'.";

        private const string GlobalTypeMessageFormat =
            "Type '{0}' is declared in global namespace, which does not match with assembly name '{1}'.";

        private const string Description = "Name assemblies after their contained namespace.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NamespaceMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor TypeInNamespaceRule = new DiagnosticDescriptor(DiagnosticId, Title,
            TypeInNamespaceMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor GlobalTypeRule = new DiagnosticDescriptor(DiagnosticId, Title,
            GlobalTypeMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NamespaceRule, TypeInNamespaceRule, GlobalTypeRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamespace), SymbolKind.Namespace);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
        }

        private void AnalyzeNamespace(SymbolAnalysisContext context)
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

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.ContainingNamespace.IsGlobalNamespace)
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

            [ItemNotNull]
            private readonly ImmutableArray<string> assemblyNameParts;

            [NotNull]
            private readonly string reportAssemblyName;

            private SymbolAnalysisContext context;

            [NotNull]
            [ItemNotNull]
            private readonly Stack<string> namespaceNames = new Stack<string>();

            [NotNull]
            private string CurrentNamespaceName => string.Join(".", namespaceNames.Reverse());

            [NotNull]
            private static readonly char[] DotSeparator = { '.' };

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

                if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequirePartialMatchWithAssemblyName))
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
                if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequireCompleteMatchWithAssemblyName))
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
