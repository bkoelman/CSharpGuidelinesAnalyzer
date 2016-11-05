using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer
{
    public static class AnalysisUtilities
    {
        [ItemNotNull]
        private static readonly ImmutableArray<string> UnitTestFrameworkMethodAttributeNames =
            new[]
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
                "Xunit.FactAttribute",
                "NUnit.Framework.TestAttribute",
                "MbUnit.Framework.TestAttribute"
            }.ToImmutableArray();

        public static bool SupportsOperations([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            IReadOnlyDictionary<string, string> features = compilation.SyntaxTrees.FirstOrDefault()?.Options.Features;
            return features != null && features.ContainsKey("IOperation") && features["IOperation"] == "true";
        }

        public static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext)
        {
            ISymbol symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
            return SyntaxToSymbolContext(syntaxContext, symbol);
        }

        private static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext,
            [NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            return new SymbolAnalysisContext(symbol, syntaxContext.SemanticModel.Compilation, syntaxContext.Options,
                syntaxContext.ReportDiagnostic, x => true, syntaxContext.CancellationToken);
        }

        public static bool IsNullableBoolean([NotNull] ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                var namedTypeSymbol = type as INamedTypeSymbol;

                if (namedTypeSymbol?.TypeArguments[0].SpecialType == SpecialType.System_Boolean)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNullableEnum([NotNull] ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                var namedTypeSymbol = type as INamedTypeSymbol;
                ITypeSymbol innerType = namedTypeSymbol?.TypeArguments[0];

                if (innerType?.BaseType != null && innerType.BaseType.SpecialType == SpecialType.System_Enum)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWithAnyWordOf([NotNull] string identiferName,
            [ItemNotNull] ImmutableArray<string> wordsToFind, bool allowLowerCaseMatch)
        {
            Guard.NotNullNorWhiteSpace(identiferName, nameof(identiferName));

            IList<string> wordsInText = ExtractWords(identiferName);
            string firstWordInText = wordsInText.First();

            if (wordsToFind.Contains(firstWordInText))
            {
                return true;
            }

            if (allowLowerCaseMatch)
            {
                ImmutableArray<string> lowerCaseWordsToFind =
                    wordsToFind.Select(w => w.ToLowerInvariant()).ToImmutableArray();
                if (lowerCaseWordsToFind.Contains(firstWordInText.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        [CanBeNull]
        public static string GetFirstWordInSetFromIdentifier([NotNull] string identiferName,
            [ItemNotNull] ImmutableArray<string> wordsToFind, bool allowLowerCaseMatch)
        {
            Guard.NotNullNorWhiteSpace(identiferName, nameof(identiferName));

            IList<string> wordsInText = ExtractWords(identiferName);

            ImmutableArray<string> lowerCaseWordsInText = allowLowerCaseMatch
                ? wordsInText.Select(w => w.ToLowerInvariant()).ToImmutableArray()
                : ImmutableArray<string>.Empty;

            foreach (string wordToFind in wordsToFind)
            {
                if (wordsInText.Contains(wordToFind))
                {
                    return wordToFind;
                }

                if (allowLowerCaseMatch)
                {
                    if (lowerCaseWordsInText.Contains(wordToFind.ToLowerInvariant()))
                    {
                        return wordToFind;
                    }
                }
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        private static IList<string> ExtractWords([NotNull] string identifierName)
        {
            var words = new List<string>();

            var builder = new StringBuilder();
            foreach (char ch in identifierName)
            {
                if (char.IsUpper(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsDigit(ch) ||
                    char.IsSymbol(ch))
                {
                    FlushBuilder(words, builder);

                    if (!char.IsUpper(ch))
                    {
                        continue;
                    }
                }

                builder.Append(ch);
            }

            FlushBuilder(words, builder);

            return words;
        }

        private static void FlushBuilder([NotNull] [ItemNotNull] List<string> words, [NotNull] StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                words.Add(builder.ToString());
                builder.Clear();
            }
        }

        public static bool HidesBaseMember([NotNull] ISymbol member, CancellationToken cancellationToken)
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

        private static bool ContainsNewModifier([CanBeNull] SyntaxTokenList? modifiers)
        {
            return modifiers != null && modifiers.Value.Any(m => m.Kind() == SyntaxKind.NewKeyword);
        }

        [NotNull]
        public static ISymbol GetContainingMember([NotNull] ISymbol owningSymbol)
        {
            Guard.NotNull(owningSymbol, nameof(owningSymbol));

            return IsPropertyOrEventAccessor(owningSymbol)
                ? ((IMethodSymbol) owningSymbol).AssociatedSymbol
                : owningSymbol;
        }

        public static bool IsPropertyOrEventAccessor([CanBeNull] ISymbol symbol)
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

        [CanBeNull]
        public static IdentifierInfo TryGetIdentifierInfo([CanBeNull] IOperation identifier)
        {
            var local = identifier as ILocalReferenceExpression;
            if (local != null)
            {
                return new IdentifierInfo(local.Local.Name,
                    local.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), local.Local.Type,
                    "Variable");
            }

            var parameter = identifier as IParameterReferenceExpression;
            if (parameter != null)
            {
                return new IdentifierInfo(parameter.Parameter.Name,
                    /* CSharpShortErrorMessageFormat returns 'ref int', ie. without parameter name */
                    parameter.Parameter.Name, parameter.Parameter.Type, parameter.Parameter.Kind.ToString());
            }

            var field = identifier as IFieldReferenceExpression;
            if (field != null)
            {
                return new IdentifierInfo(field.Field.Name,
                    field.Field.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), field.Field.Type,
                    field.Field.Kind.ToString());
            }

            var property = identifier as IPropertyReferenceExpression;
            if (property != null)
            {
                return new IdentifierInfo(property.Property.Name,
                    property.Property.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    property.Property.Type, property.Property.Kind.ToString());
            }

            var method = identifier as IInvocationExpression;
            if (method != null)
            {
                return new IdentifierInfo(method.TargetMethod.Name,
                    method.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    method.TargetMethod.ReturnType, method.TargetMethod.Kind.ToString());
            }

            return null;
        }

        public static bool IsInterfaceImplementation([NotNull] IParameterSymbol parameter)
        {
            Guard.NotNull(parameter, nameof(parameter));

            ISymbol containingMember = parameter.ContainingSymbol;

#pragma warning disable AV1532 // Loop statement contains nested loop
            foreach (INamedTypeSymbol iface in parameter.ContainingType.AllInterfaces)
            {
                foreach (ISymbol ifaceMember in iface.GetMembers())
                {
                    ISymbol implementer = parameter.ContainingType.FindImplementationForInterfaceMember(ifaceMember);

                    if (containingMember.Equals(implementer))
                    {
                        return true;
                    }
                }
            }
#pragma warning restore AV1532 // Loop statement contains nested loop

            return false;
        }

        public static bool IsInterfaceImplementation<TSymbol>([NotNull] TSymbol member) where TSymbol : ISymbol
        {
            if (member is IFieldSymbol)
            {
                return false;
            }

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

            return false;
        }

        [CanBeNull]
        public static IOperation TryGetOperationBlockForMethod([NotNull] IMethodSymbol method,
            [NotNull] Compilation compilation, CancellationToken cancellationToken)
        {
            SyntaxNode bodySyntax = TryGetBodySyntaxForMethod(method, cancellationToken);
            if (bodySyntax != null)
            {
                SemanticModel model = compilation.GetSemanticModel(bodySyntax.SyntaxTree);
                return model.GetOperation(bodySyntax);
            }

            return null;
        }

        [CanBeNull]
        public static SyntaxNode TryGetBodySyntaxForMethod([NotNull] IMethodSymbol method,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(method, nameof(method));

            SyntaxNode[] methodSyntaxNodes =
                method.DeclaringSyntaxReferences.Select(syntaxReference => syntaxReference.GetSyntax(cancellationToken))
                    .ToArray();
            foreach (MethodDeclarationSyntax methodSyntax in methodSyntaxNodes.OfType<MethodDeclarationSyntax>())
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

            foreach (ConstructorDeclarationSyntax constructorSyntax in
                methodSyntaxNodes.OfType<ConstructorDeclarationSyntax>())
            {
                if (constructorSyntax.Body != null)
                {
                    return constructorSyntax.Body;
                }
            }

            return null;
        }

        public static bool IsUnitTestMethod([CanBeNull] ISymbol symbol)
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