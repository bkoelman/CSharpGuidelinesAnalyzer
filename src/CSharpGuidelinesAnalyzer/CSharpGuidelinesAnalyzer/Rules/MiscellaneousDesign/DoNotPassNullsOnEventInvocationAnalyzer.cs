using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotPassNullsOnEventInvocationAnalyzer : DiagnosticAnalyzer
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
                        startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeEventInvocation(c, systemEventArgs)),
                            OperationKind.InvocationExpression);
                    }
                }
            });
        }

        private void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            var expression = (IInvocationExpression)context.Operation;

            bool? targetsStaticEvent = IsStaticEvent(expression.Instance, context.Compilation);
            if (targetsStaticEvent != null)
            {
                if (expression.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
                {
                    if (!targetsStaticEvent.Value)
                    {
                        AnalyzeSenderArgument(expression, context);
                    }

                    AnalyzeArgsArgument(expression, systemEventArgs, context);
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
            var eventReference = operation as IEventReferenceExpression;
            if (eventReference != null)
            {
                return eventReference.Instance == null;
            }

            return null;
        }

        [CanBeNull]
        private static bool? IsStaticEventInvocationUsingNullConditionalAccessOperator([NotNull] IOperation operation,
            [NotNull] Compilation compilation)
        {
            var conditionalAccess = operation as IConditionalAccessInstanceExpression;
            if (conditionalAccess != null)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
                var eventSymbol = model.GetSymbolInfo(operation.Syntax).Symbol as IEventSymbol;
                if (eventSymbol != null)
                {
                    return eventSymbol.IsStatic;
                }
            }

            return null;
        }

        private void AnalyzeSenderArgument([NotNull] IInvocationExpression invocation, OperationAnalysisContext context)
        {
            IArgument senderArgument = GetSenderArgument(invocation);
            if (senderArgument != null && IsNullConstant(senderArgument.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(SenderRule, senderArgument.Syntax.GetLocation()));
            }
        }

        [CanBeNull]
        private IArgument GetSenderArgument([NotNull] IInvocationExpression invocation)
        {
            IArgument argument = invocation.ArgumentsInParameterOrder.FirstOrDefault();

            return argument != null && argument.Parameter.Name == "sender" &&
                argument.Parameter.Type.SpecialType == SpecialType.System_Object
                    ? argument
                    : null;
        }

        private void AnalyzeArgsArgument([NotNull] IInvocationExpression invocation, [NotNull] INamedTypeSymbol systemEventArgs,
            OperationAnalysisContext context)
        {
            IArgument argsArgument = GetArgsArgument(invocation, systemEventArgs);
            if (argsArgument != null && IsNullConstant(argsArgument.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(ArgsRule, argsArgument.Syntax.GetLocation(),
                    argsArgument.Parameter.Name));
            }
        }

        [CanBeNull]
        private IArgument GetArgsArgument([NotNull] IInvocationExpression invocation, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            if (invocation.ArgumentsInParameterOrder.Length == 2)
            {
                IArgument argument = invocation.ArgumentsInParameterOrder[1];

                if (!string.IsNullOrEmpty(argument.Parameter?.Name) && IsEventArgs(argument.Parameter.Type, systemEventArgs))
                {
                    return argument;
                }
            }

            return null;
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
