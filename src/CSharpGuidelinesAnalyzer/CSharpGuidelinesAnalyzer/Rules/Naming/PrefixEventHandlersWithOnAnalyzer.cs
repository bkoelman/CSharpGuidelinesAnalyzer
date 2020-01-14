using System;
using System.Collections.Immutable;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PrefixEventHandlersWithOnAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Event handlers should be named according to the pattern '(InstanceName)On(EventName)'";
        private const string MessageFormat = "{0} '{1}' that handles event '{2}' should be renamed to '{3}'.";
        private const string Description = "Prefix an event handler with \"On\".";

        public const string DiagnosticId = "AV1738";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeEventAssignmentAction = context =>
            context.SkipInvalid(AnalyzeEventAssignment);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeEventAssignmentAction, OperationKind.EventAssignment);
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

        private static void AnalyzeEventAssignmentMethod([NotNull] IMethodReferenceOperation binding,
            [NotNull] PortableEventAssignmentOperation assignment, OperationAnalysisContext context)
        {
            if (assignment.EventReference != null)
            {
                string eventTargetName = GetEventTargetName(assignment.EventReference, binding.Method);
                string handlerNameExpected = string.Concat(eventTargetName, "On", assignment.EventReference.Event.Name);

                string handlerNameActual = binding.Method.Name;

                if (handlerNameActual != handlerNameExpected)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, binding.Syntax.GetLocation(), binding.Method.GetKind(),
                        handlerNameActual, assignment.EventReference.Event.Name, handlerNameExpected));
                }
            }
        }

        [NotNull]
        private static string GetEventTargetName([NotNull] IEventReferenceOperation eventReference,
            [NotNull] IMethodSymbol targetMethod)
        {
            return eventReference.Instance != null
                ? GetInstanceEventTargetName(eventReference.Instance)
                : GetStaticEventTargetName(eventReference, targetMethod);
        }

        [NotNull]
        private static string GetInstanceEventTargetName([NotNull] IOperation eventInstance)
        {
            bool isEventLocal = eventInstance is IInstanceReferenceOperation;

            if (!isEventLocal)
            {
                IdentifierInfo info = eventInstance.TryGetIdentifierInfo();

                if (info != null)
                {
                    return MakeCamelCaseWithoutUnderscorePrefix(info.Name.ShortName);
                }
            }

            return string.Empty;
        }

        [NotNull]
        private static string MakeCamelCaseWithoutUnderscorePrefix([NotNull] string identifierName)
        {
            string noUnderscorePrefix = RemoveUnderscorePrefix(identifierName);
            return ToCamelCase(noUnderscorePrefix);
        }

        [NotNull]
        private static string RemoveUnderscorePrefix([NotNull] string identifierName)
        {
            return identifierName.StartsWith("_", StringComparison.Ordinal) ? identifierName.Substring(1) : identifierName;
        }

        [NotNull]
        private static string ToCamelCase([NotNull] string identifierName)
        {
            return identifierName.Length > 0 && char.IsLower(identifierName[0])
                ? char.ToUpper(identifierName[0]) + identifierName.Substring(1)
                : identifierName;
        }

        [NotNull]
        private static string GetStaticEventTargetName([NotNull] IEventReferenceOperation eventReference,
            [NotNull] IMethodSymbol targetMethod)
        {
            INamedTypeSymbol eventContainingType = eventReference.Event.ContainingType;

            bool isEventLocal = eventContainingType.IsEqualTo(targetMethod.ContainingType);
            return isEventLocal ? string.Empty : eventContainingType.Name;
        }

        private sealed class PortableEventAssignmentOperation
        {
            [NotNull]
            private static readonly MethodInfo EventReferencePropertyGetMethod = ResolveEventReferencePropertyGetMethod();

            [NotNull]
            private readonly IEventAssignmentOperation innerOperation;

            [CanBeNull]
            public IEventReferenceOperation EventReference => InvokeEventReferencePropertyGetMethod();

            [NotNull]
            public IOperation HandlerValue => innerOperation.HandlerValue;

            public bool Adds => innerOperation.Adds;

            public PortableEventAssignmentOperation([NotNull] IEventAssignmentOperation operation)
            {
                Guard.NotNull(operation, nameof(operation));
                innerOperation = operation;
            }

            [NotNull]
            private static MethodInfo ResolveEventReferencePropertyGetMethod()
            {
                // Breaking change in Microsoft.CodeAnalysis v2.9:
                // type of IEventAssignmentOperation.EventReference was changed from IEventReferenceOperation to IOperation.

                PropertyInfo propertyInfo = typeof(IEventAssignmentOperation).GetRuntimeProperty("EventReference");
                return propertyInfo.GetMethod;
            }

            [CanBeNull]
            private IEventReferenceOperation InvokeEventReferencePropertyGetMethod()
            {
                object propertyValue = EventReferencePropertyGetMethod.Invoke(innerOperation, Array.Empty<object>());

                return typeof(IEventReferenceOperation).GetTypeInfo().IsAssignableFrom(propertyValue.GetType().GetTypeInfo())
                    ? (IEventReferenceOperation)propertyValue
                    : null;
            }
        }
    }
}
