using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class SymbolExtensions
{
    private static readonly ImmutableArray<string> UnitTestFrameworkMethodAttributeNames = ImmutableArray.Create(
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute", "Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute",
        "Xunit.FactAttribute", "Xunit.TheoryAttribute", "NUnit.Framework.TestAttribute", "NUnit.Framework.TestCaseAttribute", "MbUnit.Framework.TestAttribute");

    private static readonly Lazy<IEqualityComparer<ISymbol?>> SymbolComparerLazy = new(() =>
    {
        Type comparerType = typeof(ISymbol).GetTypeInfo().Assembly.GetType("Microsoft.CodeAnalysis.SymbolEqualityComparer");
        FieldInfo? includeField = comparerType?.GetTypeInfo().GetDeclaredField("IncludeNullability");

        if (includeField != null && includeField.GetValue(null) is IEqualityComparer<ISymbol?> comparer)
        {
            return comparer;
        }

        return EqualityComparer<ISymbol?>.Default;
    });

    public static bool HidesBaseMember(this ISymbol member, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(member);

        foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
        {
            SyntaxNode syntax = reference.GetSyntax(cancellationToken);
            SyntaxTokenList? modifiers = TryGetModifiers(syntax);

            if (ContainsNewModifier(modifiers))
            {
                return true;
            }
        }

        return false;
    }

    private static SyntaxTokenList? TryGetModifiers(SyntaxNode? syntax)
    {
        switch (syntax)
        {
            case MethodDeclarationSyntax methodSyntax:
            {
                return methodSyntax.Modifiers;
            }
            case BasePropertyDeclarationSyntax propertyEventIndexerSyntax:
            {
                return propertyEventIndexerSyntax.Modifiers;
            }
            case VariableDeclaratorSyntax:
            {
                if (syntax.Parent.Parent is BaseFieldDeclarationSyntax eventFieldSyntax)
                {
                    return eventFieldSyntax.Modifiers;
                }

                break;
            }
            case BaseTypeDeclarationSyntax typeSyntax:
            {
                return typeSyntax.Modifiers;
            }
            case DelegateDeclarationSyntax delegateSyntax:
            {
                return delegateSyntax.Modifiers;
            }
        }

        return null;
    }

    public static bool AreDocumentationCommentsReported(this ISymbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        SyntaxReference reference = symbol.DeclaringSyntaxReferences.First();
        return reference.SyntaxTree.Options.DocumentationMode == DocumentationMode.Diagnose;
    }

    private static bool ContainsNewModifier(SyntaxTokenList? modifiers)
    {
        return modifiers != null && modifiers.Value.Any(modifier => modifier.IsKind(SyntaxKind.NewKeyword));
    }

    public static ISymbol GetContainingMember(this ISymbol owningSymbol)
    {
        ArgumentNullException.ThrowIfNull(owningSymbol);

        return IsPropertyOrEventAccessor(owningSymbol) ? ((IMethodSymbol)owningSymbol).AssociatedSymbol : owningSymbol;
    }

    public static bool IsPropertyOrEventAccessor(this ISymbol? symbol)
    {
        var method = symbol as IMethodSymbol;

        switch (method?.MethodKind)
        {
            case MethodKind.PropertyGet:
            case MethodKind.PropertySet:
            case MethodKind.EventAdd:
            case MethodKind.EventRemove:
            {
                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    public static bool IsInterfaceImplementation(this IParameterSymbol parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        foreach (ISymbol interfaceMember in parameter.ContainingType.AllInterfaces.SelectMany(@interface => @interface.GetMembers()))
        {
            ISymbol implementer = parameter.ContainingType.FindImplementationForInterfaceMember(interfaceMember);

            if (parameter.ContainingSymbol.IsEqualTo(implementer))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsInterfaceImplementation<TSymbol>(this TSymbol member)
        where TSymbol : ISymbol
    {
        if (member is not IFieldSymbol)
        {
            foreach (TSymbol interfaceMember in member.ContainingType.AllInterfaces.SelectMany(@interface => @interface.GetMembers().OfType<TSymbol>()))
            {
                ISymbol implementer = member.ContainingType.FindImplementationForInterfaceMember(interfaceMember);

                if (member.Equals(implementer))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static IOperation? TryGetOperationBlockForMethod(this IMethodSymbol method, Compilation compilation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? bodySyntax = TryGetBodySyntaxForMethod(method, cancellationToken);

        if (bodySyntax != null)
        {
            SemanticModel model = compilation.GetSemanticModel(bodySyntax.SyntaxTree);
            IOperation operation = model.GetOperation(bodySyntax);

            if (operation != null && !operation.HasErrors(compilation, cancellationToken))
            {
                return operation;
            }
        }

        return null;
    }

    public static SyntaxNode? TryGetBodySyntaxForMethod(this IMethodSymbol method, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(method);

        foreach (SyntaxNode syntaxNode in method.DeclaringSyntaxReferences.Select(syntaxReference => syntaxReference.GetSyntax(cancellationToken)).ToArray())
        {
            SyntaxNode? bodySyntax = TryGetDeclarationBody(syntaxNode);

            if (bodySyntax != null)
            {
                return bodySyntax;
            }
        }

        return TryGetBodyForPartialMethodSyntax(method, cancellationToken);
    }

    private static SyntaxNode? TryGetDeclarationBody(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case BaseMethodDeclarationSyntax methodSyntax:
            {
                return (SyntaxNode)methodSyntax.Body ?? methodSyntax.ExpressionBody?.Expression;
            }
            case AccessorDeclarationSyntax accessorSyntax:
            {
                return (SyntaxNode)accessorSyntax.Body ?? accessorSyntax.ExpressionBody?.Expression;
            }
            case PropertyDeclarationSyntax propertySyntax:
            {
                return propertySyntax.ExpressionBody?.Expression;
            }
            case IndexerDeclarationSyntax indexerSyntax:
            {
                return indexerSyntax.ExpressionBody?.Expression;
            }
            case AnonymousFunctionExpressionSyntax anonymousFunctionSyntax:
            {
                return anonymousFunctionSyntax.Body;
            }
            case LocalFunctionStatementSyntax localFunctionSyntax:
            {
                return (SyntaxNode)localFunctionSyntax.Body ?? localFunctionSyntax.ExpressionBody?.Expression;
            }
            default:
            {
                return null;
            }
        }
    }

    private static SyntaxNode? TryGetBodyForPartialMethodSyntax(IMethodSymbol method, CancellationToken cancellationToken)
    {
        return method.PartialImplementationPart != null ? TryGetBodySyntaxForMethod(method.PartialImplementationPart, cancellationToken) : null;
    }

    public static bool IsUnitTestMethod(this ISymbol? symbol)
    {
        return symbol is IMethodSymbol method && HasUnitTestAttribute(method);
    }

    private static bool HasUnitTestAttribute(IMethodSymbol method)
    {
        foreach (AttributeData attribute in method.GetAttributes())
        {
            string attributeClassName = attribute.AttributeClass.ToString();

            if (UnitTestFrameworkMethodAttributeNames.Contains(attributeClassName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsSymbolAccessibleFromRoot(this ISymbol? symbol)
    {
        ISymbol? container = symbol;

        while (container != null)
        {
            if (container.DeclaredAccessibility == Accessibility.Private)
            {
                return false;
            }

            container = container.ContainingType;
        }

        return true;
    }

    public static bool IsDeconstructor(this ISymbol? symbol)
    {
        return symbol is IMethodSymbol { Name: "Deconstruct" };
    }

    public static string GetKind(this ISymbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        if (symbol.Kind == SymbolKind.Local)
        {
            return "Variable";
        }

        if (symbol.Kind == SymbolKind.Method && symbol is IMethodSymbol method)
        {
            return GetMethodKind(method);
        }

        return symbol.Kind.ToString();
    }

    private static string GetMethodKind(IMethodSymbol method)
    {
        switch (method.MethodKind)
        {
            case MethodKind.PropertyGet:
            case MethodKind.PropertySet:
            {
                return "Property accessor";
            }
            case MethodKind.EventAdd:
            case MethodKind.EventRemove:
            {
                return "Event accessor";
            }
            case MethodKind.LocalFunction:
            {
                return "Local function";
            }
            default:
            {
                return method.Kind.ToString();
            }
        }
    }

    public static ITypeSymbol GetSymbolType(this ISymbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        switch (symbol)
        {
            case IFieldSymbol field:
            {
                return field.Type;
            }
            case IPropertySymbol property:
            {
                return property.Type;
            }
            case IEventSymbol @event:
            {
                return @event.Type;
            }
            case IMethodSymbol method:
            {
                return method.ReturnType;
            }
            case IParameterSymbol parameter:
            {
                return parameter.Type;
            }
            case ILocalSymbol local:
            {
                return local.Type;
            }
            default:
            {
                throw new InvalidOperationException($"Unexpected type '{symbol.GetType()}'.");
            }
        }
    }

    public static bool IsSynthesized(this ISymbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);

        return !symbol.Locations.Any();
    }

    public static string MemberNameWithoutExplicitInterfacePrefix(this ISymbol symbol)
    {
        int index = symbol.Name.LastIndexOf(".", StringComparison.Ordinal);
        return index != -1 ? symbol.Name.Substring(index + 1) : symbol.Name;
    }

    public static bool IsEntryPoint(this IMethodSymbol method, Compilation compilation, CancellationToken cancellationToken)
    {
        IMethodSymbol? entryPoint = method.MethodKind == MethodKind.Ordinary ? compilation.GetEntryPoint(cancellationToken) : null;

        return method.IsEqualTo(entryPoint);
    }

    public static bool IsEqualTo(this ISymbol? first, ISymbol? second)
    {
        return SymbolComparerLazy.Value.Equals(first, second);
    }
}
