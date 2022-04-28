using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnInterfaceToUnchangeableCollectionAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Return type in method signature should be an interface to an unchangeable collection";
        private const string MessageFormat = "Return type in signature for '{0}' should be an interface to an unchangeable collection";
        private const string Description = "Return interfaces to unchangeable collections.";

        private const string DependencyInjectionServiceCollectionTypeName = "Microsoft.Extensions.DependencyInjection.IServiceCollection";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1130";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        [NotNull]
        private static readonly Action<SymbolAnalysisContext, ISet<INamedTypeSymbol>> AnalyzeMethodAction = (context, unchangeableCollectionInterfaces) =>
            context.SkipEmptyName(_ => AnalyzeMethod(context, unchangeableCollectionInterfaces));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            ISet<INamedTypeSymbol> unchangeableCollectionInterfaces = ResolveUnchangeableCollectionInterfaces(startContext.Compilation);

            if (unchangeableCollectionInterfaces.Any())
            {
                startContext.RegisterSymbolAction(context => AnalyzeMethodAction(context, unchangeableCollectionInterfaces), SymbolKind.Method);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static ISet<INamedTypeSymbol> ResolveUnchangeableCollectionInterfaces([NotNull] Compilation compilation)
        {
            INamedTypeSymbol[] types =
            {
                KnownTypes.SystemCollectionsGenericIEnumerableT(compilation),
                KnownTypes.SystemCollectionsGenericIAsyncEnumerableT(compilation),
                KnownTypes.SystemLinqIQueryable(compilation),
                KnownTypes.SystemLinqIQueryableT(compilation),
                KnownTypes.SystemCollectionsGenericIReadOnlyCollectionT(compilation),
                KnownTypes.SystemCollectionsGenericIReadOnlyListT(compilation),
                KnownTypes.SystemCollectionsGenericIReadOnlySetT(compilation),
                KnownTypes.SystemCollectionsGenericIReadOnlyDictionaryTKeyTValue(compilation)
            };

            return types.Where(type => type != null).ToImmutableHashSet();
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, [NotNull] [ItemNotNull] ISet<INamedTypeSymbol> unchangeableCollectionInterfaces)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (method.ReturnsVoid || IsString(method.ReturnType) || IsImmutable(method.ReturnType) || method.IsSynthesized() || !IsMethodAccessible(method))
            {
                return;
            }

            if (IsArray(method.ReturnType) || IsChangeableCollection(method.ReturnType, unchangeableCollectionInterfaces))
            {
                if (!method.IsPropertyOrEventAccessor() && !method.IsOverride && !method.IsInterfaceImplementation() &&
                    !method.HidesBaseMember(context.CancellationToken))
                {
                    if (!IsWhitelisted(method))
                    {
                        string name = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                        var diagnostic = Diagnostic.Create(Rule, method.Locations[0], name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsString([NotNull] ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_String;
        }

        private static bool IsImmutable([NotNull] ITypeSymbol type)
        {
            return type.Name.StartsWith("Immutable", StringComparison.Ordinal) || type.Name.StartsWith("IImmutable", StringComparison.Ordinal);
        }

        private static bool IsMethodAccessible([NotNull] IMethodSymbol method)
        {
            return method.DeclaredAccessibility != Accessibility.Private && method.IsSymbolAccessibleFromRoot();
        }

        private static bool IsArray([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Array;
        }

        private static bool IsChangeableCollection([NotNull] ITypeSymbol type, [NotNull] [ItemNotNull] ISet<INamedTypeSymbol> unchangeableCollectionInterfaces)
        {
            if (!type.ImplementsIEnumerable())
            {
                return false;
            }

            return type is INamedTypeSymbol { IsGenericType: true } genericType
                ? !unchangeableCollectionInterfaces.Contains(genericType.ConstructedFrom)
                : !unchangeableCollectionInterfaces.Contains(type);
        }

        private static bool IsWhitelisted([NotNull] IMethodSymbol method)
        {
            return IsDependencyInjectionRegistrationMethod(method);
        }

        private static bool IsDependencyInjectionRegistrationMethod([NotNull] IMethodSymbol method)
        {
            return method.Name.StartsWith("Add", StringComparison.Ordinal) && method.IsExtensionMethod &&
                method.ReturnType.ToString() == DependencyInjectionServiceCollectionTypeName && method.Parameters.Length >= 1 &&
                method.Parameters[0].Type.ToString() == DependencyInjectionServiceCollectionTypeName;
        }
    }
}
