using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotIncludeContainingTypeNameInMembersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1710";

        private const string Title = "Member name includes the name of its containing type";
        private const string MessageFormat = "{0} '{1}' contains the name of its containing type '{2}'.";
        private const string Description = "Don't repeat the name of a class or enumeration in its members.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds = ImmutableArray.Create(SymbolKind.Property,
            SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            string typeName = context.Symbol.ContainingType.Name;

            if (typeName.Length < 2 || context.Symbol.IsPropertyOrEventAccessor())
            {
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
