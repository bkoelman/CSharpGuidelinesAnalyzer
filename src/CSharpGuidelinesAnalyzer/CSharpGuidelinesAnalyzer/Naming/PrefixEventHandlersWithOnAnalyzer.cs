using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PrefixEventHandlersWithOnAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1738";

        private const string Title = "Event handlers should be named according to a pattern";
        private const string MessageFormat = "Method '{0}' that handles event '{1}' should be renamed to '{2}'.";
        private const string Description = "Prefix an event handler with On.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterOperationAction(AnalyzeEventAssignment, OperationKind.EventAssignmentExpression);
                }
            });
        }

        private void AnalyzeEventAssignment(OperationAnalysisContext context)
        {
            var assignment = (IEventAssignmentExpression) context.Operation;

            if (!assignment.Adds)
            {
                return;
            }

            var binding = assignment.HandlerValue as IMethodBindingExpression;
            if (binding?.Method != null)
            {
                string eventTargetName = GetEventTargetName(assignment.EventInstance);
                string handlerNameExpected = "On" + eventTargetName + assignment.Event.Name;

                string handlerNameActual = binding.Method.Name;
                if (handlerNameActual != handlerNameExpected)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, binding.Syntax.GetLocation(), handlerNameActual,
                        assignment.Event.Name, handlerNameExpected));
                }
            }
        }

        [NotNull]
        private static string GetEventTargetName([NotNull] IOperation eventInstance)
        {
            bool isEventLocal = eventInstance is IInstanceReferenceExpression;

            if (!isEventLocal)
            {
                IdentifierInfo info = AnalysisUtilities.TryGetIdentifierInfo(eventInstance);
                if (info != null)
                {
                    return MakeCamelCase(info.Name);
                }
            }

            return string.Empty;
        }

        [NotNull]
        private static string MakeCamelCase([NotNull] string identifierName)
        {
            string noUnderscorePrefix = identifierName.StartsWith("_", StringComparison.Ordinal)
                ? identifierName.Substring(1)
                : identifierName;

            string camelCased = noUnderscorePrefix.Length > 0 && char.IsLower(noUnderscorePrefix[0])
                ? char.ToUpper(noUnderscorePrefix[0]) + noUnderscorePrefix.Substring(1)
                : noUnderscorePrefix;

            return camelCased;
        }
    }
}