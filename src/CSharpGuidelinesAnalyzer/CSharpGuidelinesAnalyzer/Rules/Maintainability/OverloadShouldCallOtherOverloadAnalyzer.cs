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

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                startContext.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            });
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
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
            INamedTypeSymbol currentType = type;
            do
            {
                foreach (IMethodSymbol method in GetRegularMethodsInType(currentType, cancellationToken))
                {
                    yield return method;
                }

                currentType = currentType.BaseType;
            }
            while (currentType != null);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<IMethodSymbol> GetRegularMethodsInType([NotNull] INamedTypeSymbol type,
            CancellationToken cancellationToken)
        {
            return type.GetMembers().OfType<IMethodSymbol>().Where(method => IsRegularMethod(method, cancellationToken));
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

        private bool HasAtLeastTwoItems<T>([NotNull] [ItemCanBeNull] IEnumerable<T> source)
        {
            return source.Skip(1).Any();
        }

        private void AnalyzeMethodGroup([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup,
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

                AnalyzeOverloads(methodGroup, longestOverload, activeType, context);
            }
        }

        private void AnalyzeOverloads([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup,
            [NotNull] IMethodSymbol longestOverload, [NotNull] INamedTypeSymbol activeType, SymbolAnalysisContext context)
        {
            IEnumerable<IMethodSymbol> overloadsInActiveType = methodGroup.Where(method =>
                !method.Equals(longestOverload) && method.ContainingType.Equals(activeType));

            foreach (IMethodSymbol overload in overloadsInActiveType)
            {
                AnalyzeOverload(overload, methodGroup, longestOverload, context);
            }
        }

        private void AnalyzeOverload([NotNull] IMethodSymbol overload,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup, [NotNull] IMethodSymbol longestOverload,
            SymbolAnalysisContext context)
        {
            if (!overload.IsOverride && !overload.IsInterfaceImplementation() &&
                !overload.HidesBaseMember(context.CancellationToken))
            {
                CompareOrderOfParameters(overload, longestOverload, context);
            }

            var invocationWalker = new MethodInvocationWalker(methodGroup);

            if (!InvokesAnotherOverload(overload, invocationWalker, context))
            {
                IMethodSymbol methodToReport = overload.PartialImplementationPart ?? overload;

                context.ReportDiagnostic(Diagnostic.Create(InvokeRule, methodToReport.Locations[0],
                    methodToReport.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        [CanBeNull]
        private IMethodSymbol TryGetSingleLongestOverload([NotNull] [ItemNotNull] IReadOnlyCollection<IMethodSymbol> methodGroup)
        {
            IGrouping<int, IMethodSymbol> overloadsWithHighestParameterCount =
                methodGroup.GroupBy(mg => mg.Parameters.Length).OrderByDescending(x => x.Key).First();
            return overloadsWithHighestParameterCount.Skip(1).FirstOrDefault() == null
                ? overloadsWithHighestParameterCount.First()
                : null;
        }

        private bool CanBeMadeVirtual([NotNull] IMethodSymbol method)
        {
            return !method.IsStatic && method.DeclaredAccessibility != Accessibility.Private && !method.ContainingType.IsSealed &&
                method.ContainingType.TypeKind != TypeKind.Struct && !method.IsVirtual && !method.IsOverride &&
                !method.ExplicitInterfaceImplementations.Any();
        }

        private void CompareOrderOfParameters([NotNull] IMethodSymbol method, [NotNull] IMethodSymbol longestOverload,
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
            List<IParameterSymbol> regularParametersInMethod = method.Parameters.Where(IsRegularParameter).ToList();
            List<IParameterSymbol> regularParametersInlongestOverload =
                parametersInLongestOverload.Where(IsRegularParameter).ToList();

            if (AreParametersDeclaredInSameOrder(regularParametersInMethod, regularParametersInlongestOverload))
            {
                List<IParameterSymbol> defaultParametersInMethod = method.Parameters.Where(IsParameterWithDefaultValue).ToList();
                List<IParameterSymbol> defaultParametersInlongestOverload =
                    parametersInLongestOverload.Where(IsParameterWithDefaultValue).ToList();

                if (AreParametersDeclaredInSameOrder(defaultParametersInMethod, defaultParametersInlongestOverload))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRegularParameter([NotNull] IParameterSymbol parameter)
        {
            return !parameter.HasExplicitDefaultValue && !parameter.IsParams;
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

        private bool InvokesAnotherOverload([NotNull] IMethodSymbol methodToAnalyze,
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

        private sealed class MethodInvocationWalker : OperationWalker
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

            public void AnalyzeBlock([NotNull] IOperation block, [NotNull] IMethodSymbol containingMethod)
            {
                Guard.NotNull(block, nameof(block));
                Guard.NotNull(containingMethod, nameof(containingMethod));

                this.containingMethod = containingMethod;
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
                    if (methodToFind.Equals(operation.TargetMethod.OriginalDefinition))
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
    }
}
