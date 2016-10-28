using System.Collections.Immutable;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RaiseEventsFromProtectedVirtualMethodsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1225";

        private const string Title =
            "Methods that raise an event should be protected virtual and be named 'On' followed by event name.";

        private const string ModifiersMessageFormat =
            "Method '{0}' raises event '{1}', so should be protected and virtual.";

        private const string NameMessageFormat = "Method '{0}' raises event '{1}', so it should be named '{2}'.";
        private const string Description = "Use a protected virtual method to raise each event.";
        private const string Category = "Miscellaneous Design";

        [NotNull]
        private static readonly DiagnosticDescriptor ModifiersRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ModifiersMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: Description, helpLinkUri: HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor NameRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NameMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(NameRule, ModifiersRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterOperationAction(AnalyzeInvocation, OperationKind.InvocationExpression);
                }
            });
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationExpression) context.Operation;

            if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
            {
                IEventSymbol evnt = TryGetEvent(invocation.Instance, context.ContainingSymbol as IMethodSymbol,
                    context.Compilation, context.CancellationToken);
                if (evnt != null)
                {
                    AnalyzeEventInvocation(context, evnt);
                }
            }
        }

        [CanBeNull]
        private IEventSymbol TryGetEvent([NotNull] IOperation operation, [CanBeNull] IMethodSymbol containingMethod,
            [NotNull] Compilation compilation, CancellationToken cancellationToken)
        {
            var eventReference = operation as IEventReferenceExpression;
            if (eventReference != null)
            {
                return eventReference.Event;
            }

            var conditionalAccess = operation as IConditionalAccessInstanceExpression;
            if (conditionalAccess != null)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
                var eventSymbol = model.GetSymbolInfo(operation.Syntax, cancellationToken).Symbol as IEventSymbol;
                return eventSymbol;
            }

            var local = operation as ILocalReferenceExpression;
            if (local != null && containingMethod != null)
            {
                IOperation body = AnalysisUtilities.TryGetOperationBlockForMethod(containingMethod, compilation,
                    cancellationToken);
                if (body != null)
                {
                    var walker = new LocalAssignmentWalker(local.Local);
                    walker.Visit(body);
                    return walker.Event;
                }
            }

            return null;
        }

        private void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] IEventSymbol evnt)
        {
            var method = context.ContainingSymbol as IMethodSymbol;
            if (method != null)
            {
                string nameExpected = "On" + evnt.Name;
                if (method.Name != nameExpected)
                {
                    context.ReportDiagnostic(Diagnostic.Create(NameRule, method.Locations[0], method.Name, evnt.Name,
                        nameExpected));
                }

                if (!method.ContainingType.IsSealed && !method.IsStatic)
                {
                    if (!method.IsVirtual || !IsProtected(method))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ModifiersRule, method.Locations[0], method.Name,
                            evnt.Name));
                    }
                }
            }
        }

        private static bool IsProtected([NotNull] IMethodSymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Protected ||
                method.DeclaredAccessibility == Accessibility.ProtectedAndInternal;
        }

        private sealed class LocalAssignmentWalker : OperationWalker
        {
            [NotNull]
            private readonly ILocalSymbol local;

            [CanBeNull]
            public IEventSymbol Event { get; private set; }

            public LocalAssignmentWalker([NotNull] ILocalSymbol local)
            {
                Guard.NotNull(local, nameof(local));
                this.local = local;
            }

            public override void VisitAssignmentExpression([NotNull] IAssignmentExpression operation)
            {
                var targetLocal = operation.Target as ILocalReferenceExpression;
                if (targetLocal != null && local.Equals(targetLocal.Local))
                {
                    TrySetEvent(operation.Value);
                }

                base.VisitAssignmentExpression(operation);
            }

            public override void VisitVariableDeclaration([NotNull] IVariableDeclaration operation)
            {
                if (local.Equals(operation.Variable))
                {
                    TrySetEvent(operation.InitialValue);
                }

                base.VisitVariableDeclaration(operation);
            }

            private void TrySetEvent([CanBeNull] IOperation assignedValue)
            {
                var eventReference = assignedValue as IEventReferenceExpression;
                if (eventReference != null)
                {
                    Event = eventReference.Event;
                }
            }
        }
    }
}