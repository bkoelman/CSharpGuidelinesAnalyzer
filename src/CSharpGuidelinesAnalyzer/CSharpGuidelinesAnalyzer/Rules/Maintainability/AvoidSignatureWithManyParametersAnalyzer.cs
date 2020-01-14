using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using CSharpGuidelinesAnalyzer.Settings;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidSignatureWithManyParametersAnalyzer : DiagnosticAnalyzer
    {
        private const int DefaultMaxParameterCount = 3;

        private const string Title = "Signature contains too many parameters";

        private const string ParameterCountMessageFormat =
            "{0} contains {1} parameters, which exceeds the maximum of {2} parameters.";

        private const string TupleParameterMessageFormat = "{0} contains tuple parameter '{1}'.";

        private const string TupleReturnMessageFormat =
            "{0} returns a tuple with {1} elements, which exceeds the maximum of 2 elements.";

        private const string Description = "Don't declare signatures with more than a predefined number of parameters.";

        public const string DiagnosticId = "AV1561";

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

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

        [NotNull]
        private static readonly Action<SymbolAnalysisContext, int> AnalyzePropertyAction = (context, maxParameterCount) =>
            context.SkipEmptyName(_ => AnalyzeProperty(context, maxParameterCount));

        [NotNull]
        private static readonly Action<SymbolAnalysisContext, int> AnalyzeMethodAction = (context, maxParameterCount) =>
            context.SkipEmptyName(_ => AnalyzeMethod(context, maxParameterCount));

        [NotNull]
        private static readonly Action<SymbolAnalysisContext, int> AnalyzeNamedTypeAction = (context, maxParameterCount) =>
            context.SkipEmptyName(_ => AnalyzeNamedType(context, maxParameterCount));

        [NotNull]
        private static readonly Action<OperationAnalysisContext, int> AnalyzeLocalFunctionAction = (context, maxParameterCount) =>
            context.SkipInvalid(_ => AnalyzeLocalFunction(context, maxParameterCount));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ParameterCountRule, TupleParameterRule, TupleReturnRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            Guard.NotNull(startContext, nameof(startContext));

            int maxParameterCount = GetMaxParameterCountFromSettings(startContext.Options, startContext.CancellationToken);

            startContext.RegisterSymbolAction(actionContext => AnalyzePropertyAction(actionContext, maxParameterCount),
                SymbolKind.Property);

            startContext.RegisterSymbolAction(actionContext => AnalyzeMethodAction(actionContext, maxParameterCount),
                SymbolKind.Method);

            startContext.RegisterSymbolAction(actionContext => AnalyzeNamedTypeAction(actionContext, maxParameterCount),
                SymbolKind.NamedType);

            startContext.RegisterOperationAction(actionContext => AnalyzeLocalFunctionAction(actionContext, maxParameterCount),
                OperationKind.LocalFunction);
        }

        private static int GetMaxParameterCountFromSettings([NotNull] AnalyzerOptions options,
            CancellationToken cancellationToken)
        {
            AnalyzerSettingsRegistry registry = AnalyzerSettingsProvider.LoadSettings(options, cancellationToken);

            var settingKey = new AnalyzerSettingKey(DiagnosticId, "MaxParameterCount");
            return registry.TryGetInt32(settingKey, 0, 255) ?? DefaultMaxParameterCount;
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context, int maxParameterCount)
        {
            var property = (IPropertySymbol)context.Symbol;

            if (property.IsIndexer && MemberRequiresAnalysis(property, context.CancellationToken))
            {
                var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(context.Wrap(property.Parameters),
                    maxParameterCount);

                AnalyzeParameters(info, property, "Indexer");
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, int maxParameterCount)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (!method.IsPropertyOrEventAccessor() && MemberRequiresAnalysis(method, context.CancellationToken))
            {
                string memberName = GetMemberName(method);

                var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(context.Wrap(method.Parameters),
                    maxParameterCount);

                AnalyzeParameters(info, method, memberName);

                if (MethodCanReturnValue(method))
                {
                    AnalyzeReturnType(context.Wrap(method.ReturnType), method, memberName);
                }
            }
        }

        private static bool MemberRequiresAnalysis([NotNull] ISymbol member, CancellationToken cancellationToken)
        {
            return !member.IsExtern && !member.IsOverride && !member.HidesBaseMember(cancellationToken) &&
                !member.IsInterfaceImplementation();
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

        private static void AnalyzeNamedType(SymbolAnalysisContext context, int maxParameterCount)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (IsDelegate(type))
            {
                AnalyzeDelegate(type, context, maxParameterCount);
            }
        }

        private static void AnalyzeDelegate([NotNull] INamedTypeSymbol type, SymbolAnalysisContext context, int maxParameterCount)
        {
            IMethodSymbol method = type.DelegateInvokeMethod;

            if (method != null)
            {
                string typeName = $"Delegate '{type.Name}'";

                var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(context.Wrap(method.Parameters),
                    maxParameterCount);

                AnalyzeParameters(info, type, typeName);

                AnalyzeReturnType(context.Wrap(method.ReturnType), type, typeName);
            }
        }

        private static bool IsDelegate([NotNull] INamedTypeSymbol type)
        {
            return type.TypeKind == TypeKind.Delegate;
        }

        private static void AnalyzeLocalFunction(OperationAnalysisContext context, int maxParameterCount)
        {
            var operation = (ILocalFunctionOperation)context.Operation;

            string memberName = GetMemberName(operation.Symbol);

            var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(context.Wrap(operation.Symbol.Parameters),
                maxParameterCount);

            AnalyzeParameters(info, operation.Symbol, memberName);

            AnalyzeReturnType(context.Wrap(operation.Symbol.ReturnType), operation.Symbol, memberName);
        }

        private static void AnalyzeParameters(ParameterCountInfo<ImmutableArray<IParameterSymbol>> info, [NotNull] ISymbol member,
            [NotNull] string memberName)
        {
            ImmutableArray<IParameterSymbol> parameters = info.Context.Target;

            if (parameters.Length > info.MaxParameterCount)
            {
                var memberInfo = new ParameterCountInfo<ISymbol>(info.Context.WithTarget(member), info.MaxParameterCount);
                ReportParameterCount(memberInfo, memberName, parameters.Length);
            }

            foreach (IParameterSymbol parameter in parameters)
            {
                if (parameter.Type.IsTupleType || TryGetSystemTupleElementCount(parameter.Type) != null)
                {
                    ReportTupleParameter(info.Context.WithTarget(parameter), memberName, parameter.Name);
                }
            }
        }

        private static void ReportParameterCount(ParameterCountInfo<ISymbol> info, [NotNull] string name, int parameterCount)
        {
            if (!info.Context.Target.IsSynthesized())
            {
                var diagnostic = Diagnostic.Create(ParameterCountRule, info.Context.Target.Locations[0], name, parameterCount,
                    info.MaxParameterCount);

                info.Context.ReportDiagnostic(diagnostic);
            }
        }

        private static void ReportTupleParameter(BaseAnalysisContext<IParameterSymbol> context, [NotNull] string memberName,
            [NotNull] string parameterName)
        {
            if (!context.Target.IsSynthesized())
            {
                var diagnostic = Diagnostic.Create(TupleParameterRule, context.Target.Locations[0], memberName, parameterName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeReturnType(BaseAnalysisContext<ITypeSymbol> context, [NotNull] ISymbol member,
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
        private static int? TryGetSystemTupleElementCount([NotNull] ITypeSymbol type)
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
                var diagnostic = Diagnostic.Create(TupleReturnRule, context.Target.Locations[0], memberName, tupleElementCount);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private struct ParameterCountInfo<TTarget>
        {
            public BaseAnalysisContext<TTarget> Context { get; }
            public int MaxParameterCount { get; }

            public ParameterCountInfo(BaseAnalysisContext<TTarget> context, int maxParameterCount)
            {
                Context = context;
                MaxParameterCount = maxParameterCount;
            }
        }
    }
}
