using System.Collections.Immutable;
using System.Xml;
using System.Xml.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Documentation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DocumentInternalMemberAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Missing XML comment for internally visible type, member or parameter";
    private const string MissingTypeOrMemberMessageFormat = "Missing XML comment for internally visible type or member '{0}'";
    private const string MissingParameterMessageFormat = "Missing XML comment for internally visible parameter '{0}'";
    private const string ExtraParameterMessageFormat = "Parameter '{0}' in XML comment not found in method signature";
    private const string Description = "Document all public, protected and internal types and members.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2305";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Documentation;

    private static readonly DiagnosticDescriptor MissingTypeOrMemberRule = new(DiagnosticId, Title, MissingTypeOrMemberMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor MissingParameterRule = new(DiagnosticId, Title, MissingParameterMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor ExtraParameterRule = new(DiagnosticId, Title, ExtraParameterMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
        ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

    private static readonly HashSet<string> EmptyHashSet = [];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingTypeOrMemberRule, MissingParameterRule, ExtraParameterRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.SafeRegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol.AreDocumentationCommentsReported() && IsTypeAccessible(context.Symbol))
        {
            AnalyzeSymbol(context.Symbol, context);
        }
    }

    private static bool IsTypeAccessible(ISymbol type)
    {
        return type.DeclaredAccessibility == Accessibility.Internal && type.IsSymbolAccessibleFromRoot();
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        if (context.Symbol.AreDocumentationCommentsReported() && !context.Symbol.IsPropertyOrEventAccessor() && IsMemberAccessible(context.Symbol))
        {
            AnalyzeSymbol(context.Symbol, context);
        }
    }

    private static bool IsMemberAccessible(ISymbol member)
    {
        return IsMemberInternal(member) || IsMemberPublicInInternalTypeHierarchy(member);
    }

    private static bool IsMemberInternal(ISymbol member)
    {
        bool isInternal = member.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedAndInternal;

        return isInternal && member.IsSymbolAccessibleFromRoot();
    }

    private static bool IsMemberPublicInInternalTypeHierarchy(ISymbol member)
    {
        return member.DeclaredAccessibility == Accessibility.Public && member.IsSymbolAccessibleFromRoot() && HasInternalTypeInHierarchy(member);
    }

    private static bool HasInternalTypeInHierarchy(ISymbol symbol)
    {
        ISymbol? container = symbol;

        while (container != null)
        {
            if (container.DeclaredAccessibility == Accessibility.Internal)
            {
                return true;
            }

            container = container.NullableContainingType;
        }

        return false;
    }

    private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
    {
        if (symbol.IsSynthesized())
        {
            return;
        }

        string? documentationXml = symbol.GetDocumentationCommentXml(null, false, context.CancellationToken);

        if (string.IsNullOrEmpty(documentationXml))
        {
            string name = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

            var diagnostic = Diagnostic.Create(MissingTypeOrMemberRule, symbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }

        if (symbol is IMethodSymbol method && method.Parameters.Any() && !InheritsDocumentation(documentationXml))
        {
            AnalyzeParameters(method.Parameters, documentationXml, context);
        }
    }

    private static bool InheritsDocumentation(string? documentationXml)
    {
        if (documentationXml == null)
        {
            return false;
        }

        int tagIndex = documentationXml.IndexOf("<inheritdoc", StringComparison.Ordinal);
        return tagIndex != -1;
    }

    private static void AnalyzeParameters(ImmutableArray<IParameterSymbol> parameters, string? documentationXml, SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        ISet<string>? parameterNamesInDocumentation = documentationXml is null or "" ? EmptyHashSet : TryParseDocumentationCommentXml(documentationXml);

        if (parameterNamesInDocumentation != null)
        {
            AnalyzeMissingParameters(parameters, parameterNamesInDocumentation, context);
            AnalyzeExtraParameters(parameterNamesInDocumentation, context);
        }
    }

    private static ISet<string>? TryParseDocumentationCommentXml(string documentationXml)
    {
        try
        {
            XDocument document = XDocument.Parse(documentationXml);

            return GetParameterNamesFromXml(document);
        }
        catch (XmlException)
        {
#pragma warning disable AV1135 // Do not return null for strings, collections or tasks
            return null;
#pragma warning restore AV1135 // Do not return null for strings, collections or tasks
        }
    }

    private static ISet<string> GetParameterNamesFromXml(XDocument document)
    {
        var parameterNames = new HashSet<string>();

        foreach (XElement paramElement in document.Element("member")?.Elements("param") ?? [])
        {
            XAttribute? paramAttribute = paramElement.Attribute("name");

            if (paramAttribute?.Value is not (null or ""))
            {
                parameterNames.Add(paramAttribute.Value);
            }
        }

        return parameterNames;
    }

    private static void AnalyzeMissingParameters(ImmutableArray<IParameterSymbol> parameters, ISet<string> parameterNamesInDocumentation,
        SymbolAnalysisContext context)
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
                        var diagnostic = Diagnostic.Create(MissingParameterRule, parameter.Locations[0], parameter.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else
                {
                    parameterNamesInDocumentation.Remove(parameter.Name);
                }
            }
        }
    }

    private static void AnalyzeExtraParameters(ISet<string> parameterNamesInDocumentation, SymbolAnalysisContext context)
    {
        if (context.Symbol.IsSynthesized())
        {
            return;
        }

        foreach (string parameterNameInDocumentation in parameterNamesInDocumentation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = Diagnostic.Create(ExtraParameterRule, context.Symbol.Locations[0], parameterNameInDocumentation);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
