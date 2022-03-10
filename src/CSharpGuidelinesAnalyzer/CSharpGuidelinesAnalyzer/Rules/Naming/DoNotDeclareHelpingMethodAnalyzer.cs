using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotDeclareHelpingMethodAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Type name contains term that should be avoided";
        private const string MessageFormat = "Name of type '{0}' contains the term '{1}'";
        private const string Description = "Name types using nouns, noun phrases or adjective phrases.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1708";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("Utility", "Utilities", "Facility",
            "Facilities", "Helper", "Helpers", "Common", "Shared");

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeNamedTypeAction = context => context.SkipEmptyName(AnalyzeNamedType);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamedTypeAction, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.IsSynthesized())
            {
                return;
            }

            ICollection<WordToken> wordsListed = type.Name.GetWordsInList(WordsBlacklist);

            if (wordsListed.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.Name, wordsListed.First().Text));
            }
        }
    }
}
