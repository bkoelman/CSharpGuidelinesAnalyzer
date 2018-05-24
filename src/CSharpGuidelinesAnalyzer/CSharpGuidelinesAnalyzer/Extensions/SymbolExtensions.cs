using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class SymbolExtensions
    {
        [ItemNotNull]
        private static readonly ImmutableArray<string> UnitTestFrameworkMethodAttributeNames =
            ImmutableArray.Create("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute", "Xunit.FactAttribute",
                "NUnit.Framework.TestAttribute", "MbUnit.Framework.TestAttribute");

        public static bool HidesBaseMember([NotNull] this ISymbol member, CancellationToken cancellationToken)
        {
            Guard.NotNull(member, nameof(member));

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

        [CanBeNull]
        private static SyntaxTokenList? TryGetModifiers([CanBeNull] SyntaxNode syntax)
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
                case VariableDeclaratorSyntax _:
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

        public static bool AreDocumentationCommentsReported([NotNull] this ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            SyntaxReference reference = symbol.DeclaringSyntaxReferences.First();
            return reference.SyntaxTree.Options.DocumentationMode == DocumentationMode.Diagnose;
        }

        private static bool ContainsNewModifier([CanBeNull] SyntaxTokenList? modifiers)
        {
            return modifiers != null && modifiers.Value.Any(m => m.IsKind(SyntaxKind.NewKeyword));
        }

        [NotNull]
        public static ISymbol GetContainingMember([NotNull] this ISymbol owningSymbol)
        {
            Guard.NotNull(owningSymbol, nameof(owningSymbol));

            return IsPropertyOrEventAccessor(owningSymbol) ? ((IMethodSymbol)owningSymbol).AssociatedSymbol : owningSymbol;
        }

        public static bool IsPropertyOrEventAccessor([CanBeNull] this ISymbol symbol)
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

        public static bool IsInterfaceImplementation([NotNull] this IParameterSymbol parameter)
        {
            Guard.NotNull(parameter, nameof(parameter));

            foreach (ISymbol ifaceMember in parameter.ContainingType.AllInterfaces.SelectMany(iface => iface.GetMembers()))
            {
                ISymbol implementer = parameter.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                if (parameter.ContainingSymbol.Equals(implementer))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsInterfaceImplementation<TSymbol>([NotNull] this TSymbol member)
            where TSymbol : ISymbol
        {
            if (!(member is IFieldSymbol))
            {
                foreach (TSymbol ifaceMember in member.ContainingType.AllInterfaces.SelectMany(iface =>
                    iface.GetMembers().OfType<TSymbol>()))
                {
                    ISymbol implementer = member.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                    if (member.Equals(implementer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [CanBeNull]
        public static IOperation TryGetOperationBlockForMethod([NotNull] this IMethodSymbol method,
            [NotNull] Compilation compilation, CancellationToken cancellationToken)
        {
            SyntaxNode bodySyntax = TryGetBodySyntaxForMethod(method, cancellationToken);
            if (bodySyntax != null)
            {
                SemanticModel model = compilation.GetSemanticModel(bodySyntax.SyntaxTree);
                IOperation operation = model.GetOperation(bodySyntax);

                if (!operation.HasErrors(compilation, cancellationToken))
                {
                    return operation;
                }
            }

            return null;
        }

        [CanBeNull]
        public static SyntaxNode TryGetBodySyntaxForMethod([NotNull] this IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(method, nameof(method));

            foreach (SyntaxNode syntaxNode in method.DeclaringSyntaxReferences
                .Select(syntaxReference => syntaxReference.GetSyntax(cancellationToken)).ToArray())
            {
                SyntaxNode bodySyntax = TryGetDeclarationBody(syntaxNode);
                if (bodySyntax != null)
                {
                    return bodySyntax;
                }
            }

            return TryGetBodyForPartialMethodSyntax(method, cancellationToken);
        }

        [CanBeNull]
        private static SyntaxNode TryGetDeclarationBody([NotNull] SyntaxNode syntaxNode)
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

        [CanBeNull]
        private static SyntaxNode TryGetBodyForPartialMethodSyntax([NotNull] IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            return method.PartialImplementationPart != null
                ? TryGetBodySyntaxForMethod(method.PartialImplementationPart, cancellationToken)
                : null;
        }

        public static bool IsUnitTestMethod([CanBeNull] this ISymbol symbol)
        {
            return symbol is IMethodSymbol method && HasUnitTestAttribute(method);
        }

        private static bool HasUnitTestAttribute([NotNull] IMethodSymbol method)
        {
            foreach (AttributeData attribute in method.GetAttributes())
            {
                if (UnitTestFrameworkMethodAttributeNames.Contains(attribute.AttributeClass.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSymbolAccessibleFromRoot([CanBeNull] this ISymbol symbol)
        {
            ISymbol container = symbol;
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

        public static bool IsDeconstructor([CanBeNull] this ISymbol symbol)
        {
            return symbol is IMethodSymbol method && method.Name == "Deconstruct";
        }

        [NotNull]
        public static string GetKind([NotNull] this ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            if (symbol.Kind == SymbolKind.Method && symbol is IMethodSymbol method)
            {
                if (method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.StaticConstructor)
                {
                    return "Constructor";
                }

                if (method.MethodKind == MethodKind.LocalFunction)
                {
                    return "Local function";
                }
            }

            return symbol.Kind.ToString();
        }

        [NotNull]
        public static ITypeSymbol GetMemberType([NotNull] this ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

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
                case IEventSymbol evnt:
                {
                    return evnt.Type;
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

        public static bool IsSynthesized([NotNull] this ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            return !symbol.Locations.Any();
        }
    }
}
