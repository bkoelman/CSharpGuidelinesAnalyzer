using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseFrameworkTerminologyInMemberNamesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1711";

        private const string Title = "AV1711";
        private const string MessageFormat = "{0} '{1}' should be renamed to '{2}'.";
        private const string Description = "Name members similarly to members of related .NET Framework classes.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            new[] { SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event }.ToImmutableArray();

        [NotNull]
        private static readonly ImmutableDictionary<string, string> WordsReplacementMap =
            new Dictionary<string, string>
            {
                { "AddItem", "Add" },
                { "Delete", "Remove" },
                { "NumberOfItems", "Count" }
            }.ToImmutableDictionary();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (AnalysisUtilities.IsPropertyOrEventAccessor(context.Symbol))
            {
                return;
            }

            if (WordsReplacementMap.ContainsKey(context.Symbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                    context.Symbol.Name, WordsReplacementMap[context.Symbol.Name]));
            }
        }
    }
}