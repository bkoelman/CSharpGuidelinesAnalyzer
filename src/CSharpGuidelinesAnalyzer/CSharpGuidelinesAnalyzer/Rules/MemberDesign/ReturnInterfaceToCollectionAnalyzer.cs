using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnInterfaceToCollectionAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Return type in method signature should be a collection interface instead of a concrete type";
        private const string MessageFormat = "Return type in signature for '{0}' should be a collection interface instead of a concrete type";
        private const string Description = "Return an IEnumerable<T> or ICollection<T> instead of a concrete collection class.";

        public const string DiagnosticId = "AV1130";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeMethodAction = context => context.SkipEmptyName(AnalyzeMethod);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMethodAction, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (method.ReturnsVoid || IsString(method.ReturnType) || IsImmutable(method.ReturnType) || method.IsSynthesized())
            {
                return;
            }

            if (method.IsPropertyOrEventAccessor() || method.IsOverride || method.IsInterfaceImplementation() ||
                method.HidesBaseMember(context.CancellationToken))
            {
                return;
            }

            if (IsArray(method.ReturnType) || IsClassOrStructThatImplementsIEnumerable(method.ReturnType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, method.Locations[0], method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
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

        private static bool IsArray([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Array;
        }

        private static bool IsClassOrStructThatImplementsIEnumerable([NotNull] ITypeSymbol type)
        {
            return IsClassOrStruct(type) && TypeImplementsIEnumerable(type);
        }

        private static bool IsClassOrStruct([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct;
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
