using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Documentation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DocumentInternalMemberAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2305";

        private const string Title = "Missing XML comment for internally visible type, member or parameter";

        private const string MissingTypeOrMemberMessageFormat =
            "Missing XML comment for internally visible type or member '{0}'.";

        private const string MissingParameterMessageFormat = "Missing XML comment for internally visible parameter '{0}'.";
        private const string ExtraParameterMessageFormat = "Parameter '{0}' in XML comment not found in method signature.";
        private const string Description = "Document all public, protected and internal types and members.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Documentation;

        [NotNull]
        private static readonly DiagnosticDescriptor MissingTypeOrMemberRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MissingTypeOrMemberMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor MissingParameterRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MissingParameterMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ExtraParameterRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ExtraParameterMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(MissingTypeOrMemberRule, MissingParameterRule, ExtraParameterRule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

        [NotNull]
        [ItemNotNull]
        private static readonly HashSet<string> EmptyHashSet = new HashSet<string>();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (context.Symbol.AreDocumentationCommentsReported() && IsTypeAccessible(context.Symbol))
            {
                AnalyzeSymbol(context.Symbol, context);
            }
        }

        private static bool IsTypeAccessible([NotNull] ISymbol type)
        {
            return type.DeclaredAccessibility == Accessibility.Internal && type.IsSymbolAccessibleFromRoot();
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.AreDocumentationCommentsReported() && !context.Symbol.IsPropertyOrEventAccessor() &&
                IsMemberAccessible(context.Symbol))
            {
                AnalyzeSymbol(context.Symbol, context);
            }
        }

        private static bool IsMemberAccessible([NotNull] ISymbol member)
        {
            return IsMemberInternal(member) || IsMemberPublicInInternalTypeHierarchy(member);
        }

        private static bool IsMemberInternal([NotNull] ISymbol member)
        {
            bool isInternal = member.DeclaredAccessibility == Accessibility.Internal ||
                member.DeclaredAccessibility == Accessibility.ProtectedAndInternal;

            return isInternal && member.IsSymbolAccessibleFromRoot();
        }

        private static bool IsMemberPublicInInternalTypeHierarchy([NotNull] ISymbol member)
        {
            return member.DeclaredAccessibility == Accessibility.Public && member.IsSymbolAccessibleFromRoot() &&
                HasInternalTypeInHierarchy(member);
        }

        private static bool HasInternalTypeInHierarchy([NotNull] ISymbol symbol)
        {
            ISymbol container = symbol;
            while (container != null)
            {
                if (container.DeclaredAccessibility == Accessibility.Internal)
                {
                    return true;
                }

                container = container.ContainingType;
            }

            return false;
        }

        private static void AnalyzeSymbol([NotNull] ISymbol symbol, SymbolAnalysisContext context)
        {
            if (symbol.IsSynthesized())
            {
                return;
            }

            string documentationXml = symbol.GetDocumentationCommentXml(null, false, context.CancellationToken);
            if (string.IsNullOrEmpty(documentationXml))
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingTypeOrMemberRule, symbol.Locations[0],
                    symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }

            if (symbol is IMethodSymbol method && method.Parameters.Any() && !InheritsDocumentation(documentationXml))
            {
                AnalyzeParameters(method.Parameters, documentationXml, context);
            }
        }

        private static bool InheritsDocumentation([CanBeNull] string documentationXml)
        {
            if (documentationXml == null)
            {
                return false;
            }

            int tagIndex = documentationXml.IndexOf("<inheritdoc", StringComparison.Ordinal);
            return tagIndex != -1;
        }

        private static void AnalyzeParameters([ItemNotNull] ImmutableArray<IParameterSymbol> parameters,
            [CanBeNull] string documentationXml, SymbolAnalysisContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            ISet<string> parameterNamesInDocumentation = string.IsNullOrEmpty(documentationXml)
                ? EmptyHashSet
                : TryParseDocumentationCommentXml(documentationXml);

            if (parameterNamesInDocumentation != null)
            {
                AnalyzeMissingParameters(parameters, parameterNamesInDocumentation, context);
                AnalyzeExtraParameters(parameterNamesInDocumentation, context);
            }
        }

        [CanBeNull]
        [ItemNotNull]
        private static ISet<string> TryParseDocumentationCommentXml([NotNull] string documentationXml)
        {
            try
            {
                XDocument document = XDocument.Parse(documentationXml);

                return GetParameterNamesFromXml(document);
            }
            catch (XmlException)
            {
                return null;
            }
        }

        [NotNull]
        [ItemNotNull]
        private static ISet<string> GetParameterNamesFromXml([NotNull] XDocument document)
        {
            var parameterNames = new HashSet<string>();

            foreach (XElement paramElement in document.Element("member")?.Elements("param") ?? ImmutableArray<XElement>.Empty)
            {
                XAttribute paramAttribute = paramElement.Attribute("name");

                if (!string.IsNullOrEmpty(paramAttribute?.Value))
                {
                    parameterNames.Add(paramAttribute.Value);
                }
            }

            return parameterNames;
        }

        private static void AnalyzeMissingParameters([ItemNotNull] ImmutableArray<IParameterSymbol> parameters,
            [NotNull] [ItemNotNull] ISet<string> parameterNamesInDocumentation, SymbolAnalysisContext context)
        {
            foreach (IParameterSymbol parameter in parameters)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    if (!parameterNamesInDocumentation.Contains(parameter.Name))
                    {
                        if (!parameter.IsSynthesized())
                        {
                            context.ReportDiagnostic(Diagnostic.Create(MissingParameterRule, parameter.Locations[0],
                                parameter.Name));
                        }
                    }
                    else
                    {
                        parameterNamesInDocumentation.Remove(parameter.Name);
                    }
                }
            }
        }

        private static void AnalyzeExtraParameters([NotNull] [ItemNotNull] ISet<string> parameterNamesInDocumentation,
            SymbolAnalysisContext context)
        {
            if (context.Symbol.IsSynthesized())
            {
                return;
            }

            foreach (string parameterNameInDocumentation in parameterNamesInDocumentation)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                context.ReportDiagnostic(Diagnostic.Create(ExtraParameterRule, context.Symbol.Locations[0],
                    parameterNameInDocumentation));
            }
        }
    }
}
