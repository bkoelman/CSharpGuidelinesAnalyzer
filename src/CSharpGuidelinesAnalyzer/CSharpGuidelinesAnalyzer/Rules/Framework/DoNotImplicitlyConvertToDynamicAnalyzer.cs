using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotImplicitlyConvertToDynamicAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "An expression is implicitly converted to dynamic";
    private const string MessageFormat = "An expression of type '{0}' is implicitly converted to dynamic";
    private const string Description = "Only use the dynamic keyword when talking to a dynamic object.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2230";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
    [NotNull]
    private static readonly Action<OperationAnalysisContext, INamedTypeSymbol> AnalyzeConversionAction = (context, objectHandleType) =>
        context.SkipInvalid(_ => AnalyzeConversion(context, objectHandleType));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
    [NotNull]
    private static readonly Action<OperationAnalysisContext, INamedTypeSymbol> AnalyzeCompoundAssignmentAction = (context, objectHandleType) =>
        context.SkipInvalid(_ => AnalyzeCompoundAssignment(context, objectHandleType));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStartAction);
    }

    private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
    {
        INamedTypeSymbol objectHandleType = KnownTypes.SystemRuntimeRemotingObjectHandle(startContext.Compilation);

        startContext.RegisterOperationAction(context => AnalyzeConversionAction(context, objectHandleType), OperationKind.Conversion);

        startContext.RegisterOperationAction(context => AnalyzeCompoundAssignmentAction(context, objectHandleType), OperationKind.CompoundAssignment);
    }

    private static void AnalyzeConversion(OperationAnalysisContext context, [CanBeNull] INamedTypeSymbol objectHandleType)
    {
        var conversion = (IConversionOperation)context.Operation;

        if (!conversion.IsImplicit)
        {
            return;
        }

        ITypeSymbol sourceType = conversion.Operand.Type;
        ITypeSymbol destinationType = conversion.Type;

        if (RequiresReport(sourceType, destinationType, objectHandleType))
        {
            Location location = conversion.Syntax.GetLocation();
            ReportAt(sourceType, location, context.ReportDiagnostic);
        }
    }

    private static void AnalyzeCompoundAssignment(OperationAnalysisContext context, [CanBeNull] INamedTypeSymbol objectHandleType)
    {
        var compoundAssignment = (ICompoundAssignmentOperation)context.Operation;

        ITypeSymbol sourceType = compoundAssignment.Value.Type;
        ITypeSymbol destinationType = compoundAssignment.Target.Type;

        if (RequiresReport(sourceType, destinationType, objectHandleType))
        {
            Location location = compoundAssignment.Value.Syntax.GetLocation();
            ReportAt(sourceType, location, context.ReportDiagnostic);
        }
    }

    private static bool RequiresReport([CanBeNull] ITypeSymbol sourceType, [NotNull] ITypeSymbol destinationType,
        [CanBeNull] INamedTypeSymbol objectHandleType)
    {
        if (!IsDynamic(destinationType))
        {
            return false;
        }

        if (sourceType == null || IsObject(sourceType) || IsObjectHandle(sourceType, objectHandleType))
        {
            return false;
        }

        return sourceType.TypeKind != TypeKind.Dynamic;
    }

    private static bool IsDynamic([NotNull] ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Dynamic;
    }

    private static bool IsObject([NotNull] ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_Object;
    }

    private static bool IsObjectHandle([NotNull] ITypeSymbol type, [CanBeNull] INamedTypeSymbol objectHandleType)
    {
        return objectHandleType != null && objectHandleType.IsEqualTo(type);
    }

    private static void ReportAt([NotNull] ITypeSymbol sourceType, [NotNull] Location reportLocation, [NotNull] Action<Diagnostic> reportDiagnostic)
    {
        string sourceTypeName = sourceType.IsAnonymousType ? "(anonymous)" : sourceType.Name;

        var diagnostic = Diagnostic.Create(Rule, reportLocation, sourceTypeName);
        reportDiagnostic(diagnostic);
    }
}