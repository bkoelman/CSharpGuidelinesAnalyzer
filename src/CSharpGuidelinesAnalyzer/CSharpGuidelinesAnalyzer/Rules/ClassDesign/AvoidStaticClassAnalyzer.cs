﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidStaticClassAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Class should not be static";
    private const string TypeMessageFormat = "Class '{0}' should be non-static or its name should be suffixed with 'Extensions'";
    private const string MemberMessageFormat = "Extension method container class '{0}' contains {1} member '{2}', which is not an extension method";
    private const string Description = "Avoid static classes.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1008";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

    [NotNull]
    private static readonly DiagnosticDescriptor TypeRule = new(DiagnosticId, Title, TypeMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly DiagnosticDescriptor MemberRule = new(DiagnosticId, Title, MemberMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    private static readonly ImmutableArray<string> PlatformInvokeWrapperTypeNames =
        ImmutableArray.Create("NativeMethods", "SafeNativeMethods", "UnsafeNativeMethods");

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TypeRule, MemberRule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (!type.IsStatic || type.IsSynthesized() || IsPlatformInvokeWrapper(type))
        {
            return;
        }

        if (!type.Name.EndsWith("Extensions", StringComparison.Ordinal))
        {
            if (!TypeContainsEntryPoint(type, context.Compilation, context.CancellationToken))
            {
                var diagnostic = Diagnostic.Create(TypeRule, type.Locations[0], type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        else
        {
            AnalyzeTypeMembers(type, context);
        }
    }

    private static bool IsPlatformInvokeWrapper([NotNull] INamedTypeSymbol type)
    {
        return PlatformInvokeWrapperTypeNames.Contains(type.Name);
    }

    private static bool TypeContainsEntryPoint([NotNull] INamedTypeSymbol type, [NotNull] Compilation compilation, CancellationToken cancellationToken)
    {
        return type.GetMembers().OfType<IMethodSymbol>().Any(method => method.IsEntryPoint(compilation, cancellationToken));
    }

    private static void AnalyzeTypeMembers([NotNull] INamedTypeSymbol type, SymbolAnalysisContext context)
    {
        IEnumerable<ISymbol> accessibleMembers = type.GetMembers().Where(IsPublicOrInternal)
            .Where(member => !IsNestedType(member) && !member.IsPropertyOrEventAccessor());

        foreach (ISymbol member in accessibleMembers)
        {
            AnalyzeAccessibleMember(member, type, context);
        }
    }

    private static void AnalyzeAccessibleMember([NotNull] ISymbol member, [NotNull] INamedTypeSymbol containingType, SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (member.IsSynthesized())
        {
            return;
        }

        if (member is IMethodSymbol method && (method.IsExtensionMethod || method.MethodKind == MethodKind.StaticConstructor))
        {
            return;
        }

        string accessibility = member.DeclaredAccessibility.ToText().ToLowerInvariant();

        var diagnostic = Diagnostic.Create(MemberRule, member.Locations[0], containingType.Name, accessibility, member.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsPublicOrInternal([NotNull] ISymbol member)
    {
        return member.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
    }

    private static bool IsNestedType([NotNull] ISymbol member)
    {
        return member is ITypeSymbol;
    }
}
