using System;
using System.Collections.Immutable;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RaiseEventFromProtectedVirtualMethodAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Method that raises an event should be protected virtual and be named 'On' followed by event name";
    private const string KindMessageFormat = "Event '{0}' should be raised from a regular method";
    private const string ModifiersMessageFormat = "Method '{0}' raises event '{1}', so it should be protected and virtual";
    private const string NameMessageFormat = "Method '{0}' raises event '{1}', so it should be named '{2}'";
    private const string Description = "Use a protected virtual method to raise each event.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1225";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    [NotNull]
    private static readonly DiagnosticDescriptor KindRule = new(DiagnosticId, Title, KindMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly DiagnosticDescriptor ModifiersRule = new(DiagnosticId, Title, ModifiersMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly DiagnosticDescriptor NameRule = new(DiagnosticId, Title, NameMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<MethodKind> RegularMethodKinds = new[]
    {
        MethodKind.Ordinary,
        MethodKind.ExplicitInterfaceImplementation
    }.ToImmutableArray();

    [NotNull]
    private static readonly Action<OperationAnalysisContext> AnalyzeInvocationAction = context => context.SkipInvalid(AnalyzeInvocation);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(KindRule, ModifiersRule, NameRule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeInvocationAction, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
        {
            AnalyzeEventInvocation(context, invocation);
        }
    }

    private static void AnalyzeEventInvocation(OperationAnalysisContext context, [NotNull] IInvocationOperation invocation)
    {
        IEventSymbol @event = TryGetEvent(invocation.Instance, context.ContainingSymbol as IMethodSymbol, context);

        if (@event != null)
        {
            IMethodSymbol containingMethod = invocation.TryGetContainingMethod(context.Compilation);
            AnalyzeContainingMethod(containingMethod, @event, context);
        }
    }

    [CanBeNull]
    private static IEventSymbol TryGetEvent([NotNull] IOperation operation, [CanBeNull] IMethodSymbol containingMethod, OperationAnalysisContext context)
    {
        return TryGetEventForInvocation(operation) ??
            TryGetEventForNullConditionalAccessInvocation(operation, context.Compilation, context.CancellationToken) ??
            TryGetEventForLocalCopy(operation, containingMethod, context);
    }

    [CanBeNull]
    private static IEventSymbol TryGetEventForInvocation([NotNull] IOperation operation)
    {
        var eventReference = operation as IEventReferenceOperation;
        return eventReference?.Event;
    }

    [CanBeNull]
    private static IEventSymbol TryGetEventForNullConditionalAccessInvocation([NotNull] IOperation operation, [NotNull] Compilation compilation,
        CancellationToken cancellationToken)
    {
        if (operation is IConditionalAccessInstanceOperation)
        {
            SemanticModel model = operation.GetSemanticModel(compilation);
            return model.GetSymbolInfo(operation.Syntax, cancellationToken).Symbol as IEventSymbol;
        }

        return null;
    }

    [CanBeNull]
    private static IEventSymbol TryGetEventForLocalCopy([NotNull] IOperation operation, [CanBeNull] IMethodSymbol containingMethod,
        OperationAnalysisContext context)
    {
        return operation is ILocalReferenceOperation local && containingMethod != null
            ? TryGetEventFromMethodStatements(containingMethod, local.Local, context)
            : null;
    }

    [CanBeNull]
    private static IEventSymbol TryGetEventFromMethodStatements([NotNull] IMethodSymbol containingMethod, [NotNull] ILocalSymbol local,
        OperationAnalysisContext context)
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

    private static void AnalyzeContainingMethod([CanBeNull] IMethodSymbol method, [NotNull] IEventSymbol @event, OperationAnalysisContext context)
    {
        if (method == null || !RegularMethodKinds.Contains(method.MethodKind))
        {
            Location location = method != null && !method.IsSynthesized() ? method.Locations[0] : context.Operation.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(KindRule, location, @event.Name);
            context.ReportDiagnostic(diagnostic);
        }
        else
        {
            if (!method.IsSynthesized())
            {
                if (!AnalyzeMethodName(method, @event, context))
                {
                    AnalyzeMethodSignature(method, @event, context);
                }
            }
        }
    }

    private static bool AnalyzeMethodName([NotNull] IMethodSymbol method, [NotNull] IEventSymbol @event, OperationAnalysisContext context)
    {
        string nameExpected = string.Concat("On", @event.Name);
        string nameActual = method.MemberNameWithoutExplicitInterfacePrefix();

        if (nameActual != nameExpected)
        {
            var diagnostic = Diagnostic.Create(NameRule, method.Locations[0], method.Name, @event.Name, nameExpected);
            context.ReportDiagnostic(diagnostic);

            return true;
        }

        return false;
    }

    private static void AnalyzeMethodSignature([NotNull] IMethodSymbol method, [NotNull] IEventSymbol @event, OperationAnalysisContext context)
    {
        if (!method.ContainingType.IsSealed && !method.IsStatic && method.MethodKind != MethodKind.ExplicitInterfaceImplementation)
        {
            if (!method.IsVirtual || !IsProtected(method))
            {
                var diagnostic = Diagnostic.Create(ModifiersRule, method.Locations[0], method.Name, @event.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsProtected([NotNull] IMethodSymbol method)
    {
        return method.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedAndInternal;
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
            if (operation.Target is ILocalReferenceOperation targetLocal && local.IsEqualTo(targetLocal.Local))
            {
                TrySetEvent(operation.Value);
            }

            base.VisitSimpleAssignment(operation);
        }

        public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
        {
            if (local.IsEqualTo(operation.Symbol))
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