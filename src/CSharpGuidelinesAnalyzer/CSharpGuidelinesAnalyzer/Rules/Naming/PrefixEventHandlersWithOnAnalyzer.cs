using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
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
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeEventAssignment),
                OperationKind.EventAssignmentExpression);
        }

        private void AnalyzeEventAssignment(OperationAnalysisContext context)
        {
            var assignment = (IEventAssignmentExpression)context.Operation;

            if (!assignment.Adds)
            {
                return;
            }

            var binding = assignment.HandlerValue as IMethodBindingExpression;
            if (binding?.Method != null)
            {
                AnalyzeEventAssignmentMethod(binding, assignment, context);
            }
        }

        private static void AnalyzeEventAssignmentMethod([NotNull] IMethodBindingExpression binding,
            [NotNull] IEventAssignmentExpression assignment, OperationAnalysisContext context)
        {
            string eventTargetName = GetEventTargetName(assignment.EventInstance);
            string handlerNameExpected = string.Concat(eventTargetName, "On", assignment.Event.Name);

            string handlerNameActual = binding.Method.Name;
            if (handlerNameActual != handlerNameExpected)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, binding.Syntax.GetLocation(), handlerNameActual,
                    assignment.Event.Name, handlerNameExpected));
            }
        }

        [NotNull]
        private static string GetEventTargetName([NotNull] IOperation eventInstance)
        {
            bool isEventLocal = eventInstance is IInstanceReferenceExpression;

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
    }
}
