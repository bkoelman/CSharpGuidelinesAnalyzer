using System.Collections.Immutable;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RaiseEventsFromProtectedVirtualMethodsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1225";

        private const string Title =
            "Methods that raise an event should be protected virtual and be named 'On' followed by event name.";

        private const string KindMessageFormat = "Event '{0}' should be raised from a regular method.";

        private const string ModifiersMessageFormat =
            "Method '{0}' raises event '{1}', so should be protected and virtual.";

        private const string NameMessageFormat = "Method '{0}' raises event '{1}', so it should be named '{2}'.";
        private const string Description = "Use a protected virtual method to raise each event.";
        private const string Category = "Miscellaneous Design";

        [NotNull]
        private static readonly DiagnosticDescriptor KindRule = new DiagnosticDescriptor(DiagnosticId, Title,
            KindMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ModifiersRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ModifiersMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor NameRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NameMessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(KindRule, ModifiersRule, NameRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeInvocation),
                OperationKind.InvocationExpression);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationExpression) context.Operation;

            if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
            {
                AnalyzeEventInvocation(context, invocation);
            }
        }

        private void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] IInvocationExpression invocation)
        {
            IEventSymbol evnt = TryGetEvent(invocation.Instance, context.ContainingSymbol as IMethodSymbol, context);
            if (evnt != null)
            {
                IMethodSymbol containingMethod = TryGetContainingMethod(invocation, context);
                AnalyzeContainingMethod(containingMethod, evnt, context);
            }
        }

        [CanBeNull]
        private IEventSymbol TryGetEvent([NotNull] IOperation operation, [CanBeNull] IMethodSymbol containingMethod,
            OperationAnalysisContext context)
        {
            return TryGetEventForInvocation(operation) ??
                TryGetEventForNullConditionalAccessInvocation(operation, context.Compilation, context.CancellationToken) ??
                TryGetEventForLocalCopy(operation, containingMethod, context);
        }

        [CanBeNull]
        private IEventSymbol TryGetEventForInvocation([NotNull] IOperation operation)
        {
            var eventReference = operation as IEventReferenceExpression;
            return eventReference?.Event;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventForNullConditionalAccessInvocation([NotNull] IOperation operation,
            [NotNull] Compilation compilation, CancellationToken cancellationToken)
        {
            var conditionalAccess = operation as IConditionalAccessInstanceExpression;
            if (conditionalAccess != null)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
                return model.GetSymbolInfo(operation.Syntax, cancellationToken).Symbol as IEventSymbol;
            }

            return null;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventForLocalCopy([NotNull] IOperation operation,
            [CanBeNull] IMethodSymbol containingMethod, OperationAnalysisContext context)
        {
            var local = operation as ILocalReferenceExpression;

            return local != null && containingMethod != null
                ? TryGetEventFromMethodStatements(containingMethod, local.Local, context)
                : null;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventFromMethodStatements([NotNull] IMethodSymbol containingMethod,
            [NotNull] ILocalSymbol local, OperationAnalysisContext context)
        {
            IOperation body = containingMethod.TryGetOperationBlockForMethod(context.Compilation,
                context.CancellationToken);
            if (body != null)
            {
                var walker = new LocalAssignmentWalker(local);
                walker.Visit(body);

                return walker.Event;
            }

            return null;
        }

        [CanBeNull]
        private IMethodSymbol TryGetContainingMethod([NotNull] IInvocationExpression invocation,
            OperationAnalysisContext context)
        {
            SemanticModel model = context.Compilation.GetSemanticModel(invocation.Syntax.SyntaxTree);
            return model.GetEnclosingSymbol(invocation.Syntax.GetLocation().SourceSpan.Start) as IMethodSymbol;
        }

        private void AnalyzeContainingMethod([CanBeNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            if (method == null || method.MethodKind != MethodKind.Ordinary)
            {
                Location location = method != null ? method.Locations[0] : context.Operation.Syntax.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(KindRule, location, evnt.Name));
            }
            else
            {
                AnalyzeMethodName(method, evnt, context);
                AnalyzeMethodSignature(method, evnt, context);
            }
        }

        private static void AnalyzeMethodName([NotNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            string nameExpected = "On" + evnt.Name;
            if (method.Name != nameExpected)
            {
                context.ReportDiagnostic(Diagnostic.Create(NameRule, method.Locations[0], method.Name, evnt.Name,
                    nameExpected));
            }
        }

        private static void AnalyzeMethodSignature([NotNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            if (!method.ContainingType.IsSealed && !method.IsStatic)
            {
                if (!method.IsVirtual || !IsProtected(method))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ModifiersRule, method.Locations[0], method.Name,
                        evnt.Name));
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