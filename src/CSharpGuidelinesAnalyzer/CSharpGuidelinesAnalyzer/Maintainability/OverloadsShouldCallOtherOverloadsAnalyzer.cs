using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OverloadsShouldCallOtherOverloadsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1551";

        private const string Title = "Overloaded method should call another overload";
        private const string InvokeMessageFormat = "Overloaded method '{0}' should call another overload.";
        private const string MakeVirtualMessageFormat = "Method overload with the most parameters should be virtual.";

        private const string OrderMessageFormat =
            "Parameter order in '{0}' does not match with the parameter order of the longest overload.";

        private const string Description = "Call the more overloaded method from other overloads.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor InvokeRule = new DiagnosticDescriptor(DiagnosticId, Title,
            InvokeMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor MakeVirtualRule = new DiagnosticDescriptor(DiagnosticId, Title,
            MakeVirtualMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor OrderRule = new DiagnosticDescriptor(DiagnosticId, Title,
            OrderMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(InvokeRule, MakeVirtualRule, OrderRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
                }
            });
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            {
                return;
            }

            IGrouping<string, IMethodSymbol>[] methodGroups =
                type.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.MethodKind != MethodKind.Constructor)
                    .Where(method => HasMethodBody(method, context.CancellationToken))
                    .GroupBy(method => method.Name)
                    .Where(HasAtLeastTwoItems)
                    .ToArray();

            foreach (IGrouping<string, IMethodSymbol> methodGroup in methodGroups)
            {
                AnalyzeMethodGroup(methodGroup.ToArray(), context);
            }
        }

        private static bool HasMethodBody([NotNull] IMethodSymbol method, CancellationToken cancellationToken)
        {
            return AnalysisUtilities.TryGetBodySyntaxForMethod(method, cancellationToken) != null;
        }

        private bool HasAtLeastTwoItems<T>([NotNull] [ItemCanBeNull] IEnumerable<T> source)
        {
            return source.Skip(1).Any();
        }

        private void AnalyzeMethodGroup([NotNull] [ItemNotNull] ICollection<IMethodSymbol> methodGroup,
            SymbolAnalysisContext context)
        {
            IMethodSymbol longestOverload = TryGetSingleLongestOverload(methodGroup);
            if (longestOverload != null)
            {
                if (CanBeMadeVirtual(longestOverload))
                {
                    IMethodSymbol methodToReport = longestOverload.PartialImplementationPart ?? longestOverload;

                    context.ReportDiagnostic(Diagnostic.Create(MakeVirtualRule, methodToReport.Locations[0]));
                }

                foreach (IMethodSymbol overload in methodGroup.Where(method => !method.Equals(longestOverload)))
                {
                    if (!overload.IsOverride && !AnalysisUtilities.IsInterfaceImplementation(overload) &&
                        !AnalysisUtilities.HidesBaseMember(overload, context.CancellationToken))
                    {
                        CompareParameterOrder(overload, longestOverload, context);
                    }

                    ImmutableArray<IMethodSymbol> otherOverloads =
                        methodGroup.Where(m => !m.Equals(overload)).ToImmutableArray();

                    if (!HasInvocationToAnyOf(otherOverloads, overload, context.Compilation, context.CancellationToken))
                    {
                        IMethodSymbol methodToReport = overload.PartialImplementationPart ?? overload;

                        context.ReportDiagnostic(Diagnostic.Create(InvokeRule, methodToReport.Locations[0],
                            methodToReport.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                    }
                }
            }
        }

        [CanBeNull]
        private IMethodSymbol TryGetSingleLongestOverload([NotNull] [ItemNotNull] ICollection<IMethodSymbol> methodGroup)
        {
            IGrouping<int, IMethodSymbol> overloadsWithHighestParameterCount =
                methodGroup.GroupBy(mg => mg.Parameters.Length).OrderByDescending(x => x.Key).First();
            return overloadsWithHighestParameterCount.Skip(1).FirstOrDefault() == null
                ? overloadsWithHighestParameterCount.First()
                : null;
        }

        private bool CanBeMadeVirtual([NotNull] IMethodSymbol method)
        {
            return !method.IsStatic && !method.ContainingType.IsSealed &&
                method.ContainingType.TypeKind != TypeKind.Struct && !method.IsVirtual && !method.IsOverride &&
                !method.ExplicitInterfaceImplementations.Any();
        }

        private void CompareParameterOrder([NotNull] IMethodSymbol method, [NotNull] IMethodSymbol longestOverload,
            SymbolAnalysisContext context)
        {
            bool hasMismatch = false;
            List<IParameterSymbol> parametersInlongestOverload = longestOverload.Parameters.ToList();

            for (int parameterIndex = 0; parameterIndex < method.Parameters.Length; parameterIndex++)
            {
                string parameterName = method.Parameters[parameterIndex].Name;

                int indexInLongestOverload = parametersInlongestOverload.FindIndex(p => p.Name == parameterName);
                if (indexInLongestOverload != -1 && indexInLongestOverload != parameterIndex)
                {
                    hasMismatch = true;
                }
            }

            if (hasMismatch)
            {
                context.ReportDiagnostic(Diagnostic.Create(OrderRule, method.Locations[0],
                    method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        private bool HasInvocationToAnyOf([ItemNotNull] ImmutableArray<IMethodSymbol> methodsToInvoke,
            [NotNull] IMethodSymbol methodToAnalyze, [NotNull] Compilation compilation,
            CancellationToken cancellationToken)
        {
            IOperation operation = AnalysisUtilities.TryGetOperationBlockForMethod(methodToAnalyze, compilation,
                cancellationToken);
            if (operation != null)
            {
                var walker = new MethodInvocationWalker(methodsToInvoke);
                walker.Visit(operation);
                return walker.HasFoundInvocation;
            }

            return false;
        }

        private sealed class MethodInvocationWalker : OperationWalker
        {
            [ItemNotNull]
            private readonly ImmutableArray<IMethodSymbol> methodsToFind;

            public bool HasFoundInvocation { get; private set; }

            public MethodInvocationWalker([ItemNotNull] ImmutableArray<IMethodSymbol> methodsToFind)
            {
                this.methodsToFind = methodsToFind;
            }

            public override void VisitInvocationExpression([NotNull] IInvocationExpression operation)
            {
                if (!HasFoundInvocation)
                {
                    foreach (IMethodSymbol methodToFind in methodsToFind)
                    {
                        if (methodToFind.MethodKind == MethodKind.ExplicitInterfaceImplementation)
                        {
                            ScanExplicitInterfaceInvocation(operation, methodToFind);
                        }
                        else
                        {
                            if (methodsToFind.Any(method => method.Equals(operation.TargetMethod)))
                            {
                                HasFoundInvocation = true;
                            }
                        }
                    }

                    if (!HasFoundInvocation)
                    {
                        base.VisitInvocationExpression(operation);
                    }
                }
            }

            private void ScanExplicitInterfaceInvocation([NotNull] IInvocationExpression operation,
                [NotNull] IMethodSymbol methodToFind)
            {
                foreach (IMethodSymbol ifaceMethod in methodToFind.ExplicitInterfaceImplementations)
                {
                    if (operation.TargetMethod.Equals(ifaceMethod))
                    {
                        HasFoundInvocation = true;
                        break;
                    }
                }
            }
        }
    }
}