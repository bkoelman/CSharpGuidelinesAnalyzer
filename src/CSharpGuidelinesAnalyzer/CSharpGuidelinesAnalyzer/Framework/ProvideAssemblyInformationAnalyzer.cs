using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ProvideAssemblyInformationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2215";

        private const string Title = "Assembly-level attribute is missing or empty.";
        private const string MessageFormat = "Assembly-level attribute '{0}' is missing or empty.";
        private const string Description = "Properly fill the attributes of the AssemblyInfo.cs file.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var types = new AttributeTypes(startContext.Compilation);
                if (types.AssemblyAttributes.Any())
                {
                    startContext.RegisterSemanticModelAction(c => AnalyzeSemanticModel(c, types));
                }
            });
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext context, [NotNull] AttributeTypes types)
        {
            ImmutableArray<AttributeData> compilationAttributes =
                context.SemanticModel.Compilation.Assembly.GetAttributes();

            foreach (INamedTypeSymbol assemblyAttribute in types.AssemblyAttributes)
            {
                Location locationToReport = null;

                AttributeData compilationAttribute =
                    compilationAttributes.FirstOrDefault(attr => assemblyAttribute.Equals(attr.AttributeClass));
                if (compilationAttribute == null)
                {
                    locationToReport = Location.None;
                }
                else
                {
                    TypedConstant firstConstructorArgument = compilationAttribute.ConstructorArguments.FirstOrDefault();
                    string firstStringArgument = firstConstructorArgument.Value as string;

                    if (string.IsNullOrEmpty(firstStringArgument))
                    {
                        SyntaxNode syntaxNode =
                            compilationAttribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);
                        locationToReport = syntaxNode != null ? syntaxNode.GetLocation() : Location.None;
                    }
                }

                if (locationToReport != null)
                {
                    ReportAt(locationToReport, assemblyAttribute, context);
                }
            }
        }

        private void ReportAt([NotNull] Location location, [NotNull] INamedTypeSymbol assemblyAttribute,
            SemanticModelAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                assemblyAttribute.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }

        private sealed class AttributeTypes
        {
            [ItemNotNull]
            public ImmutableArray<INamedTypeSymbol> AssemblyAttributes { get; }

            public AttributeTypes([NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                ImmutableArray<INamedTypeSymbol> attributes =
                    new[]
                        {
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyTitleAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyDescriptionAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyConfigurationAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyCompanyAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyProductAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyCopyrightAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyTrademarkAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyVersionAttribute"),
                            compilation.GetTypeByMetadataName("System.Reflection.AssemblyFileVersionAttribute")
                        }
                        .ToImmutableArray();

                AssemblyAttributes = attributes.Any(attr => attr == null)
                    ? ImmutableArray<INamedTypeSymbol>.Empty
                    : attributes;
            }
        }
    }
}