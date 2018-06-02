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
                AnalyzeParameters(property.Parameters, property, "Indexer", context.ReportDiagnostic);
            }
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (!method.IsPropertyOrEventAccessor())
            {
                string memberName = GetMemberName(method);

                AnalyzeParameters(method.Parameters, method, memberName, context.ReportDiagnostic);

                if (MethodCanReturnValue(method))
                {
                    AnalyzeReturnValue(method.ReturnType, method, memberName, context.ReportDiagnostic);
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

                    AnalyzeParameters(method.Parameters, type, typeName, context.ReportDiagnostic);
                    AnalyzeReturnValue(method.ReturnType, type, typeName, context.ReportDiagnostic);
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

            AnalyzeParameters(operation.Symbol.Parameters, operation.Symbol, memberName, context.ReportDiagnostic);
            AnalyzeReturnValue(operation.Symbol.ReturnType, operation.Symbol, memberName, context.ReportDiagnostic);
        }

        private void AnalyzeParameters([ItemNotNull] ImmutableArray<IParameterSymbol> parameters, [NotNull] ISymbol member,
            [NotNull] string memberName, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (parameters.Length > MaxParameterCount)
            {
                ReportParameterCount(member, memberName, parameters.Length, reportDiagnostic);
            }

            foreach (IParameterSymbol parameter in parameters)
            {
                if (parameter.Type.IsTupleType)
                {
                    ReportTupleParameter(parameter, memberName, parameter.Name, reportDiagnostic);
                }
            }
        }

        private static void ReportParameterCount([NotNull] ISymbol symbol, [NotNull] string name, int parameterCount,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (!symbol.IsSynthesized())
            {
                Diagnostic diagnostic = Diagnostic.Create(ParameterCountRule, symbol.Locations[0], name, parameterCount);
                reportDiagnostic(diagnostic);
            }
        }

        private static void ReportTupleParameter([NotNull] ISymbol symbol, [NotNull] string memberName,
            [NotNull] string parameterName, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (!symbol.IsSynthesized())
            {
                Diagnostic diagnostic = Diagnostic.Create(TupleParameterRule, symbol.Locations[0], memberName, parameterName);
                reportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeReturnValue([NotNull] ITypeSymbol returnType, [NotNull] ISymbol member, [NotNull] string memberName,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (returnType.IsTupleType && returnType is INamedTypeSymbol type)
            {
                if (type.TupleElements.Length > 2)
                {
                    ReportTupleReturn(member, memberName, type.TupleElements.Length, reportDiagnostic);
                }
            }
        }

        private static void ReportTupleReturn([NotNull] ISymbol symbol, [NotNull] string memberName, int tupleElementCount,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (!symbol.IsSynthesized())
            {
                Diagnostic diagnostic = Diagnostic.Create(TupleReturnRule, symbol.Locations[0], memberName, tupleElementCount);
                reportDiagnostic(diagnostic);
            }
        }
    }
}
