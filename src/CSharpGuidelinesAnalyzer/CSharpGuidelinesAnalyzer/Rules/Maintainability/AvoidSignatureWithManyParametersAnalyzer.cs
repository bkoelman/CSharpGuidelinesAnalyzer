﻿using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using CSharpGuidelinesAnalyzer.Settings;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidSignatureWithManyParametersAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMaxParameterCount = 3;

    private const string Title = "Signature contains too many parameters";
    private const string ParameterCountMessageFormat = "{0} contains {1} parameters, which exceeds the maximum of {2} parameters";
    private const string TupleParameterMessageFormat = "{0} contains tuple parameter '{1}'";
    private const string TupleReturnMessageFormat = "{0} returns a tuple with {1} elements, which exceeds the maximum of 2 elements";
    private const string Description = "Don't declare signatures with more than a predefined number of parameters.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1561";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor ParameterCountRule = new(DiagnosticId, Title, ParameterCountMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly DiagnosticDescriptor TupleParameterRule = new(DiagnosticId, Title, TupleParameterMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly DiagnosticDescriptor TupleReturnRule = new(DiagnosticId, Title, TupleReturnMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly AnalyzerSettingKey MaxParameterCountKey = new(DiagnosticId, "MaxParameterCount");

    [NotNull]
    private static readonly AnalyzerSettingKey MaxConstructorParameterCountKey = new(DiagnosticId, "MaxConstructorParameterCount");

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ParameterCountRule, TupleParameterRule, TupleReturnRule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
    {
        Guard.NotNull(startContext, nameof(startContext));

        var settingsReader = new AnalyzerSettingsReader(startContext.Options, startContext.CancellationToken);

        startContext.SafeRegisterSymbolAction(context => AnalyzeProperty(context, settingsReader), SymbolKind.Property);
        startContext.SafeRegisterSymbolAction(context => AnalyzeMethod(context, settingsReader), SymbolKind.Method);
        startContext.SafeRegisterSymbolAction(context => AnalyzeNamedType(context, settingsReader), SymbolKind.NamedType);
        startContext.SafeRegisterOperationAction(context => AnalyzeLocalFunction(context, settingsReader), OperationKind.LocalFunction);
    }

    [NotNull]
    private static ParameterSettings GetParameterSettings([NotNull] AnalyzerSettingsReader settingsReader, [NotNull] SyntaxTree syntaxTree)
    {
        int maxParameterCount = settingsReader.TryGetInt32(syntaxTree, MaxParameterCountKey, 0, 255) ?? DefaultMaxParameterCount;
        int maxConstructorParameterCount = settingsReader.TryGetInt32(syntaxTree, MaxConstructorParameterCountKey, 0, 255) ?? maxParameterCount;

        return new ParameterSettings(maxParameterCount, maxConstructorParameterCount);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, [NotNull] AnalyzerSettingsReader settingsReader)
    {
        var property = (IPropertySymbol)context.Symbol;

        if (property.IsIndexer && MemberRequiresAnalysis(property, context.CancellationToken))
        {
            ParameterSettings settings = GetParameterSettings(settingsReader, context.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);
            BaseAnalysisContext<ImmutableArray<IParameterSymbol>> parametersContext = context.Wrap(property.Parameters);
            var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(parametersContext, settings);

            AnalyzeParameters(info, property, "Indexer");
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, [NotNull] AnalyzerSettingsReader settingsReader)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (!method.IsPropertyOrEventAccessor() && MemberRequiresAnalysis(method, context.CancellationToken))
        {
            string memberName = GetMemberName(method);
            bool isConstructor = IsConstructor(method);

            ParameterSettings settings = GetParameterSettings(settingsReader, context.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);
            BaseAnalysisContext<ImmutableArray<IParameterSymbol>> parametersContext = context.Wrap(method.Parameters);
            var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(parametersContext, settings, isConstructor);

            AnalyzeParameters(info, method, memberName);

            if (MethodCanReturnValue(method))
            {
                BaseAnalysisContext<ITypeSymbol> typeContext = context.Wrap(method.ReturnType);
                AnalyzeReturnType(typeContext, method, memberName);
            }
        }
    }

    private static bool MemberRequiresAnalysis([NotNull] ISymbol member, CancellationToken cancellationToken)
    {
        return member is { IsExtern: false, IsOverride: false } && !member.HidesBaseMember(cancellationToken) && !member.IsInterfaceImplementation();
    }

    private static bool MethodCanReturnValue([NotNull] IMethodSymbol method)
    {
        return !IsConstructor(method) && method.MethodKind != MethodKind.Destructor;
    }

    [NotNull]
    private static string GetMemberName([NotNull] IMethodSymbol method)
    {
        return IsConstructor(method) ? GetNameForConstructor(method) : GetNameForMethod(method);
    }

    private static bool IsConstructor([NotNull] IMethodSymbol method)
    {
        return method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
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
        string kind = method.GetKind();

        builder.Append(kind);
        builder.Append(" '");
        builder.Append(method.Name);
        builder.Append("'");

        return builder.ToString();
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, [NotNull] AnalyzerSettingsReader settingsReader)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (IsDelegate(type))
        {
            ParameterSettings settings = GetParameterSettings(settingsReader, context.Symbol.DeclaringSyntaxReferences[0].SyntaxTree);

            AnalyzeDelegate(type, context, settings);
        }
    }

    private static void AnalyzeDelegate([NotNull] INamedTypeSymbol type, SymbolAnalysisContext context, [NotNull] ParameterSettings settings)
    {
        IMethodSymbol method = type.DelegateInvokeMethod;

        if (method != null)
        {
            string typeName = $"Delegate '{type.Name}'";

            BaseAnalysisContext<ImmutableArray<IParameterSymbol>> parametersContext = context.Wrap(method.Parameters);
            var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(parametersContext, settings);

            AnalyzeParameters(info, type, typeName);

            BaseAnalysisContext<ITypeSymbol> typeContext = context.Wrap(method.ReturnType);
            AnalyzeReturnType(typeContext, type, typeName);
        }
    }

    private static bool IsDelegate([NotNull] INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Delegate;
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context, [NotNull] AnalyzerSettingsReader settingsReader)
    {
        var operation = (ILocalFunctionOperation)context.Operation;

        string memberName = GetMemberName(operation.Symbol);

        ParameterSettings settings = GetParameterSettings(settingsReader, context.Operation.Syntax.SyntaxTree);

        BaseAnalysisContext<ImmutableArray<IParameterSymbol>> parametersContext = context.Wrap(operation.Symbol.Parameters);
        var info = new ParameterCountInfo<ImmutableArray<IParameterSymbol>>(parametersContext, settings);

        AnalyzeParameters(info, operation.Symbol, memberName);

        BaseAnalysisContext<ITypeSymbol> typeContext = context.Wrap(operation.Symbol.ReturnType);
        AnalyzeReturnType(typeContext, operation.Symbol, memberName);
    }

    private static void AnalyzeParameters(ParameterCountInfo<ImmutableArray<IParameterSymbol>> info, [NotNull] ISymbol member, [NotNull] string memberName)
    {
        ImmutableArray<IParameterSymbol> parameters = info.Context.Target;

        if (parameters.Length > info.MaxParameterCount)
        {
            BaseAnalysisContext<ISymbol> contextWithTarget = info.Context.WithTarget(member);
            ParameterCountInfo<ISymbol> memberInfo = info.ChangeContext(contextWithTarget);
            ReportParameterCount(memberInfo, memberName, parameters.Length);
        }

        foreach (IParameterSymbol parameter in parameters)
        {
            if (parameter.Type.IsTupleType || TryGetSystemTupleElementCount(parameter.Type) != null)
            {
                BaseAnalysisContext<IParameterSymbol> contextWithTarget = info.Context.WithTarget(parameter);
                ReportTupleParameter(contextWithTarget, memberName, parameter.Name);
            }
        }
    }

    private static void ReportParameterCount(ParameterCountInfo<ISymbol> info, [NotNull] string name, int parameterCount)
    {
        if (!info.Context.Target.IsSynthesized())
        {
            var diagnostic = Diagnostic.Create(ParameterCountRule, info.Context.Target.Locations[0], name, parameterCount, info.MaxParameterCount);
            info.Context.ReportDiagnostic(diagnostic);
        }
    }

    private static void ReportTupleParameter(BaseAnalysisContext<IParameterSymbol> context, [NotNull] string memberName, [NotNull] string parameterName)
    {
        if (!context.Target.IsSynthesized())
        {
            var diagnostic = Diagnostic.Create(TupleParameterRule, context.Target.Locations[0], memberName, parameterName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeReturnType(BaseAnalysisContext<ITypeSymbol> context, [NotNull] ISymbol member, [NotNull] string memberName)
    {
        int? elementCount = TryGetValueTupleElementCount(context.Target) ?? TryGetSystemTupleElementCount(context.Target);

        if (elementCount > 2)
        {
            BaseAnalysisContext<ISymbol> contextWithTarget = context.WithTarget(member);
            ReportTupleReturn(contextWithTarget, memberName, elementCount.Value);
        }
    }

    [CanBeNull]
    private static int? TryGetValueTupleElementCount([NotNull] ITypeSymbol type)
    {
        return type.IsTupleType && type is INamedTypeSymbol namedType ? namedType.TupleElements.Length : null;
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

    private static void ReportTupleReturn(BaseAnalysisContext<ISymbol> context, [NotNull] string memberName, int tupleElementCount)
    {
        if (!context.Target.IsSynthesized())
        {
            var diagnostic = Diagnostic.Create(TupleReturnRule, context.Target.Locations[0], memberName, tupleElementCount);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private sealed class ParameterSettings(int maxParameterCount, int maxConstructorParameterCount)
    {
        public int MaxParameterCount { get; } = maxParameterCount;
        public int MaxConstructorParameterCount { get; } = maxConstructorParameterCount;
    }

    private readonly struct ParameterCountInfo<TTarget>
    {
        public BaseAnalysisContext<TTarget> Context { get; }

        public int MaxParameterCount => isConstructor ? settings.MaxConstructorParameterCount : settings.MaxParameterCount;

        [NotNull]
        private readonly ParameterSettings settings;

        private readonly bool isConstructor;

        public ParameterCountInfo(BaseAnalysisContext<TTarget> context, [NotNull] ParameterSettings settings, bool isConstructor = false)
        {
            Guard.NotNull(settings, nameof(settings));

            Context = context;
            this.settings = settings;
            this.isConstructor = isConstructor;
        }

        public ParameterCountInfo<T> ChangeContext<T>(BaseAnalysisContext<T> context)
        {
            return new ParameterCountInfo<T>(context, settings, isConstructor);
        }
    }
}
