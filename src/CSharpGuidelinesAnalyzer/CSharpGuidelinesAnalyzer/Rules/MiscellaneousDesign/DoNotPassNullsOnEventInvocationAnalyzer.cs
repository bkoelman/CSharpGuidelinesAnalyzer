using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotPassNullsOnEventInvocationAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1235";

        private const string SenderTitle = "'sender' argument is null in non-static event invocation";
        private const string SenderMessageFormat = "'sender' argument is null in non-static event invocation.";
        private const string ArgsTitle = "Argument for second parameter is null in event invocation";
        private const string ArgsMessageFormat = "'{0}' argument is null in event invocation.";
        private const string Description = "Don't pass null as the sender argument when raising an event.";
        private const string Category = "Miscellaneous Design";

        [NotNull]
        private static readonly DiagnosticDescriptor SenderRule = new DiagnosticDescriptor(DiagnosticId, SenderTitle,
            SenderMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ArgsRule = new DiagnosticDescriptor(DiagnosticId, ArgsTitle,
            ArgsMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SenderRule, ArgsRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    INamedTypeSymbol systemEventArgs = startContext.Compilation.GetTypeByMetadataName("System.EventArgs");
                    if (systemEventArgs != null)
                    {
                        startContext.RegisterOperationAction(
                            c => c.SkipInvalid(_ => AnalyzeInvocationExpression(c, systemEventArgs)),
                            OperationKind.Invocation);
                    }
                }
            });
        }

        private void AnalyzeInvocationExpression(OperationAnalysisContext context, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.Arguments.Length != 2)
            {
                return;
            }

            AnalyzeEventInvocation(invocation, context, systemEventArgs);
        }

        private void AnalyzeEventInvocation([NotNull] IInvocationOperation invocation, OperationAnalysisContext context,
            [NotNull] INamedTypeSymbol systemEventArgs)
        {
            bool? targetsStaticEvent = IsStaticEvent(invocation.Instance, context.Compilation);
            if (targetsStaticEvent != null)
            {
                if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
                {
                    if (!targetsStaticEvent.Value)
                    {
                        AnalyzeSenderArgument(invocation, context);
                    }

                    AnalyzeArgsArgument(invocation, systemEventArgs, context);
                }
            }
        }

        [CanBeNull]
        private static bool? IsStaticEvent([NotNull] IOperation operation, [NotNull] Compilation compilation)
        {
            return IsStaticEventInvocation(operation) ??
                IsStaticEventInvocationUsingNullConditionalAccessOperator(operation, compilation);
        }

        [CanBeNull]
        private static bool? IsStaticEventInvocation([NotNull] IOperation operation)
        {
            if (operation is IEventReferenceOperation eventReference)
            {
                return eventReference.Instance == null;
            }

            return null;
        }

        [CanBeNull]
        private static bool? IsStaticEventInvocationUsingNullConditionalAccessOperator([NotNull] IOperation operation,
            [NotNull] Compilation compilation)
        {
            if (operation is IConditionalAccessInstanceOperation)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
                if (model.GetSymbolInfo(operation.Syntax).Symbol is IEventSymbol eventSymbol)
                {
                    return eventSymbol.IsStatic;
                }
            }

            return null;
        }

        private void AnalyzeSenderArgument([NotNull] IInvocationOperation invocation, OperationAnalysisContext context)
        {
            IArgumentOperation senderArgument = GetSenderArgument(invocation);
            if (senderArgument != null && IsNullConstant(senderArgument.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(SenderRule, senderArgument.Syntax.GetLocation()));
            }
        }

        [CanBeNull]
        private IArgumentOperation GetSenderArgument([NotNull] IInvocationOperation invocation)
        {
            IArgumentOperation argument = invocation.Arguments.FirstOrDefault(x => x.Parameter.Name == "sender");

            return argument != null && argument.Parameter.Type.SpecialType == SpecialType.System_Object ? argument : null;
        }

        private void AnalyzeArgsArgument([NotNull] IInvocationOperation invocation, [NotNull] INamedTypeSymbol systemEventArgs,
            OperationAnalysisContext context)
        {
            IArgumentOperation argsArgument = GetArgsArgument(invocation, systemEventArgs);
            if (argsArgument != null && IsNullConstant(argsArgument.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(ArgsRule, argsArgument.Syntax.GetLocation(),
                    argsArgument.Parameter.Name));
            }
        }

        [CanBeNull]
        private IArgumentOperation GetArgsArgument([NotNull] IInvocationOperation invocation, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            return invocation.Arguments.FirstOrDefault(x =>
                !string.IsNullOrEmpty(x.Parameter?.Name) && IsEventArgs(x.Parameter.Type, systemEventArgs));
        }

        private static bool IsEventArgs([CanBeNull] ITypeSymbol type, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            ITypeSymbol nextType = type;
            while (nextType != null)
            {
                if (nextType.Equals(systemEventArgs))
                {
                    return true;
                }

                nextType = nextType.BaseType;
            }

            return false;
        }

        private static bool IsNullConstant([NotNull] IOperation operation)
        {
            return operation.ConstantValue.HasValue && operation.ConstantValue.Value == null;
        }
    }
}
