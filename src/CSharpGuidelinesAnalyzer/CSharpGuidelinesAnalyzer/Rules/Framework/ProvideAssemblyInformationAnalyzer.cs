using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ProvideAssemblyInformationAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV2215";

        private const string Title = "Assembly-level attribute is missing or empty";
        private const string MessageFormat = "Assembly-level attribute '{0}' is missing or empty.";
        private const string Description = "Properly fill the attributes of the AssemblyInfo.cs file.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                ConcurrentQueue<INamedTypeSymbol> assemblyAttributesToAnalyze =
                    GetAssemblyAttributesToAnalyze(startContext.Compilation);

                ImmutableArray<AttributeData> attributesInCompilation = startContext.Compilation.Assembly.GetAttributes();

                if (assemblyAttributesToAnalyze.Any())
                {
                    startContext.RegisterSemanticModelAction(c =>
                        AnalyzeSemanticModel(c, assemblyAttributesToAnalyze, attributesInCompilation));
                }
            });
        }

        [NotNull]
        [ItemNotNull]
#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        private ConcurrentQueue<INamedTypeSymbol> GetAssemblyAttributesToAnalyze([NotNull] Compilation compilation)
#pragma warning restore AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        {
            ImmutableArray<INamedTypeSymbol> attributes = ImmutableArray.Create(
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyTitleAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyDescriptionAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyConfigurationAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyCompanyAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyProductAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyCopyrightAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyTrademarkAttribute"),
                compilation.GetTypeByMetadataName("System.Reflection.AssemblyVersionAttribute"));

            return attributes.Any(attr => attr == null)
                ? new ConcurrentQueue<INamedTypeSymbol>()
                : new ConcurrentQueue<INamedTypeSymbol>(attributes);
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext context,
            [NotNull] [ItemNotNull] ConcurrentQueue<INamedTypeSymbol> assemblyAttributesToAnalyze,
            [ItemNotNull] ImmutableArray<AttributeData> attributesInCompilation)
        {
            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!assemblyAttributesToAnalyze.TryDequeue(out INamedTypeSymbol assemblyAttributeToAnalyze))
                {
                    return;
                }

                AnalyzeAssemblyAttribute(assemblyAttributeToAnalyze, attributesInCompilation, context);
            }
        }

        private static void AnalyzeAssemblyAttribute([NotNull] INamedTypeSymbol assemblyAttributeToAnalyze,
            [ItemNotNull] ImmutableArray<AttributeData> attributesInCompilation, SemanticModelAnalysisContext context)
        {
            AttributeData compilationAttribute =
                attributesInCompilation.FirstOrDefault(attr => assemblyAttributeToAnalyze.Equals(attr.AttributeClass));

            if (compilationAttribute == null)
            {
                ReportAt(Location.None, assemblyAttributeToAnalyze, context);
            }
            else
            {
                AnalyzeExistingAttribute(compilationAttribute, assemblyAttributeToAnalyze, context);
            }
        }

        private static void AnalyzeExistingAttribute([NotNull] AttributeData attributeInCompilation,
            [NotNull] INamedTypeSymbol assemblyAttributeToAnalyze, SemanticModelAnalysisContext context)
        {
            TypedConstant firstConstructorArgument = attributeInCompilation.ConstructorArguments.FirstOrDefault();
            string firstStringArgument = firstConstructorArgument.Value as string;

            if (string.IsNullOrEmpty(firstStringArgument))
            {
                SyntaxNode syntaxNode = attributeInCompilation.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);
                Location location = syntaxNode != null ? syntaxNode.GetLocation() : Location.None;

                ReportAt(location, assemblyAttributeToAnalyze, context);
            }
        }

        private static void ReportAt([NotNull] Location locationToReport, [NotNull] INamedTypeSymbol assemblyAttribute,
            SemanticModelAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, locationToReport,
                assemblyAttribute.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }
    }
}
