using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;
using CSharpLanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicLanguageVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

// ReSharper disable once CheckNamespace
namespace RoslynTestFramework
{
    /// <summary />
    internal sealed class DocumentFactory
    {
        [NotNull]
        private static readonly CSharpCompilationOptions DefaultCSharpCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        [NotNull]
        private static readonly VisualBasicCompilationOptions DefaultBasicCompilationOptions =
            new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        [NotNull]
        private static readonly CSharpParseOptions DefaultCSharpParseOptions =
#if NET45 || NET452
            new CSharpParseOptions();
#else
            new CSharpParseOptions(CSharpLanguageVersion.Preview);
#endif

        [NotNull]
        private static readonly VisualBasicParseOptions DefaultBasicParseOptions =
#if NET45 || NET452
            new VisualBasicParseOptions();
#else
            new VisualBasicParseOptions(VisualBasicLanguageVersion.Latest);
#endif

        [NotNull]
        public string FormatSourceCode([NotNull] string sourceCode, [NotNull] AnalyzerTestContext context)
        {
            FrameworkGuard.NotNull(context, nameof(context));
            FrameworkGuard.NotNull(sourceCode, nameof(sourceCode));

            Document document = ToDocument(sourceCode, context);
            return FormatDocument(document);
        }

        [NotNull]
        public static Document ToDocument([NotNull] string code, [NotNull] AnalyzerTestContext context)
        {
            ParseOptions parseOptions = GetParseOptions(context.DocumentationMode, context.LanguageName);
            CompilationOptions compilationOptions = GetCompilationOptions(context);

            Document document = new AdhocWorkspace()
                .AddProject(context.AssemblyName, context.LanguageName)
                .WithParseOptions(parseOptions)
                .WithCompilationOptions(compilationOptions)
                .AddMetadataReferences(context.References)
                .AddDocument(context.FileName, code);

            return document;
        }

        [NotNull]
        private static ParseOptions GetParseOptions(DocumentationMode documentationMode, [NotNull] string languageName)
        {
            return languageName == LanguageNames.VisualBasic
                ? (ParseOptions)DefaultBasicParseOptions.WithDocumentationMode(documentationMode)
                : DefaultCSharpParseOptions.WithDocumentationMode(documentationMode);
        }

        [NotNull]
        private static CompilationOptions GetCompilationOptions([NotNull] AnalyzerTestContext context)
        {
            CompilationOptions options = context.LanguageName == LanguageNames.VisualBasic
                ? GetBasicCompilationOptions()
                : GetCSharpCompilationOptions(context.CompilerWarningLevel);

            options = options.WithOutputKind(context.OutputKind);

            if (context.WarningsAsErrors == TreatWarningsAsErrors.All)
            {
                options = options.WithGeneralDiagnosticOption(ReportDiagnostic.Error);
            }

#if !NET45 && !NET452
            if (context.NullableReferenceTypesSupport == NullableReferenceTypesSupport.Enabled &&
                context.LanguageName == LanguageNames.CSharp)
            {
                options = ((CSharpCompilationOptions)options).WithNullableContextOptions(NullableContextOptions.Enable);
            }
#endif

            return options;
        }

        [NotNull]
        private static CompilationOptions GetBasicCompilationOptions()
        {
            return DefaultBasicCompilationOptions;
        }

        [NotNull]
        private static CompilationOptions GetCSharpCompilationOptions([CanBeNull] int? compilerWarningLevel)
        {
            CSharpCompilationOptions options = DefaultCSharpCompilationOptions;

            if (compilerWarningLevel != null)
            {
                options = options.WithWarningLevel(compilerWarningLevel.Value);
            }

            return options;
        }

        [NotNull]
        public string FormatDocument([NotNull] Document document)
        {
            FrameworkGuard.NotNull(document, nameof(document));

            SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;

            SyntaxNode formattedSyntaxRoot = Formatter.Format(syntaxRoot, document.Project.Solution.Workspace);
            return formattedSyntaxRoot.ToFullString();
        }
    }
}
