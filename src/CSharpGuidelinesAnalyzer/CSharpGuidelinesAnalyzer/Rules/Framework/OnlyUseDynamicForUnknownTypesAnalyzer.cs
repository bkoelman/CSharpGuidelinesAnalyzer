using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OnlyUseDynamicForUnknownTypesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2230";

        private const string Title = "A non-dynamic result is implicitly assigned to a dynamic identifier";
        private const string MessageFormat = "A non-dynamic result is implicitly assigned to dynamic identifier '{0}'.";
        private const string Description = "Only use the dynamic keyword when talking to a dynamic object.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(c => c.SkipInvalid(AnalyzeVariableDeclaration),
                        OperationKind.VariableDeclaration);
                    startContext.RegisterOperationAction(c => c.SkipInvalid(AnalyzeAssignment),
                        OperationKind.AssignmentExpression);
                }
            });
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context)
        {
            var declaration = (IVariableDeclaration) context.Operation;

            if (declaration.Variable.Type.TypeKind == TypeKind.Dynamic)
            {
                if (RequiresReport(declaration.InitialValue))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Syntax.GetLocation(),
                        declaration.Variable.Name));
                }
            }
        }

        private void AnalyzeAssignment(OperationAnalysisContext context)
        {
            var assignment = (IAssignmentExpression) context.Operation;

            IdentifierInfo identifierInfo = assignment.Target.TryGetIdentifierInfo();
            if (identifierInfo != null && identifierInfo.Type.TypeKind == TypeKind.Dynamic)
            {
                if (RequiresReport(assignment.Value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Syntax.GetLocation(),
                        identifierInfo.Name));
                }
            }
        }

        private bool RequiresReport([CanBeNull] IOperation value)
        {
            var conversion = value as IConversionExpression;
            if (conversion != null && !conversion.IsExplicit)
            {
                ITypeSymbol sourceType = conversion.Operand.Type;

                if (sourceType != null && sourceType.TypeKind != TypeKind.Error &&
                    sourceType.TypeKind != TypeKind.Dynamic && sourceType.SpecialType != SpecialType.System_Object)
                {
                    return true;
                }
            }

            return false;
        }
    }
}