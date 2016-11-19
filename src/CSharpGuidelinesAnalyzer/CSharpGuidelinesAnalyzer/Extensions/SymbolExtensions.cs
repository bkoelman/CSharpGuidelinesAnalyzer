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
            ImmutableArray.Create("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
                "Xunit.FactAttribute", "NUnit.Framework.TestAttribute", "MbUnit.Framework.TestAttribute");

        public static bool HidesBaseMember([NotNull] this ISymbol member, CancellationToken cancellationToken)
        {
            Guard.NotNull(member, nameof(member));

            SyntaxNode syntax = member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken);

            var method = syntax as MethodDeclarationSyntax;
            var propertyEventIndexer = syntax as BasePropertyDeclarationSyntax;

            EventFieldDeclarationSyntax eventField = member is IEventSymbol
                ? syntax.FirstAncestorOrSelf<EventFieldDeclarationSyntax>()
                : null;

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
            return modifiers != null && modifiers.Value.Any(m => m.Kind() == SyntaxKind.NewKeyword);
        }

        [NotNull]
        public static ISymbol GetContainingMember([NotNull] this ISymbol owningSymbol)
        {
            Guard.NotNull(owningSymbol, nameof(owningSymbol));

            return IsPropertyOrEventAccessor(owningSymbol)
                ? ((IMethodSymbol) owningSymbol).AssociatedSymbol
                : owningSymbol;
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

        public static bool IsInterfaceImplementation<TSymbol>([NotNull] this TSymbol member) where TSymbol : ISymbol
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

                if (!operation.IsInvalid)
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

            SyntaxNode[] syntaxNodes =
                method.DeclaringSyntaxReferences.Select(syntaxReference => syntaxReference.GetSyntax(cancellationToken))
                    .ToArray();

            foreach (SyntaxNode syntaxNode in syntaxNodes)
            {
                var methodSyntax = syntaxNode as MethodDeclarationSyntax;
                if (methodSyntax != null)
                {
                    if (methodSyntax.Body != null)
                    {
                        return methodSyntax.Body;
                    }

                    if (methodSyntax.ExpressionBody?.Expression != null)
                    {
                        return methodSyntax.ExpressionBody.Expression;
                    }

                    if (method.PartialImplementationPart != null)
                    {
                        return TryGetBodySyntaxForMethod(method.PartialImplementationPart, cancellationToken);
                    }
                }

                var lambdaSyntax = syntaxNode as AnonymousFunctionExpressionSyntax;
                if (lambdaSyntax?.Body != null)
                {
                    return lambdaSyntax.Body;
                }
            }

            foreach (ConstructorDeclarationSyntax constructorSyntax in
                syntaxNodes.OfType<ConstructorDeclarationSyntax>())
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
            var method = symbol as IMethodSymbol;
            return method != null && HasUnitTestAttribute(method);
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
    }
}