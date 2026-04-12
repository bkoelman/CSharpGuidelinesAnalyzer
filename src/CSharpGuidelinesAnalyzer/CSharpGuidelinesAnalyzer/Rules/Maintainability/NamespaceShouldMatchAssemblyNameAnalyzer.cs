using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamespaceShouldMatchAssemblyNameAnalyzer : DiagnosticAnalyzer
{
    // Copied from Microsoft.CodeAnalysis.WellKnownMemberNames, which provides these in later versions.
    private const string TopLevelStatementsEntryPointTypeName = "Program";
    private const string TopLevelStatementsEntryPointMethodName = "<Main>$";

    private const string Title = "Namespace should match with assembly name";
    private const string NamespaceMessageFormat = "Namespace '{0}' does not match with assembly name '{1}'";
    private const string TypeInNamespaceMessageFormat = "Type '{0}' is declared in namespace '{1}', which does not match with assembly name '{2}'";
    private const string GlobalTypeMessageFormat = "Type '{0}' is declared in global namespace, which does not match with assembly name '{1}'";
    private const string Description = "Name assemblies after their contained namespace.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1505";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor NamespaceRule = new(DiagnosticId, Title, NamespaceMessageFormat, Category.DisplayName, DiagnosticSeverity.Info,
        true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor TypeInNamespaceRule = new(DiagnosticId, Title, TypeInNamespaceMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor GlobalTypeRule = new(DiagnosticId, Title, GlobalTypeMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NamespaceRule, TypeInNamespaceRule, GlobalTypeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeNamespace, SymbolKind.Namespace);
        context.SafeRegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamespace(SymbolAnalysisContext context)
    {
        var namespaceSymbol = (INamespaceSymbol)context.Symbol;

        if (IsTopLevelNamespace(namespaceSymbol))
        {
            AnalyzeTopLevelNamespace(namespaceSymbol, context);
        }
    }

    private static bool IsTopLevelNamespace(INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.ContainingNamespace.IsGlobalNamespace;
    }

    private static void AnalyzeTopLevelNamespace(INamespaceSymbol namespaceSymbol, SymbolAnalysisContext context)
    {
        string reportAssemblyName = namespaceSymbol.ContainingAssembly.Name;
        string assemblyName = GetAssemblyNameWithoutCore(reportAssemblyName);

        context.CancellationToken.ThrowIfCancellationRequested();

        var visitor = new TypesInNamespaceVisitor(assemblyName, reportAssemblyName, context);
        visitor.Visit(namespaceSymbol);
    }

    private static string GetAssemblyNameWithoutCore(string assemblyName)
    {
        if (assemblyName == "Core")
        {
            return string.Empty;
        }

        return assemblyName.EndsWith(".Core", StringComparison.Ordinal) ? assemblyName.Substring(0, assemblyName.Length - ".Core".Length) : assemblyName;
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.ContainingNamespace.IsGlobalNamespace && !type.IsSynthesized() && !IsTopLevelStatementsContainer(type))
        {
            var diagnostic = Diagnostic.Create(GlobalTypeRule, type.Locations[0], type.Name, type.ContainingAssembly.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsTopLevelStatementsContainer(INamedTypeSymbol type)
    {
        return type.Name == TopLevelStatementsEntryPointTypeName && type.GetMembers(TopLevelStatementsEntryPointMethodName).Any();
    }

    private sealed class TypesInNamespaceVisitor : SymbolVisitor
    {
        private static readonly ImmutableArray<string> JetBrainsAnnotationsNamespace = ImmutableArray.Create("JetBrains", "Annotations");

        private static readonly char[] DotSeparator = ['.'];

        private readonly ImmutableArray<string> assemblyNameParts;

        private readonly string reportAssemblyName;

        private readonly Stack<string> namespaceNames = new();

        private readonly SymbolAnalysisContext context;

        private string CurrentNamespaceName
        {
            get
            {
                IEnumerable<string> reversed = namespaceNames.Reverse();
                return string.Join(".", reversed);
            }
        }

        public TypesInNamespaceVisitor(string assemblyName, string reportAssemblyName, SymbolAnalysisContext context)
        {
            ArgumentNullException.ThrowIfNull(assemblyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(reportAssemblyName);

            assemblyNameParts = assemblyName.Split(DotSeparator, StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();

            this.reportAssemblyName = reportAssemblyName;
            this.context = context;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            namespaceNames.Push(symbol.Name);

            if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequirePartialMatchWithAssemblyName) && !symbol.IsSynthesized())
            {
                var diagnostic = Diagnostic.Create(NamespaceRule, symbol.Locations[0], CurrentNamespaceName, reportAssemblyName);
                context.ReportDiagnostic(diagnostic);
            }

            VisitChildren(symbol);

            namespaceNames.Pop();
        }

        private void VisitChildren(INamespaceSymbol namespaceSymbol)
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

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (!IsCurrentNamespaceAllowed(NamespaceMatchMode.RequireCompleteMatchWithAssemblyName) && !symbol.IsSynthesized())
            {
                var diagnostic = Diagnostic.Create(TypeInNamespaceRule, symbol.Locations[0], symbol.Name, CurrentNamespaceName, reportAssemblyName);
                context.ReportDiagnostic(diagnostic);
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

        private bool IsCurrentNamespacePartOfJetBrainsAnnotations(string[] currentNamespaceParts)
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

        private bool? IsMatchOnNamespaceParts(string[] currentNamespaceParts, NamespaceMatchMode matchMode)
        {
            if (matchMode == NamespaceMatchMode.RequireCompleteMatchWithAssemblyName && assemblyNameParts.Length > currentNamespaceParts.Length)
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
