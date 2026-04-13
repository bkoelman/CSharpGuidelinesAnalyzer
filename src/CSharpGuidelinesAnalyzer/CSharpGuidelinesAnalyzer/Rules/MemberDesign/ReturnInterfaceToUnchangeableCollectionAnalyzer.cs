using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnInterfaceToUnchangeableCollectionAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Return type in method signature should be an interface to an unchangeable collection";
    private const string MessageFormat = "Return type in signature for '{0}' should be an interface to an unchangeable collection";
    private const string Description = "Return interfaces to unchangeable collections.";

    private const string DependencyInjectionServiceCollectionTypeName = "Microsoft.Extensions.DependencyInjection.IServiceCollection";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1130";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        ImmutableArray<ITypeSymbol> unchangeableCollectionInterfaces = ResolveUnchangeableCollectionInterfaces(startContext.Compilation);

        if (unchangeableCollectionInterfaces.Any())
        {
            startContext.SafeRegisterSymbolAction(context => AnalyzeMethod(context, unchangeableCollectionInterfaces), SymbolKind.Method);
        }
    }

    private static ImmutableArray<ITypeSymbol> ResolveUnchangeableCollectionInterfaces(Compilation compilation)
    {
        ITypeSymbol?[] types =
        [
            KnownTypes.SystemCollectionsGenericIEnumerableT(compilation),
            KnownTypes.SystemCollectionsGenericIAsyncEnumerableT(compilation),
            KnownTypes.SystemLinqIQueryable(compilation),
            KnownTypes.SystemLinqIQueryableT(compilation),
            KnownTypes.SystemCollectionsGenericIReadOnlyCollectionT(compilation),
            KnownTypes.SystemCollectionsGenericIReadOnlyListT(compilation),
            KnownTypes.SystemCollectionsGenericIReadOnlySetT(compilation),
            KnownTypes.SystemCollectionsGenericIReadOnlyDictionaryTKeyTValue(compilation)
        ];

        return types.Where(type => type != null).Cast<ITypeSymbol>().ToImmutableArray();
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, ImmutableArray<ITypeSymbol> unchangeableCollectionInterfaces)
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

    private static bool IsString(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_String;
    }

    private static bool IsImmutable(ITypeSymbol type)
    {
        return type.Name.StartsWith("Immutable", StringComparison.Ordinal) || type.Name.StartsWith("IImmutable", StringComparison.Ordinal);
    }

    private static bool IsMethodAccessible(IMethodSymbol method)
    {
        return method.DeclaredAccessibility != Accessibility.Private && method.IsSymbolAccessibleFromRoot();
    }

    private static bool IsArray(ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Array;
    }

    private static bool IsChangeableCollection(ITypeSymbol type, ImmutableArray<ITypeSymbol> unchangeableCollectionInterfaces)
    {
        if (!type.ImplementsIEnumerable())
        {
            return false;
        }

        return type is INamedTypeSymbol { IsGenericType: true } genericType
            ? !unchangeableCollectionInterfaces.Contains(genericType.ConstructedFrom, SymbolEqualityComparer.IncludeNullability)
            : !unchangeableCollectionInterfaces.Contains(type, SymbolEqualityComparer.IncludeNullability);
    }

    private static bool IsWhitelisted(IMethodSymbol method)
    {
        return IsDependencyInjectionRegistrationMethod(method);
    }

    private static bool IsDependencyInjectionRegistrationMethod(IMethodSymbol method)
    {
        return method.Name.StartsWith("Add", StringComparison.Ordinal) && method.IsExtensionMethod &&
            method.ReturnType.ToString() == DependencyInjectionServiceCollectionTypeName && method.Parameters.Length >= 1 &&
            method.Parameters[0].Type.ToString() == DependencyInjectionServiceCollectionTypeName;
    }
}
