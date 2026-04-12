using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    private static readonly DiagnosticDescriptor KindRule = new(DiagnosticId, Title, KindMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor ModifiersRule = new(DiagnosticId, Title, ModifiersMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor NameRule = new(DiagnosticId, Title, NameMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly ImmutableArray<MethodKind> RegularMethodKinds = new[]
    {
        MethodKind.Ordinary,
        MethodKind.ExplicitInterfaceImplementation
    }.ToImmutableArray();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(KindRule, ModifiersRule, NameRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
        {
            AnalyzeEventInvocation(context, invocation);
        }
    }

    private static void AnalyzeEventInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        if (invocation.Instance != null)
        {
            IEventSymbol? @event = TryGetEvent(invocation.Instance, context.ContainingSymbol as IMethodSymbol, context);

            if (@event != null)
            {
                IMethodSymbol? containingMethod = invocation.TryGetContainingMethod(context.Compilation);
                AnalyzeContainingMethod(containingMethod, @event, context);
            }
        }
    }

    private static IEventSymbol? TryGetEvent(IOperation operation, IMethodSymbol? containingMethod, OperationAnalysisContext context)
    {
        return TryGetEventForInvocation(operation) ?? TryGetEventForNullConditionalAccessInvocation(operation, context.CancellationToken) ??
            TryGetEventForLocalCopy(operation, containingMethod, context);
    }

    private static IEventSymbol? TryGetEventForInvocation(IOperation operation)
    {
        var eventReference = operation as IEventReferenceOperation;
        return eventReference?.Event;
    }

    private static IEventSymbol? TryGetEventForNullConditionalAccessInvocation(IOperation operation, CancellationToken cancellationToken)
    {
        if (operation is IConditionalAccessInstanceOperation)
        {
            return operation.SemanticModel?.GetSymbolInfo(operation.Syntax, cancellationToken).Symbol as IEventSymbol;
        }

        return null;
    }

    private static IEventSymbol? TryGetEventForLocalCopy(IOperation operation, IMethodSymbol? containingMethod, OperationAnalysisContext context)
    {
        return operation is ILocalReferenceOperation local && containingMethod != null
            ? TryGetEventFromMethodStatements(containingMethod, local.Local, context)
            : null;
    }

    private static IEventSymbol? TryGetEventFromMethodStatements(IMethodSymbol containingMethod, ILocalSymbol local, OperationAnalysisContext context)
    {
        IOperation? body = containingMethod.TryGetOperationBlockForMethod(context.Compilation, context.CancellationToken);

        if (body != null)
        {
            var walker = new LocalAssignmentWalker(local);
            walker.Visit(body);

            return walker.Event;
        }

        return null;
    }

    private static void AnalyzeContainingMethod(IMethodSymbol? method, IEventSymbol @event, OperationAnalysisContext context)
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

    private static bool AnalyzeMethodName(IMethodSymbol method, IEventSymbol @event, OperationAnalysisContext context)
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

    private static void AnalyzeMethodSignature(IMethodSymbol method, IEventSymbol @event, OperationAnalysisContext context)
    {
        if (method is { NullableContainingType.IsSealed: false, IsStatic: false } && method.MethodKind != MethodKind.ExplicitInterfaceImplementation)
        {
            if (!method.IsVirtual || !IsProtected(method))
            {
                var diagnostic = Diagnostic.Create(ModifiersRule, method.Locations[0], method.Name, @event.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsProtected(IMethodSymbol method)
    {
        return method.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedAndInternal;
    }

    private sealed class LocalAssignmentWalker : ExplicitOperationWalker
    {
        private readonly ILocalSymbol local;

        public IEventSymbol? Event { get; private set; }

        public LocalAssignmentWalker(ILocalSymbol local)
        {
            ArgumentNullException.ThrowIfNull(local);
            this.local = local;
        }

        public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
        {
            if (operation.Target is ILocalReferenceOperation targetLocal && local.IsEqualTo(targetLocal.Local))
            {
                TrySetEvent(operation.Value);
            }

            base.VisitSimpleAssignment(operation);
        }

        public override void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
        {
            if (local.IsEqualTo(operation.Symbol))
            {
                IVariableInitializerOperation? initializer = operation.GetVariableInitializer();

                if (initializer != null)
                {
                    TrySetEvent(initializer.Value);
                }
            }

            base.VisitVariableDeclarator(operation);
        }

        private void TrySetEvent(IOperation? assignedValue)
        {
            if (assignedValue is IEventReferenceOperation eventReference)
            {
                Event = eventReference.Event;
            }
        }
    }
}
