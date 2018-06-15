using System;
using System.Collections.Immutable;
using System.Text;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidSignatureWithManyParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1561";

        private const int MaxParameterCount = 3;
        private const string MaxParameterCountText = "3";

        private const string Title = "Signature contains more than " + MaxParameterCountText + " parameters";

        private const string ParameterCountMessageFormat = "{0} contains {1} parameters, which exceeds the maximum of " +
            MaxParameterCountText + " parameters.";

        private const string TupleParameterMessageFormat = "{0} contains tuple parameter '{1}'.";

        private const string TupleReturnMessageFormat =
            "{0} returns a tuple with {1} elements, which exceeds the maximum of 2 elements.";

        private const string Description = "Don't declare signatures with more than " + MaxParameterCountText + " parameters.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor ParameterCountRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ParameterCountMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor TupleParameterRule = new DiagnosticDescriptor(DiagnosticId, Title,
            TupleParameterMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor TupleReturnRule = new DiagnosticDescriptor(DiagnosticId, Title,
            TupleReturnMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ParameterCountRule, TupleParameterRule, TupleReturnRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeProperty), SymbolKind.Property);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMethod), SymbolKind.Method);
            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeNamedType), SymbolKind.NamedType);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;

            if (property.IsIndexer)
            {
                AnalyzeParameters(context.Wrap(property.Parameters), property, "Indexer");
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (!method.IsPropertyOrEventAccessor())
            {
                string memberName = GetMemberName(method);

                AnalyzeParameters(context.Wrap(method.Parameters), method, memberName);

                if (MethodCanReturnValue(method))
                {
                    AnalyzeReturnType(context.Wrap(method.ReturnType), method, memberName);
                }
            }
        }

        private static bool MethodCanReturnValue([NotNull] IMethodSymbol method)
        {
            return method.MethodKind != MethodKind.Constructor && method.MethodKind != MethodKind.StaticConstructor &&
                method.MethodKind != MethodKind.Destructor;
        }

        [NotNull]
        private static string GetMemberName([NotNull] IMethodSymbol method)
        {
            return IsConstructor(method) ? GetNameForConstructor(method) : GetNameForMethod(method);
        }

        private static bool IsConstructor([NotNull] IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Constructor;
        }

        [NotNull]
        private static string GetNameForConstructor([NotNull] IMethodSymbol method)
        {
            var builder = new StringBuilder();
            builder.Append("Constructor for '");
            builder.Append(method.ContainingType.Name);
            builder.Append("'");
            return builder.ToString();
        }

        [NotNull]
        private static string GetNameForMethod([NotNull] IMethodSymbol method)
        {
            var builder = new StringBuilder();

            builder.Append(method.GetKind());
            builder.Append(" '");
            builder.Append(method.Name);
            builder.Append("'");

            return builder.ToString();
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (IsDelegate(type))
            {
                IMethodSymbol method = type.DelegateInvokeMethod;
                if (method != null)
                {
                    string typeName = $"Delegate '{type.Name}'";

                    AnalyzeParameters(context.Wrap(method.Parameters), type, typeName);
                    AnalyzeReturnType(context.Wrap(method.ReturnType), type, typeName);
                }
            }
        }

        private static bool IsDelegate([NotNull] INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Delegate;
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var operation = (ILocalFunctionOperation)context.Operation;

            string memberName = GetMemberName(operation.Symbol);

            AnalyzeParameters(context.Wrap(operation.Symbol.Parameters), operation.Symbol, memberName);
            AnalyzeReturnType(context.Wrap(operation.Symbol.ReturnType), operation.Symbol, memberName);
        }

        private void AnalyzeParameters(BaseAnalysisContext<ImmutableArray<IParameterSymbol>> context, [NotNull] ISymbol member,
            [NotNull] string memberName)
        {
            ImmutableArray<IParameterSymbol> parameters = context.Target;

            if (parameters.Length > MaxParameterCount)
            {
                ReportParameterCount(context.WithTarget(member), memberName, parameters.Length);
            }

            foreach (IParameterSymbol parameter in parameters)
            {
                if (parameter.Type.IsTupleType || TryGetSystemTupleElementCount(parameter.Type) != null)
                {
                    ReportTupleParameter(context.WithTarget(parameter), memberName, parameter.Name);
                }
            }
        }

        private static void ReportParameterCount(BaseAnalysisContext<ISymbol> context, [NotNull] string name, int parameterCount)
        {
            if (!context.Target.IsSynthesized())
            {
                Diagnostic diagnostic = Diagnostic.Create(ParameterCountRule, context.Target.Locations[0], name, parameterCount);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void ReportTupleParameter(BaseAnalysisContext<IParameterSymbol> context, [NotNull] string memberName,
            [NotNull] string parameterName)
        {
            if (!context.Target.IsSynthesized())
            {
                Diagnostic diagnostic =
                    Diagnostic.Create(TupleParameterRule, context.Target.Locations[0], memberName, parameterName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeReturnType(BaseAnalysisContext<ITypeSymbol> context, [NotNull] ISymbol member,
            [NotNull] string memberName)
        {
            int? elementCount = TryGetValueTupleElementCount(context.Target) ?? TryGetSystemTupleElementCount(context.Target);
            if (elementCount > 2)
            {
                ReportTupleReturn(context.WithTarget(member), memberName, elementCount.Value);
            }
        }

        [CanBeNull]
        private static int? TryGetValueTupleElementCount([NotNull] ITypeSymbol type)
        {
            return type.IsTupleType && type is INamedTypeSymbol namedType ? (int?)namedType.TupleElements.Length : null;
        }

        [CanBeNull]
        private int? TryGetSystemTupleElementCount([NotNull] ITypeSymbol type)
        {
            if (type.Name == "Tuple" && type.ToString().StartsWith("System.Tuple<", StringComparison.Ordinal))
            {
                if (type is INamedTypeSymbol namedType)
                {
                    return namedType.TypeParameters.Length;
                }
            }

            return null;
        }

        private static void ReportTupleReturn(BaseAnalysisContext<ISymbol> context, [NotNull] string memberName,
            int tupleElementCount)
        {
            if (!context.Target.IsSynthesized())
            {
                Diagnostic diagnostic =
                    Diagnostic.Create(TupleReturnRule, context.Target.Locations[0], memberName, tupleElementCount);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
