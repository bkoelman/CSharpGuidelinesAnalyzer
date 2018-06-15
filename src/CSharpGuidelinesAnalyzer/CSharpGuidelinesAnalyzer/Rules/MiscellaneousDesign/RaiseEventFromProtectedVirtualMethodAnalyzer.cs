using System.Collections.Immutable;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RaiseEventFromProtectedVirtualMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1225";

        private const string Title =
            "Method that raises an event should be protected virtual and be named 'On' followed by event name";

        private const string KindMessageFormat = "Event '{0}' should be raised from a regular method.";
        private const string ModifiersMessageFormat = "Method '{0}' raises event '{1}', so it should be protected and virtual.";
        private const string NameMessageFormat = "Method '{0}' raises event '{1}', so it should be named '{2}'.";
        private const string Description = "Use a protected virtual method to raise each event.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor KindRule = new DiagnosticDescriptor(DiagnosticId, Title, KindMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ModifiersRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ModifiersMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor NameRule = new DiagnosticDescriptor(DiagnosticId, Title, NameMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(KindRule, ModifiersRule, NameRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeInvocation), OperationKind.Invocation);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
            {
                AnalyzeEventInvocation(context, invocation);
            }
        }

        private void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] IInvocationOperation invocation)
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
            var eventReference = operation as IEventReferenceOperation;
            return eventReference?.Event;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventForNullConditionalAccessInvocation([NotNull] IOperation operation,
            [NotNull] Compilation compilation, CancellationToken cancellationToken)
        {
            if (operation is IConditionalAccessInstanceOperation)
            {
                SemanticModel model = compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
                return model.GetSymbolInfo(operation.Syntax, cancellationToken).Symbol as IEventSymbol;
            }

            return null;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventForLocalCopy([NotNull] IOperation operation, [CanBeNull] IMethodSymbol containingMethod,
            OperationAnalysisContext context)
        {
            return operation is ILocalReferenceOperation local && containingMethod != null
                ? TryGetEventFromMethodStatements(containingMethod, local.Local, context)
                : null;
        }

        [CanBeNull]
        private IEventSymbol TryGetEventFromMethodStatements([NotNull] IMethodSymbol containingMethod,
            [NotNull] ILocalSymbol local, OperationAnalysisContext context)
        {
            IOperation body = containingMethod.TryGetOperationBlockForMethod(context.Compilation, context.CancellationToken);
            if (body != null)
            {
                var walker = new LocalAssignmentWalker(local);
                walker.Visit(body);

                return walker.Event;
            }

            return null;
        }

        [CanBeNull]
        private IMethodSymbol TryGetContainingMethod([NotNull] IInvocationOperation invocation, OperationAnalysisContext context)
        {
            SemanticModel model = context.Compilation.GetSemanticModel(invocation.Syntax.SyntaxTree);
            return model.GetEnclosingSymbol(invocation.Syntax.GetLocation().SourceSpan.Start) as IMethodSymbol;
        }

        private void AnalyzeContainingMethod([CanBeNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            if (method == null || method.MethodKind != MethodKind.Ordinary)
            {
                Location location = method != null && !method.IsSynthesized()
                    ? method.Locations[0]
                    : context.Operation.Syntax.GetLocation();

                context.ReportDiagnostic(Diagnostic.Create(KindRule, location, evnt.Name));
            }
            else
            {
                if (!method.IsSynthesized())
                {
                    if (!AnalyzeMethodName(method, evnt, context))
                    {
                        AnalyzeMethodSignature(method, evnt, context);
                    }
                }
            }
        }

        private static bool AnalyzeMethodName([NotNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            string nameExpected = string.Concat("On", evnt.Name);
            if (method.Name != nameExpected)
            {
                context.ReportDiagnostic(Diagnostic.Create(NameRule, method.Locations[0], method.Name, evnt.Name, nameExpected));
                return true;
            }

            return false;
        }

        private static void AnalyzeMethodSignature([NotNull] IMethodSymbol method, [NotNull] IEventSymbol evnt,
            OperationAnalysisContext context)
        {
            if (!method.ContainingType.IsSealed && !method.IsStatic)
            {
                if (!method.IsVirtual || !IsProtected(method))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ModifiersRule, method.Locations[0], method.Name, evnt.Name));
                }
            }
        }

        private static bool IsProtected([NotNull] IMethodSymbol method)
        {
            return method.DeclaredAccessibility == Accessibility.Protected ||
                method.DeclaredAccessibility == Accessibility.ProtectedAndInternal;
        }

        private sealed class LocalAssignmentWalker : ExplicitOperationWalker
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

            public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
            {
                if (operation.Target is ILocalReferenceOperation targetLocal && local.Equals(targetLocal.Local))
                {
                    TrySetEvent(operation.Value);
                }

                base.VisitSimpleAssignment(operation);
            }

            public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
            {
                if (local.Equals(operation.Symbol))
                {
                    IVariableInitializerOperation initializer = operation.GetVariableInitializer();
                    if (initializer != null)
                    {
                        TrySetEvent(initializer.Value);
                    }
                }

                base.VisitVariableDeclarator(operation);
            }

            private void TrySetEvent([CanBeNull] IOperation assignedValue)
            {
                if (assignedValue is IEventReferenceOperation eventReference)
                {
                    Event = eventReference.Event;
                }
            }
        }
    }
}
