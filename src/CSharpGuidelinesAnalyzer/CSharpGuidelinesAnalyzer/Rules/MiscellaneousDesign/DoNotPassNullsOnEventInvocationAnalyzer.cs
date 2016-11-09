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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(SenderRule, ArgsRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (!startContext.Compilation.SupportsOperations())
                {
                    return;
                }

                INamedTypeSymbol systemEventArgs = startContext.Compilation.GetTypeByMetadataName("System.EventArgs");
                if (systemEventArgs != null)
                {
                    startContext.RegisterOperationAction(c => AnalyzeEventInvocation(c, systemEventArgs),
                        OperationKind.InvocationExpression);
                }
            });
        }

        private void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] INamedTypeSymbol systemEventArgs)
        {
            var expression = (IInvocationExpression) context.Operation;

            bool? targetsStaticEvent = IsStaticEvent(expression.Instance, context.Compilation);
            if (targetsStaticEvent == null)
            {
                return;
            }

            if (expression.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
            {
                if (targetsStaticEvent == false)
                {
                    IArgument senderArgument = GetSenderArgument(expression);
                    if (senderArgument != null && IsNullConstant(senderArgument.Value))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(SenderRule, senderArgument.Syntax.GetLocation()));
                    }
                }

                context.CancellationToken.ThrowIfCancellationRequested();

                IArgument argsArgument = GetArgsArgument(expression, systemEventArgs);
                if (argsArgument != null && IsNullConstant(argsArgument.Value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ArgsRule, argsArgument.Syntax.GetLocation(),
                        argsArgument.Parameter.Name));
                }
            }
        }

        [CanBeNull]
        private bool? IsStaticEvent([NotNull] IOperation operation, [NotNull] Compilation compilation)
        {
            var eventReference = operation as IEventReferenceExpression;
            if (eventReference != null)
            {
                return eventReference.Instance == null;
            }

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

        [CanBeNull]
        private IArgument GetSenderArgument([NotNull] IInvocationExpression invocationExpression)
        {
            IArgument argument = invocationExpression.ArgumentsInParameterOrder.FirstOrDefault();

            return argument != null && argument.Parameter.Name == "sender" &&
                argument.Parameter.Type.SpecialType == SpecialType.System_Object
                    ? argument
                    : null;
        }

        [CanBeNull]
        private IArgument GetArgsArgument([NotNull] IInvocationExpression invocationExpression,
            [NotNull] INamedTypeSymbol systemEventArgs)
        {
            if (invocationExpression.ArgumentsInParameterOrder.Length == 2)
            {
                IArgument argument = invocationExpression.ArgumentsInParameterOrder[1];
                if (argument.Parameter != null && argument.Parameter.Name.Length > 0)
                {
                    if (IsEventArgs(argument.Parameter.Type, systemEventArgs))
                    {
                        return argument;
                    }
                }
            }

            return null;
        }

        private bool IsEventArgs([CanBeNull] ITypeSymbol type, [NotNull] INamedTypeSymbol systemEventArgs)
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

        private bool IsNullConstant([NotNull] IOperation operation)
        {
            return operation.ConstantValue.HasValue && operation.ConstantValue.Value == null;
        }
    }
}