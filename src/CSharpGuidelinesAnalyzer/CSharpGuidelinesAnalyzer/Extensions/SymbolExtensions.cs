using System.Collections.Generic;
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

            SyntaxNode syntax = member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken);

            var method = syntax as MethodDeclarationSyntax;
            var propertyEventIndexer = syntax as BasePropertyDeclarationSyntax;

            EventFieldDeclarationSyntax eventField =
                member is IEventSymbol ? syntax.FirstAncestorOrSelf<EventFieldDeclarationSyntax>() : null;

            return ContainsNewModifier(method?.Modifiers ?? propertyEventIndexer?.Modifiers ?? eventField?.Modifiers);
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

#pragma warning disable AV1532 // Loop statement contains nested loop
            foreach (INamedTypeSymbol iface in parameter.ContainingType.AllInterfaces)
            {
                foreach (ISymbol ifaceMember in iface.GetMembers())
                {
                    ISymbol implementer = parameter.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                    if (parameter.ContainingSymbol.Equals(implementer))
                    {
                        return true;
                    }
                }
            }
#pragma warning restore AV1532 // Loop statement contains nested loop

            return false;
        }

        public static bool IsInterfaceImplementation<TSymbol>([NotNull] this TSymbol member)
            where TSymbol : ISymbol
        {
            if (!(member is IFieldSymbol))
            {
#pragma warning disable AV1532 // Loop statement contains nested loop
                foreach (INamedTypeSymbol iface in member.ContainingType.AllInterfaces)
                {
                    foreach (TSymbol ifaceMember in iface.GetMembers().OfType<TSymbol>())
                    {
                        ISymbol implementer = member.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                        if (member.Equals(implementer))
                        {
                            return true;
                        }
                    }
                }
#pragma warning restore AV1532 // Loop statement contains nested loop
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

            SyntaxNode[] syntaxNodes = method.DeclaringSyntaxReferences
                .Select(syntaxReference => syntaxReference.GetSyntax(cancellationToken)).ToArray();

            return TryGetBodyForMethodSyntaxNodes(method, syntaxNodes, cancellationToken) ??
                TryGetBodyForConstructorSyntaxNodes(syntaxNodes);
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForMethodSyntaxNodes([NotNull] IMethodSymbol method,
            [NotNull] [ItemNotNull] IEnumerable<SyntaxNode> syntaxNodes, CancellationToken cancellationToken)
        {
            foreach (SyntaxNode syntaxNode in syntaxNodes)
            {
                SyntaxNode bodySyntax = TryGetBodyForMethodSyntax(syntaxNode, method, cancellationToken) ??
                    TryGetBodyForAnonymousFunctionSyntax(syntaxNode);

                if (bodySyntax != null)
                {
                    return bodySyntax;
                }
            }

            return null;
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForMethodSyntax([CanBeNull] SyntaxNode syntaxNode, [NotNull] IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            if (syntaxNode is MethodDeclarationSyntax methodSyntax)
            {
                return TryGetBodyForMethodBlockOrArrowExpressionSyntax(methodSyntax) ??
                    TryGetBodyForPartialMethodSyntax(method, cancellationToken);
            }

            return null;
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForMethodBlockOrArrowExpressionSyntax([NotNull] MethodDeclarationSyntax methodSyntax)
        {
            return (SyntaxNode)methodSyntax.Body ?? methodSyntax.ExpressionBody?.Expression;
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForPartialMethodSyntax([NotNull] IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            return method.PartialImplementationPart != null
                ? TryGetBodySyntaxForMethod(method.PartialImplementationPart, cancellationToken)
                : null;
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForAnonymousFunctionSyntax([NotNull] SyntaxNode syntaxNode)
        {
            var lambdaSyntax = syntaxNode as AnonymousFunctionExpressionSyntax;
            return lambdaSyntax?.Body;
        }

        [CanBeNull]
        private static SyntaxNode TryGetBodyForConstructorSyntaxNodes([NotNull] [ItemNotNull] IEnumerable<SyntaxNode> syntaxNodes)
        {
            foreach (ConstructorDeclarationSyntax constructorSyntax in syntaxNodes.OfType<ConstructorDeclarationSyntax>())
            {
                if (constructorSyntax.Body != null)
                {
                    return constructorSyntax.Body;
                }
            }

            return null;
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
    }
}
