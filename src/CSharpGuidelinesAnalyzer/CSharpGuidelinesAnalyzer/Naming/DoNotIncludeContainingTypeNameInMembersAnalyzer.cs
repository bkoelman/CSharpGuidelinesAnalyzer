using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotIncludeContainingTypeNameInMembersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1710";

        private const string Title = "Member name includes the name of its containing type";
        private const string MessageFormat = "{0} {1} contains the name of its containing type {2}.";
        private const string Description = "Don't repeat the name of a class or enumeration in its members.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> SymbolKinds =
            new[] { SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMember, SymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            string typeName = context.Symbol.ContainingType.Name;

            var method = context.Symbol as IMethodSymbol;
            switch (method?.MethodKind)
            {
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                    return;
            }

            if (context.Symbol.Name.Contains(typeName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                    context.Symbol.Name, typeName));
            }
        }
    }
}