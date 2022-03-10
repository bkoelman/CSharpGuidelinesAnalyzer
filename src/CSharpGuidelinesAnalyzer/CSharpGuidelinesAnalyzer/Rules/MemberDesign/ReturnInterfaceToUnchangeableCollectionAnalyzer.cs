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
            var unchangeableCollectionInterfaces = new HashSet<INamedTypeSymbol>();

            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIEnumerableT(compilation), unchangeableCollectionInterfaces);
            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIAsyncEnumerableT(compilation), unchangeableCollectionInterfaces);
            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIReadOnlyCollectionT(compilation), unchangeableCollectionInterfaces);
            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIReadOnlyListT(compilation), unchangeableCollectionInterfaces);
            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIReadOnlySetT(compilation), unchangeableCollectionInterfaces);
            IncludeIfNotNull(KnownTypes.SystemCollectionsGenericIReadOnlyDictionaryTKeyTValue(compilation), unchangeableCollectionInterfaces);

            return unchangeableCollectionInterfaces;
        }

        private static void IncludeIfNotNull([CanBeNull] INamedTypeSymbol typeToInclude, [NotNull] [ItemNotNull] ISet<INamedTypeSymbol> types)
        {
            if (typeToInclude != null)
            {
                types.Add(typeToInclude);
            }
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
                    context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0],
                        method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
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
            if (!TypeImplementsIEnumerable(type))
            {
                return false;
            }

            if (type is INamedTypeSymbol { IsGenericType: true } namedType)
            {
                return !unchangeableCollectionInterfaces.Contains(namedType.ConstructedFrom);
            }

            return true;
        }

        private static bool TypeImplementsIEnumerable([NotNull] ITypeSymbol type)
        {
            return type.AllInterfaces.Any(IsIEnumerable);
        }

        private static bool IsIEnumerable([NotNull] INamedTypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Collections_IEnumerable;
        }
    }
}
