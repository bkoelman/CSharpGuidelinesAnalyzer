using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MembersShouldDoASingleThingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1115";

        private const string Title = "Member contains the word 'and'";
        private const string MessageFormat = "{0} {1} contains the word 'and'.";
        private const string Description = "A method or property should do only one thing.";
        private const string Category = "Class Design";

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
            var method = context.Symbol as IMethodSymbol;
            switch (method?.MethodKind)
            {
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                    return;
            }

            if (IdentifierNameContainsAnyOf(context.Symbol.Name, "And", "and"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                    context.Symbol.Name));
            }
        }

        private static bool IdentifierNameContainsAnyOf([NotNull] string identiferName,
            [NotNull] [ItemNotNull] params string[] wordsToFind)
        {
            List<string> wordsInText = AnalysisUtilities.ExtractWords(identiferName);

            foreach (string wordToFind in wordsToFind)
            {
                if (wordsInText.Contains(wordToFind))
                {
                    return true;
                }
            }

            return false;
        }
    }
}