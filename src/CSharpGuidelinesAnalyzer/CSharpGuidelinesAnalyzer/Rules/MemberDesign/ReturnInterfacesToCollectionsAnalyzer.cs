using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReturnInterfacesToCollectionsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1130";

        private const string Title =
            "Return type in method signature should be a collection interface instead of a concrete type";

        private const string MessageFormat =
            "Return type in signature for '{0}' should be a collection interface instead of a concrete type.";

        private const string Description =
            "Return an IEnumerable<T> or ICollection<T> instead of a concrete collection class.";

        private const string Category = "Member Design";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol) context.Symbol;

            if (IsString(method.ReturnType))
            {
                return;
            }

            if (method.IsOverride || method.IsInterfaceImplementation() ||
                method.HidesBaseMember(context.CancellationToken))
            {
                return;
            }

            if (IsArray(method.ReturnType) ||
                (IsClassOrStruct(method.ReturnType) && DoesTypeImplementIEnumerable(method.ReturnType)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0],
                    method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        private static bool IsString([NotNull] ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_String;
        }

        private static bool IsArray([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Array;
        }

        private static bool IsClassOrStruct([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct;
        }

        private static bool DoesTypeImplementIEnumerable([NotNull] ITypeSymbol type)
        {
            return type.AllInterfaces.Any(IsIEnumerable);
        }

        private static bool IsIEnumerable([NotNull] INamedTypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Collections_IEnumerable;
        }
    }
}