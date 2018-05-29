using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotImplicitlyConvertToDynamicAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2230";

        private const string Title = "An expression is implicitly converted to dynamic";
        private const string MessageFormat = "An expression of type '{0}' is implicitly converted to dynamic.";
        private const string Description = "Only use the dynamic keyword when talking to a dynamic object.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol objectHandleType = KnownTypes.SystemRuntimeRemotingObjectHandle(startContext.Compilation);

                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeConversion(c, objectHandleType)),
                    OperationKind.Conversion);
                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeCompoundAssignment(c, objectHandleType)),
                    OperationKind.CompoundAssignment);
            });
        }

        private void AnalyzeConversion(OperationAnalysisContext context, [CanBeNull] INamedTypeSymbol objectHandleType)
        {
            var conversion = (IConversionOperation)context.Operation;
            if (!conversion.IsImplicit)
            {
                return;
            }

            ITypeSymbol sourceType = conversion.Operand.Type;
            ITypeSymbol destinationType = conversion.Type;

            AnalyzeFromToConversion(sourceType, destinationType, objectHandleType, conversion.Syntax.GetLocation(),
                context.ReportDiagnostic);
        }

        private void AnalyzeCompoundAssignment(OperationAnalysisContext context, [CanBeNull] INamedTypeSymbol objectHandleType)
        {
            var compoundAssignment = (ICompoundAssignmentOperation)context.Operation;

            ITypeSymbol sourceType = compoundAssignment.Value.Type;
            ITypeSymbol destinationType = compoundAssignment.Target.Type;

            AnalyzeFromToConversion(sourceType, destinationType, objectHandleType, compoundAssignment.Value.Syntax.GetLocation(),
                context.ReportDiagnostic);
        }

        private void AnalyzeFromToConversion([CanBeNull] ITypeSymbol sourceType, [NotNull] ITypeSymbol destinationType,
            [CanBeNull] INamedTypeSymbol objectHandleType, [NotNull] Location reportLocation,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (!IsDynamic(destinationType))
            {
                return;
            }

            if (sourceType == null || IsObject(sourceType) || IsObjectHandle(sourceType, objectHandleType))
            {
                return;
            }

            if (sourceType.TypeKind != TypeKind.Dynamic)
            {
                string sourceTypeName = sourceType.IsAnonymousType ? "(anonymous)" : sourceType.Name;
                reportDiagnostic(Diagnostic.Create(Rule, reportLocation, sourceTypeName));
            }
        }

        private static bool IsDynamic([NotNull] ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Dynamic;
        }

        private static bool IsObject([NotNull] ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Object;
        }

        private bool IsObjectHandle([NotNull] ITypeSymbol type, [CanBeNull] INamedTypeSymbol objectHandleType)
        {
            return objectHandleType != null && objectHandleType.Equals(type);
        }
    }
}
