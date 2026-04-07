using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OverloadShouldCallOtherOverloadAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Method overload should call another overload";
    private const string InvokeMessageFormat = "Overloaded method '{0}' should call another overload";
    private const string MakeVirtualMessageFormat = "Method overload with the most parameters should be virtual";
    private const string OrderMessageFormat = "Parameter order in '{0}' does not match with the parameter order of the longest overload";
    private const string Description = "Call the more overloaded method from other overloads.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1551";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor InvokeRule = new(DiagnosticId, Title, InvokeMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning,
        true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor MakeVirtualRule = new(DiagnosticId, Title, MakeVirtualMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor OrderRule = new(DiagnosticId, Title, OrderMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning,
        true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<MethodKind> RegularMethodKinds = new[]
    {
        MethodKind.Ordinary,
        MethodKind.ExplicitInterfaceImplementation,
        MethodKind.ReducedExtension
    }.ToImmutableArray();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvokeRule, MakeVirtualRule, OrderRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
        {
            return;
        }

        IGrouping<string, IMethodSymbol>[] methodGroups = GetRegularMethodsInTypeHierarchy(type, context.CancellationToken).GroupBy(method => method.Name)
            .Where(HasAtLeastTwoItems).ToArray();

        foreach (IGrouping<string, IMethodSymbol> methodGroup in methodGroups)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            IMethodSymbol[] methodGroupArray = methodGroup.ToArray();
            AnalyzeMethodGroup(methodGroupArray, type, context);
        }
    }

    private static IEnumerable<IMethodSymbol> GetRegularMethodsInTypeHierarchy(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        return EnumerateSelfWithBaseTypes(type).SelectMany(nextType => GetRegularMethodsInType(nextType, cancellationToken)).ToArray();
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateSelfWithBaseTypes(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? nextType = type; nextType != null; nextType = nextType.BaseType)
        {
            yield return nextType;
        }
    }

    private static IEnumerable<IMethodSymbol> GetRegularMethodsInType(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        return type.GetMembers().OfType<IMethodSymbol>().Where(method => IsRegularMethod(method, cancellationToken)).ToArray();
    }

    private static bool IsRegularMethod(IMethodSymbol method, CancellationToken cancellationToken)
    {
        return RegularMethodKinds.Contains(method.MethodKind) && !method.IsSynthesized() && HasMethodBody(method, cancellationToken);
    }

    private static bool HasMethodBody(IMethodSymbol method, CancellationToken cancellationToken)
    {
        return method.TryGetBodySyntaxForMethod(cancellationToken) != null;
    }

    private static bool HasAtLeastTwoItems<T>(IEnumerable<T?> source)
    {
        return source.Skip(1).Any();
    }

    private static void AnalyzeMethodGroup(IReadOnlyCollection<IMethodSymbol> methodGroup, INamedTypeSymbol activeType,
        SymbolAnalysisContext context)
    {
        IMethodSymbol? longestOverload = TryGetSingleLongestOverload(methodGroup);

        if (longestOverload != null)
        {
            if (longestOverload.ContainingType.IsEqualTo(activeType) && CanBeMadeVirtual(longestOverload))
            {
                IMethodSymbol methodToReport = longestOverload.PartialImplementationPart ?? longestOverload;

                var diagnostic = Diagnostic.Create(MakeVirtualRule, methodToReport.Locations[0]);
                context.ReportDiagnostic(diagnostic);
            }

            var info = new OverloadsInfo(methodGroup, longestOverload, context);

            AnalyzeOverloads(info, activeType);
        }
    }

    private static void AnalyzeOverloads(OverloadsInfo info, INamedTypeSymbol activeType)
    {
        IEnumerable<IMethodSymbol> overloadsInActiveType = info.MethodGroup.Where(method =>
            !method.IsEqualTo(info.LongestOverload) && method.ContainingType.IsEqualTo(activeType));

        foreach (IMethodSymbol overload in overloadsInActiveType)
        {
            AnalyzeOverload(info, overload);
        }
    }

    private static void AnalyzeOverload(OverloadsInfo info, IMethodSymbol overload)
    {
        if (!overload.IsOverride && !overload.IsInterfaceImplementation() && !overload.HidesBaseMember(info.Context.CancellationToken))
        {
            CompareOrderOfParameters(overload, info.LongestOverload, info.Context);
        }

        var invocationWalker = new MethodInvocationWalker(info.MethodGroup);

        if (!InvokesAnotherOverload(overload, invocationWalker, info.Context))
        {
            IMethodSymbol methodToReport = overload.PartialImplementationPart ?? overload;
            string name = methodToReport.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            var diagnostic = Diagnostic.Create(InvokeRule, methodToReport.Locations[0], name);
            info.Context.ReportDiagnostic(diagnostic);
        }
    }

    private static IMethodSymbol? TryGetSingleLongestOverload(IReadOnlyCollection<IMethodSymbol> methodGroup)
    {
        IGrouping<int, IMethodSymbol> overloadsWithHighestParameterCount =
            methodGroup.GroupBy(group => group.Parameters.Length).OrderByDescending(group => group.Key).First();

        return overloadsWithHighestParameterCount.Skip(1).FirstOrDefault() == null ? overloadsWithHighestParameterCount.First() : null;
    }

    private static bool CanBeMadeVirtual(IMethodSymbol method)
    {
        return !method.IsStatic && method.DeclaredAccessibility != Accessibility.Private && !method.ContainingType.IsSealed &&
            method.ContainingType.TypeKind != TypeKind.Struct && method is { IsVirtual: false, IsOverride: false } &&
            !method.ExplicitInterfaceImplementations.Any();
    }

    private static void CompareOrderOfParameters(IMethodSymbol method, IMethodSymbol longestOverload, SymbolAnalysisContext context)
    {
        List<IParameterSymbol> parametersInLongestOverload = longestOverload.Parameters.ToList();

        if (!AreParametersDeclaredInSameOrder(method, parametersInLongestOverload))
        {
            string name = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            var diagnostic = Diagnostic.Create(OrderRule, method.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool AreParametersDeclaredInSameOrder(IMethodSymbol method,
        List<IParameterSymbol> parametersInLongestOverload)
    {
        return AreRegularParametersDeclaredInSameOrder(method, parametersInLongestOverload) &&
            AreDefaultParametersDeclaredInSameOrder(method, parametersInLongestOverload);
    }

    private static bool AreRegularParametersDeclaredInSameOrder(IMethodSymbol method,
        List<IParameterSymbol> parametersInLongestOverload)
    {
        List<IParameterSymbol> regularParametersInMethod = method.Parameters.Where(IsRegularParameter).ToList();
        List<IParameterSymbol> regularParametersInLongestOverload = parametersInLongestOverload.Where(IsRegularParameter).ToList();

        return AreParametersDeclaredInSameOrder(regularParametersInMethod, regularParametersInLongestOverload);
    }

    private static bool IsRegularParameter(IParameterSymbol parameter)
    {
        return parameter is { HasExplicitDefaultValue: false, IsParams: false } && !IsCancellationToken(parameter.Type);
    }

    private static bool IsCancellationToken(ITypeSymbol type)
    {
        return type.ToDisplayString() == "System.Threading.CancellationToken";
    }

    private static bool AreDefaultParametersDeclaredInSameOrder(IMethodSymbol method,
        List<IParameterSymbol> parametersInLongestOverload)
    {
        List<IParameterSymbol> defaultParametersInMethod = method.Parameters.Where(IsParameterWithDefaultValue).ToList();
        List<IParameterSymbol> defaultParametersInLongestOverload = parametersInLongestOverload.Where(IsParameterWithDefaultValue).ToList();

        return AreParametersDeclaredInSameOrder(defaultParametersInMethod, defaultParametersInLongestOverload);
    }

    private static bool IsParameterWithDefaultValue(IParameterSymbol parameter)
    {
        return parameter is { HasExplicitDefaultValue: true, IsParams: false };
    }

    private static bool AreParametersDeclaredInSameOrder(IList<IParameterSymbol> parameters,
        List<IParameterSymbol> parametersInLongestOverload)
    {
        for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
        {
            string parameterName = parameters[parameterIndex].Name;
            int indexInLongestOverload = parametersInLongestOverload.FindIndex(parameter => parameter.Name == parameterName);

            if (indexInLongestOverload != -1 && indexInLongestOverload != parameterIndex)
            {
                return false;
            }
        }

        return true;
    }

    private static bool InvokesAnotherOverload(IMethodSymbol methodToAnalyze, MethodInvocationWalker invocationWalker,
        SymbolAnalysisContext context)
    {
        IOperation? operation = methodToAnalyze.TryGetOperationBlockForMethod(context.Compilation, context.CancellationToken);

        if (operation != null)
        {
            invocationWalker.AnalyzeBlock(operation, methodToAnalyze);
            return invocationWalker.HasFoundInvocation;
        }

        return false;
    }

    private sealed class MethodInvocationWalker(IReadOnlyCollection<IMethodSymbol> methodGroup) : ExplicitOperationWalker
    {
        private readonly IReadOnlyCollection<IMethodSymbol> methodGroup = methodGroup;

        private IMethodSymbol? containingMethod;

        public bool HasFoundInvocation { get; private set; }

        public void AnalyzeBlock(IOperation block, IMethodSymbol method)
        {
            ArgumentNullException.ThrowIfNull(block);
            ArgumentNullException.ThrowIfNull(method);

            containingMethod = method;
            HasFoundInvocation = false;

            Visit(block);
        }

        public override void VisitInvocation(IInvocationOperation operation)
        {
            if (HasFoundInvocation)
            {
                return;
            }

            foreach (IMethodSymbol methodToFind in methodGroup)
            {
                if (!methodToFind.IsEqualTo(containingMethod))
                {
                    VerifyInvocation(operation, methodToFind);
                }
            }

            if (!HasFoundInvocation)
            {
                base.VisitInvocation(operation);
            }
        }

        private void VerifyInvocation(IInvocationOperation operation, IMethodSymbol methodToFind)
        {
            if (methodToFind.MethodKind == MethodKind.ExplicitInterfaceImplementation)
            {
                ScanExplicitInterfaceInvocation(operation, methodToFind);
            }
            else
            {
                if (methodToFind.OriginalDefinition.IsEqualTo(operation.TargetMethod.OriginalDefinition))
                {
                    HasFoundInvocation = true;
                }
            }
        }

        private void ScanExplicitInterfaceInvocation(IInvocationOperation operation, IMethodSymbol methodToFind)
        {
            foreach (IMethodSymbol interfaceMethod in methodToFind.ExplicitInterfaceImplementations)
            {
                if (operation.TargetMethod.OriginalDefinition.IsEqualTo(interfaceMethod.OriginalDefinition))
                {
                    HasFoundInvocation = true;
                    break;
                }
            }
        }
    }

    private readonly struct OverloadsInfo(
        IReadOnlyCollection<IMethodSymbol> methodGroup, IMethodSymbol longestOverload, SymbolAnalysisContext context)
    {
        public IReadOnlyCollection<IMethodSymbol> MethodGroup { get; } = methodGroup;

        public IMethodSymbol LongestOverload { get; } = longestOverload;

        public SymbolAnalysisContext Context { get; } = context;
    }
}
