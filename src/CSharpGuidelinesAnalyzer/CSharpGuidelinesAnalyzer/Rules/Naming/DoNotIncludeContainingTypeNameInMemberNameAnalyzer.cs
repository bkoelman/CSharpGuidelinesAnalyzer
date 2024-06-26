using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotIncludeContainingTypeNameInMemberNameAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Member name includes the name of its containing type";
    private const string MessageFormat = "{0} '{1}' contains the name of its containing type '{2}'";
    private const string Description = "Don't repeat the name of a class or enumeration in its members.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1710";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
        ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Field, SymbolKind.Event);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeMember, MemberSymbolKinds);
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
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

    private static void AnalyzeMemberName([NotNull] string containingTypeName, SymbolAnalysisContext context)
    {
        string memberName = context.Symbol.MemberNameWithoutExplicitInterfacePrefix();

        if (memberName.Contains(containingTypeName) && !IsUserNameInUserType(containingTypeName, memberName))
        {
            var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Kind, context.Symbol.Name, containingTypeName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsUserNameInUserType([NotNull] string containingTypeName, [NotNull] string memberName)
    {
        return containingTypeName == "User" && string.Equals(memberName, "username", StringComparison.OrdinalIgnoreCase);
    }
}
