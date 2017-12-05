using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMisleadingNamesAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1712";

        private const string Title = "Identifier name is difficult to read";
        private const string MessageFormat = "{0} '{1}' has a name that is difficult to read.";
        private const string Description = "Avoid short names or names that can be mistaken for other names.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.HelpLinkUri);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> Blacklist = ImmutableArray.Create("b001", "lo", "I1", "lOl");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclarator), OperationKind.VariableDeclarator);

            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
        }

        private void AnalyzeVariableDeclarator(OperationAnalysisContext context)
        {
            var declarator = (IVariableDeclaratorOperation)context.Operation;
            ILocalSymbol variable = declarator.Symbol;

            if (Blacklist.Contains(variable.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name));
            }
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (Blacklist.Contains(parameter.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }
        }
    }
}
