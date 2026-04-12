using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseOptionalParameterInTypeHierarchyAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Method contains optional parameter in type hierarchy";
    private const string MessageFormat = "Method '{0}' contains optional parameter '{1}'";
    private const string Description = "Do not use optional parameters in interface methods or their concrete implementations.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1554";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameter = (IParameterSymbol)context.Symbol;

        if (parameter.IsOptional)
        {
            INamedTypeSymbol? type = parameter.NullableContainingType;

            if (parameter.NullableContainingSymbol is not IMethodSymbol method)
            {
                return;
            }

            if (type?.TypeKind == TypeKind.Interface || method.IsInterfaceImplementation() || method.IsAbstract || method.IsVirtual || method.IsOverride)
            {
                if (!IsOverrideFromExternalAssembly(method) && !IsInterfaceImplementationFromExternalAssembly(method))
                {
                    string containerName = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                    SyntaxReference syntaxReference = parameter.DeclaringSyntaxReferences.First();
                    var location = Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);

                    var diagnostic = Diagnostic.Create(Rule, location, containerName, parameter.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsOverrideFromExternalAssembly(IMethodSymbol method)
    {
        IMethodSymbol? baseMethod = method.OverriddenMethod;

        while (baseMethod != null)
        {
            if (!Equals(baseMethod.NullableContainingAssembly, method.NullableContainingAssembly))
            {
                return true;
            }

            baseMethod = baseMethod.OverriddenMethod;
        }

        return false;
    }

    private static bool IsInterfaceImplementationFromExternalAssembly(IMethodSymbol method)
    {
        INamedTypeSymbol? methodContainingType = method.NullableContainingType;

        if (methodContainingType != null)
        {
            foreach (ISymbol interfaceMethod in methodContainingType.AllInterfaces.SelectMany(@interface => @interface.GetMembers()))
            {
                ISymbol? implementer = methodContainingType.FindImplementationForInterfaceMember(interfaceMethod);

                if (Equals(method, implementer))
                {
                    return !Equals(method.NullableContainingAssembly, interfaceMethod.NullableContainingAssembly);
                }
            }
        }

        return false;
    }
}
