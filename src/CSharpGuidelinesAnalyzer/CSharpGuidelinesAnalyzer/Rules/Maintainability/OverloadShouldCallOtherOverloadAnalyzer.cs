using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OverloadShouldCallOtherOverloadAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1551";

        private const string Title = "Method overload should call another overload";
        private const string InvokeMessageFormat = "Overloaded method '{0}' should call another overload.";
        private const string MakeVirtualMessageFormat = "Method overload with the most parameters should be virtual.";

        private const string OrderMessageFormat =
            "Parameter order in '{0}' does not match with the parameter order of the longest overload.";

        private const string Description = "Call the more overloaded method from other overloads.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor InvokeRule = new DiagnosticDescriptor(DiagnosticId, Title,
            InvokeMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor MakeVirtualRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MakeVirtualMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor OrderRule = new DiagnosticDescriptor(DiagnosticId, Title, OrderMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(InvokeRule, MakeVirtualRule, OrderRule);

        private static readonly ImmutableArray<MethodKind> RegularMethodKinds = new[]
        {
            MethodKind.Ordinary,
            MethodKind.ExplicitInterfaceImplementation,
            MethodKind.ReducedExtension
        }.ToImmutableArray();

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeNamedTypeAction =
            context => context.SkipEmptyName(AnalyzeNamedType);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamedTypeAction, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            {
                return;
            }

            IGrouping<string, IMethodSymbol>[] methodGroups = GetRegularMethodsInTypeHierarchy(type, context.CancellationToken)
                .GroupBy(method => method.Name).Where(HasAtLeastTwoItems).ToArray();

            foreach (IGrouping<string, IMethodSymbol> methodGroup in methodGroups)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                AnalyzeMethodGroup(methodGroup.ToArray(), type, context);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<IMethodSymbol> GetRegularMethodsInTypeHierarchy([NotNull] INamedTypeSymbol type,
            CancellationToken cancellationToken)
        {
            return EnumerateSelfWithBaseTypes(type)
                .SelectMany(currentType1 => GetRegularMethodsInType(currentType1, cancellationToken)).ToArray();
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<INamedTypeSymbol> EnumerateSelfWithBaseTypes([NotNull] INamedTypeSymbol type)
        {
            for (INamedTypeSymbol nextType = type; nextType != null; nextType = nextType.BaseType)
            {
                yield return nextType;
            }
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<IMethodSymbol> GetRegularMethodsInType([NotNull] INamedTypeSymbol type,
            CancellationToken cancellationToken)
        {
            return type.GetMembers().OfType<IMethodSymbol>().Where(method => IsRegularMethod(method, cancellationToken))
                .ToArray();
        }

        private static bool IsRegularMethod([NotNull] IMethodSymbol method, CancellationToken cancellationToken)
        {
            return RegularMethodKinds.Contains(method.MethodKind) && !method.IsSynthesized() &&
                HasMethodBody(method, cancellationToken);
        }

        private static bool HasMethodBody([NotNull] IMethodSymbol method, CancellationToken cancellationToken)
        {
            return method.TryGetBodySyntaxForMethod(cancellationToken) != null;
        }

        private static bool HasAtLeastTwoItems<T>([NotNull] [ItemCanBeNull] IEnumerable<T> source)
        {
            return source.Skip(1).Any();
        }

        private static void AnalyzeMethodGroup([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup,
            [NotNull] INamedTypeSymbol activeType, SymbolAnalysisContext context)
        {
            IMethodSymbol longestOverload = TryGetSingleLongestOverload(methodGroup);
            if (longestOverload != null)
            {
                if (longestOverload.ContainingType.Equals(activeType) && CanBeMadeVirtual(longestOverload))
                {
                    IMethodSymbol methodToReport = longestOverload.PartialImplementationPart ?? longestOverload;
                    context.ReportDiagnostic(Diagnostic.Create(MakeVirtualRule, methodToReport.Locations[0]));
                }

                var info = new OverloadsInfo(methodGroup, longestOverload, context);

                AnalyzeOverloads(info, activeType);
            }
        }

        private static void AnalyzeOverloads(OverloadsInfo info, [NotNull] INamedTypeSymbol activeType)
        {
            IEnumerable<IMethodSymbol> overloadsInActiveType = info.MethodGroup.Where(method =>
                !method.Equals(info.LongestOverload) && method.ContainingType.Equals(activeType));

            foreach (IMethodSymbol overload in overloadsInActiveType)
            {
                AnalyzeOverload(info, overload);
            }
        }

        private static void AnalyzeOverload(OverloadsInfo info, [NotNull] IMethodSymbol overload)
        {
            if (!overload.IsOverride && !overload.IsInterfaceImplementation() &&
                !overload.HidesBaseMember(info.Context.CancellationToken))
            {
                CompareOrderOfParameters(overload, info.LongestOverload, info.Context);
            }

            var invocationWalker = new MethodInvocationWalker(info.MethodGroup);

            if (!InvokesAnotherOverload(overload, invocationWalker, info.Context))
            {
                IMethodSymbol methodToReport = overload.PartialImplementationPart ?? overload;

                info.Context.ReportDiagnostic(Diagnostic.Create(InvokeRule, methodToReport.Locations[0],
                    methodToReport.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        [CanBeNull]
        private static IMethodSymbol TryGetSingleLongestOverload(
            [NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup)
        {
            IGrouping<int, IMethodSymbol> overloadsWithHighestParameterCount =
                methodGroup.GroupBy(mg => mg.Parameters.Length).OrderByDescending(x => x.Key).First();
            return overloadsWithHighestParameterCount.Skip(1).FirstOrDefault() == null
                ? overloadsWithHighestParameterCount.First()
                : null;
        }

        private static bool CanBeMadeVirtual([NotNull] IMethodSymbol method)
        {
            return !method.IsStatic && method.DeclaredAccessibility != Accessibility.Private && !method.ContainingType.IsSealed &&
                method.ContainingType.TypeKind != TypeKind.Struct && !method.IsVirtual && !method.IsOverride &&
                !method.ExplicitInterfaceImplementations.Any();
        }

        private static void CompareOrderOfParameters([NotNull] IMethodSymbol method, [NotNull] IMethodSymbol longestOverload,
            SymbolAnalysisContext context)
        {
            List<IParameterSymbol> parametersInlongestOverload = longestOverload.Parameters.ToList();

            if (!AreParametersDeclaredInSameOrder(method, parametersInlongestOverload))
            {
                context.ReportDiagnostic(Diagnostic.Create(OrderRule, method.Locations[0],
                    method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        private static bool AreParametersDeclaredInSameOrder([NotNull] IMethodSymbol method,
            [NotNull] [ItemNotNull] List<IParameterSymbol> parametersInLongestOverload)
        {
            return AreRegularParametersDeclaredInSameOrder(method, parametersInLongestOverload) &&
                AreDefaultParametersDeclaredInSameOrder(method, parametersInLongestOverload);
        }

        private static bool AreRegularParametersDeclaredInSameOrder([NotNull] IMethodSymbol method,
            [NotNull] [ItemNotNull] List<IParameterSymbol> parametersInLongestOverload)
        {
            List<IParameterSymbol> regularParametersInMethod = method.Parameters.Where(IsRegularParameter).ToList();
            List<IParameterSymbol> regularParametersInlongestOverload =
                parametersInLongestOverload.Where(IsRegularParameter).ToList();

            return AreParametersDeclaredInSameOrder(regularParametersInMethod, regularParametersInlongestOverload);
        }

        private static bool IsRegularParameter([NotNull] IParameterSymbol parameter)
        {
            return !parameter.HasExplicitDefaultValue && !parameter.IsParams;
        }

        private static bool AreDefaultParametersDeclaredInSameOrder([NotNull] IMethodSymbol method,
            [NotNull] [ItemNotNull] List<IParameterSymbol> parametersInLongestOverload)
        {
            List<IParameterSymbol> defaultParametersInMethod = method.Parameters.Where(IsParameterWithDefaultValue).ToList();
            List<IParameterSymbol> defaultParametersInlongestOverload =
                parametersInLongestOverload.Where(IsParameterWithDefaultValue).ToList();

            return AreParametersDeclaredInSameOrder(defaultParametersInMethod, defaultParametersInlongestOverload);
        }

        private static bool IsParameterWithDefaultValue([NotNull] IParameterSymbol parameter)
        {
            return parameter.HasExplicitDefaultValue && !parameter.IsParams;
        }

        private static bool AreParametersDeclaredInSameOrder([NotNull] [ItemNotNull] IList<IParameterSymbol> parameters,
            [NotNull] [ItemNotNull] List<IParameterSymbol> parametersInLongestOverload)
        {
            for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
            {
                string parameterName = parameters[parameterIndex].Name;

                int indexInLongestOverload = parametersInLongestOverload.FindIndex(p => p.Name == parameterName);
                if (indexInLongestOverload != -1 && indexInLongestOverload != parameterIndex)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool InvokesAnotherOverload([NotNull] IMethodSymbol methodToAnalyze,
            [NotNull] MethodInvocationWalker invocationWalker, SymbolAnalysisContext context)
        {
            IOperation operation = methodToAnalyze.TryGetOperationBlockForMethod(context.Compilation, context.CancellationToken);
            if (operation != null)
            {
                invocationWalker.AnalyzeBlock(operation, methodToAnalyze);
                return invocationWalker.HasFoundInvocation;
            }

            return false;
        }

        private sealed class MethodInvocationWalker : ExplicitOperationWalker
        {
            [NotNull]
            [ItemNotNull]
            private readonly IReadOnlyCollection<IMethodSymbol> methodGroup;

            public bool HasFoundInvocation { get; private set; }

            [CanBeNull]
            private IMethodSymbol containingMethod;

            public MethodInvocationWalker([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup)
            {
                this.methodGroup = methodGroup;
            }

            public void AnalyzeBlock([NotNull] IOperation block, [NotNull] IMethodSymbol method)
            {
                Guard.NotNull(block, nameof(block));
                Guard.NotNull(method, nameof(method));

                containingMethod = method;
                HasFoundInvocation = false;

                Visit(block);
            }

            public override void VisitInvocation([NotNull] IInvocationOperation operation)
            {
                if (HasFoundInvocation)
                {
                    return;
                }

                foreach (IMethodSymbol methodToFind in methodGroup)
                {
                    if (!methodToFind.Equals(containingMethod))
                    {
                        VerifyInvocation(operation, methodToFind);
                    }
                }

                if (!HasFoundInvocation)
                {
                    base.VisitInvocation(operation);
                }
            }

            private void VerifyInvocation([NotNull] IInvocationOperation operation, [NotNull] IMethodSymbol methodToFind)
            {
                if (methodToFind.MethodKind == MethodKind.ExplicitInterfaceImplementation)
                {
                    ScanExplicitInterfaceInvocation(operation, methodToFind);
                }
                else
                {
                    if (methodToFind.OriginalDefinition.Equals(operation.TargetMethod.OriginalDefinition))
                    {
                        HasFoundInvocation = true;
                    }
                }
            }

            private void ScanExplicitInterfaceInvocation([NotNull] IInvocationOperation operation,
                [NotNull] IMethodSymbol methodToFind)
            {
                foreach (IMethodSymbol ifaceMethod in methodToFind.ExplicitInterfaceImplementations)
                {
                    if (operation.TargetMethod.OriginalDefinition.Equals(ifaceMethod.OriginalDefinition))
                    {
                        HasFoundInvocation = true;
                        break;
                    }
                }
            }
        }

        private struct OverloadsInfo
        {
            [NotNull]
            [ItemNotNull]
            public IReadOnlyCollection<IMethodSymbol> MethodGroup { get; }

            [NotNull]
            public IMethodSymbol LongestOverload { get; }

            public SymbolAnalysisContext Context { get; }

            public OverloadsInfo([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup,
                [NotNull] IMethodSymbol longestOverload, SymbolAnalysisContext context)
            {
                MethodGroup = methodGroup;
                LongestOverload = longestOverload;
                Context = context;
            }
        }
    }
}
