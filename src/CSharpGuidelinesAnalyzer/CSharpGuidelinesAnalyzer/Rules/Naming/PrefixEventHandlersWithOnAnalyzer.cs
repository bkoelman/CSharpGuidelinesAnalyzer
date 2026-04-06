using System.Collections.Immutable;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using TypeInfo = System.Reflection.TypeInfo;

namespace CSharpGuidelinesAnalyzer.Rules.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrefixEventHandlersWithOnAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Event handlers should be named according to the pattern '(InstanceName)On(EventName)'";
    private const string MessageFormat = "{0} '{1}' that handles event '{2}' should be renamed to '{3}'";
    private const string Description = "Prefix an event handler with \"On\".";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1738";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeEventAssignment, OperationKind.EventAssignment);
    }

    private static void AnalyzeEventAssignment(OperationAnalysisContext context)
    {
        var assignment = new PortableEventAssignmentOperation((IEventAssignmentOperation)context.Operation);

        if (!assignment.Adds)
        {
            return;
        }

        var delegateCreation = assignment.HandlerValue as IDelegateCreationOperation;
        var reference = delegateCreation?.Target as IMethodReferenceOperation;

        if (reference?.Method != null)
        {
            AnalyzeEventAssignmentMethod(reference, assignment, context);
        }
    }

    private static void AnalyzeEventAssignmentMethod(IMethodReferenceOperation binding, PortableEventAssignmentOperation assignment,
        OperationAnalysisContext context)
    {
        if (assignment.EventReference != null)
        {
            string eventTargetName = GetEventTargetName(assignment.EventReference, binding.Method);
            string handlerNameExpected = string.Concat(eventTargetName, "On", assignment.EventReference.Event.Name);

            string handlerNameActual = binding.Method.Name;

            if (handlerNameActual != handlerNameExpected)
            {
                Location location = binding.Syntax.GetLocation();
                string kindText = binding.Method.GetKind();

                var diagnostic = Diagnostic.Create(Rule, location, kindText, handlerNameActual, assignment.EventReference.Event.Name, handlerNameExpected);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static string GetEventTargetName(IEventReferenceOperation eventReference, IMethodSymbol targetMethod)
    {
        return eventReference.Instance != null ? GetInstanceEventTargetName(eventReference.Instance) : GetStaticEventTargetName(eventReference, targetMethod);
    }

    private static string GetInstanceEventTargetName(IOperation eventInstance)
    {
        bool isEventLocal = eventInstance is IInstanceReferenceOperation;

        if (!isEventLocal)
        {
            IdentifierInfo? info = eventInstance.TryGetIdentifierInfo();

            if (info != null)
            {
                return MakeCamelCaseWithoutUnderscorePrefix(info.Name.ShortName);
            }
        }

        return string.Empty;
    }

    private static string MakeCamelCaseWithoutUnderscorePrefix(string identifierName)
    {
        string noUnderscorePrefix = RemoveUnderscorePrefix(identifierName);
        return ToCamelCase(noUnderscorePrefix);
    }

    private static string RemoveUnderscorePrefix(string identifierName)
    {
        return identifierName.StartsWith("_", StringComparison.Ordinal) ? identifierName.Substring(1) : identifierName;
    }

    private static string ToCamelCase(string identifierName)
    {
        return identifierName.Length > 0 && char.IsLower(identifierName[0]) ? char.ToUpper(identifierName[0]) + identifierName.Substring(1) : identifierName;
    }

    private static string GetStaticEventTargetName(IEventReferenceOperation eventReference, IMethodSymbol targetMethod)
    {
        INamedTypeSymbol eventContainingType = eventReference.Event.ContainingType;

        bool isEventLocal = eventContainingType.IsEqualTo(targetMethod.ContainingType);
        return isEventLocal ? string.Empty : eventContainingType.Name;
    }

    private sealed class PortableEventAssignmentOperation
    {
        private static readonly MethodInfo EventReferencePropertyGetMethod = ResolveEventReferencePropertyGetMethod();

        private readonly IEventAssignmentOperation innerOperation;

        public IEventReferenceOperation? EventReference => InvokeEventReferencePropertyGetMethod();

        public IOperation HandlerValue => innerOperation.HandlerValue;

        public bool Adds => innerOperation.Adds;

        public PortableEventAssignmentOperation(IEventAssignmentOperation operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            innerOperation = operation;
        }

        private static MethodInfo ResolveEventReferencePropertyGetMethod()
        {
            // Breaking change in Microsoft.CodeAnalysis v2.9:
            // type of IEventAssignmentOperation.EventReference was changed from IEventReferenceOperation to IOperation.

            PropertyInfo propertyInfo = typeof(IEventAssignmentOperation).GetRuntimeProperty("EventReference");
            return propertyInfo.GetMethod;
        }

        private IEventReferenceOperation? InvokeEventReferencePropertyGetMethod()
        {
            object propertyValue = EventReferencePropertyGetMethod.Invoke(innerOperation, []);
            TypeInfo propertyType = propertyValue.GetType().GetTypeInfo();

            return typeof(IEventReferenceOperation).GetTypeInfo().IsAssignableFrom(propertyType) ? (IEventReferenceOperation)propertyValue : null;
        }
    }
}
