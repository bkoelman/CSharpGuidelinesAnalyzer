using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMisleadingNamesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1712";

        private const string Title = "Identifier name is difficult to read";
        private const string MessageFormat = "{0} '{1}' has a name that is difficult to read.";
        private const string Description = "Avoid short names or names that can be mistaken for other names.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> Blacklist = ImmutableArray.Create("b001", "lo", "I1", "lOl");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclaration),
                        OperationKind.VariableDeclaration);
                }
            });

            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration) context.Operation;
            ILocalSymbol variable = declaration.Variable;

            if (Blacklist.Contains(variable.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Locations[0], "Variable", variable.Name));
            }
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (string.IsNullOrEmpty(parameter.Name))
            {
                return;
            }

            if (Blacklist.Contains(parameter.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Kind, parameter.Name));
            }
        }
    }
}