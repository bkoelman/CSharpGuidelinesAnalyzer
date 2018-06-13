using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotIncludeContainingTypeNameInMemberNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1710";

        private const string Title = "Member name includes the name of its containing type";
        private const string MessageFormat = "{0} '{1}' contains the name of its containing type '{2}'.";
        private const string Description = "Don't repeat the name of a class or enumeration in its members.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsSynthesized())
            {
                return;
            }

            string typeName = context.Symbol.ContainingType.Name;
            if (typeName.Length < 2 || context.Symbol.IsPropertyOrEventAccessor())
            {
                return;
            }

            AnalyzeMemberName(typeName, context);
        }

        private void AnalyzeMemberName([NotNull] string containingTypeName, SymbolAnalysisContext context)
        {
            string memberName = WithoutExplicitInterfacePrefix(context.Symbol.Name);
            if (memberName.Contains(containingTypeName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind,
                    context.Symbol.Name, containingTypeName));
            }
        }

        [NotNull]
        private string WithoutExplicitInterfacePrefix([NotNull] string name)
        {
            int index = name.LastIndexOf(".", StringComparison.Ordinal);
            return index != -1 ? name.Substring(index) : name;
        }
    }
}
