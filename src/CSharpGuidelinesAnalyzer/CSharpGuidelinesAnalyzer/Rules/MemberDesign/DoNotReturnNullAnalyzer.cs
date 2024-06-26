﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotReturnNullAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Do not return null for strings, collections or tasks";
    private const string MessageFormat = "null is returned from {0} '{1}' which has return type of string, collection or task";
    private const string Description = "Properties, arguments and return values representing strings, collections or tasks should never be null.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1135";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<OperationKind> ReturnOperationKinds = ImmutableArray.Create(OperationKind.Return, OperationKind.YieldReturn);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
    {
        IList<INamedTypeSymbol> taskTypes = ResolveTaskTypes(startContext.Compilation).ToList();

        startContext.SafeRegisterOperationAction(context => AnalyzeReturn(context, taskTypes), ReturnOperationKinds);
    }

    [NotNull]
    [ItemNotNull]
    private static IEnumerable<INamedTypeSymbol> ResolveTaskTypes([NotNull] Compilation compilation)
    {
        foreach (INamedTypeSymbol taskType in new[]
        {
            KnownTypes.SystemThreadingTasksTaskT(compilation),
            KnownTypes.SystemThreadingTasksTask(compilation),
            KnownTypes.SystemThreadingTasksValueTask(compilation),
            KnownTypes.SystemThreadingTasksValueTaskT(compilation)
        })
        {
            if (taskType != null)
            {
                yield return taskType;
            }
        }
    }

    private static void AnalyzeReturn(OperationAnalysisContext context, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
    {
        var returnOperation = (IReturnOperation)context.Operation;

        if (returnOperation.ReturnedValue?.Type == null || !ReturnsStringOrCollectionOrTask(returnOperation, taskTypes))
        {
            return;
        }

        if (returnOperation.ReturnedValue.ConstantValue is { HasValue: true, Value: null })
        {
            ReportReturnStatement(returnOperation, context);
        }
    }

    private static bool ReturnsStringOrCollectionOrTask([NotNull] IReturnOperation returnOperation, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
    {
        return returnOperation.ReturnedValue.Type.IsOrImplementsIEnumerable() || IsTask(returnOperation.ReturnedValue.Type, taskTypes);
    }

    private static bool IsTask([NotNull] ITypeSymbol type, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
    {
        return taskTypes.Any(taskType => taskType.IsEqualTo(type.OriginalDefinition));
    }

    private static void ReportReturnStatement([NotNull] IReturnOperation returnOperation, OperationAnalysisContext context)
    {
        IMethodSymbol method = returnOperation.TryGetContainingMethod(context.Compilation);

        if (method != null && !method.IsSynthesized())
        {
            Location location = returnOperation.ReturnedValue.Syntax.GetLocation();
            string kind = method.GetKind().ToLowerInvariant();
            string name = method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            var diagnostic = Diagnostic.Create(Rule, location, kind, name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
