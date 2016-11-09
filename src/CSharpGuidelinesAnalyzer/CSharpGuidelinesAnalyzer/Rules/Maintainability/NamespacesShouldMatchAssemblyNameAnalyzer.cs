using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamespacesShouldMatchAssemblyNameAnalyzer : DiagnosticAnalyzer
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(NamespaceRule, TypeInNamespaceRule, GlobalTypeRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamespace, SymbolKind.Namespace);
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private void AnalyzeNamespace(SymbolAnalysisContext context)
        {
            var namespaceSymbol = (INamespaceSymbol) context.Symbol;

            if (!IsTopLevelNamespace(namespaceSymbol))
            {
                return;
            }

            string reportAssemblyName = namespaceSymbol.ContainingAssembly.Name;
            string assemblyName = GetAssemblyNameWithoutCore(reportAssemblyName);

            context.CancellationToken.ThrowIfCancellationRequested();

            var visitor = new TypesInNamespaceVisitor(assemblyName, context.ReportDiagnostic, reportAssemblyName,
                context.CancellationToken);
            visitor.Visit(namespaceSymbol);
        }

        private static bool IsTopLevelNamespace([NotNull] INamespaceSymbol namespaceSymbol)
        {
            return namespaceSymbol.ContainingNamespace.IsGlobalNamespace;
        }

        [NotNull]
        private static string GetAssemblyNameWithoutCore([NotNull] string assemblyName)
        {
            if (assemblyName == "Core")
            {
                return string.Empty;
            }

            if (assemblyName.EndsWith(".Core", StringComparison.Ordinal))
            {
                return assemblyName.Substring(0, assemblyName.Length - ".Core".Length);
            }

            return assemblyName;
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

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
                new[] { "JetBrains", "Annotations" }.ToImmutableArray();

            [ItemNotNull]
            private readonly ImmutableArray<string> assemblyNameParts;

            [NotNull]
            private readonly Action<Diagnostic> reportDiagnostic;

            [NotNull]
            private readonly string reportAssemblyName;

            private CancellationToken cancellationToken;

            [NotNull]
            [ItemNotNull]
            private readonly Stack<string> namespaceNames = new Stack<string>();

            [NotNull]
            private string CurrentNamespaceName => string.Join(".", namespaceNames.Reverse());

            public TypesInNamespaceVisitor([NotNull] string assemblyName, [NotNull] Action<Diagnostic> reportDiagnostic,
                [NotNull] string reportAssemblyName, CancellationToken cancellationToken)
            {
                Guard.NotNullNorWhiteSpace(assemblyName, nameof(assemblyName));
                Guard.NotNullNorWhiteSpace(reportAssemblyName, nameof(reportAssemblyName));

                assemblyNameParts =
                    assemblyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();

                this.reportDiagnostic = reportDiagnostic;
                this.reportAssemblyName = reportAssemblyName;
                this.cancellationToken = cancellationToken;
            }

            public override void VisitNamespace([NotNull] INamespaceSymbol symbol)
            {
                cancellationToken.ThrowIfCancellationRequested();

                namespaceNames.Push(symbol.Name);

                if (!IsCurrentNamespaceValid(false))
                {
                    reportDiagnostic(Diagnostic.Create(NamespaceRule, symbol.Locations[0], CurrentNamespaceName,
                        reportAssemblyName));
                }

                foreach (INamedTypeSymbol typeMember in symbol.GetTypeMembers())
                {
                    VisitNamedType(typeMember);
                }

                foreach (INamespaceSymbol namespaceMember in symbol.GetNamespaceMembers())
                {
                    VisitNamespace(namespaceMember);
                }

                namespaceNames.Pop();
            }

            public override void VisitNamedType([NotNull] INamedTypeSymbol symbol)
            {
                if (!IsCurrentNamespaceValid(true))
                {
                    reportDiagnostic(Diagnostic.Create(TypeInNamespaceRule, symbol.Locations[0], symbol.Name,
                        CurrentNamespaceName, reportAssemblyName));
                }
            }

            private bool IsCurrentNamespaceValid(bool requireCompleteMatchWithAssemblyName)
            {
                string[] currentNamespaceParts = namespaceNames.Reverse().ToArray();

                if (IsCurrentNamespacePartOfJetBrainsAnnotations(currentNamespaceParts))
                {
                    return true;
                }

                if (requireCompleteMatchWithAssemblyName && assemblyNameParts.Length > currentNamespaceParts.Length)
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

                return true;
            }

            private bool IsCurrentNamespacePartOfJetBrainsAnnotations(
                [NotNull] [ItemNotNull] string[] currentNamespaceParts)
            {
                if (currentNamespaceParts.Length == 1)
                {
                    return currentNamespaceParts[0] == JetBrainsAnnotationsNamespace[0];
                }

                if (currentNamespaceParts.Length == 2)
                {
                    return currentNamespaceParts.SequenceEqual(JetBrainsAnnotationsNamespace);
                }

                return false;
            }
        }
    }
}