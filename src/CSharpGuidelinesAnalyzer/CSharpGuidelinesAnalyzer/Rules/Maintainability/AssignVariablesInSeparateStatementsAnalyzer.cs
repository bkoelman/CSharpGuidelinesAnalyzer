using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AssignVariablesInSeparateStatementsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1522";

        private const string Title = "Assign each property, field, parameter or variable in a separate statement";
        private const string MessageFormat = "{0} are assigned in a single statement.";
        private const string Description = "Assign each variable in a separate statement.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private readonly ImmutableArray<OperationKind> statementKinds =
            ImmutableArray.Create(OperationKind.VariableDeclarationStatement, OperationKind.SwitchStatement,
                OperationKind.IfStatement, OperationKind.LoopStatement, OperationKind.ThrowStatement,
                OperationKind.ReturnStatement, OperationKind.LockStatement, OperationKind.UsingStatement,
                OperationKind.YieldReturnStatement, OperationKind.ExpressionStatement);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(AnalyzeStatement, statementKinds);
                }
            });
        }

        private void AnalyzeStatement(OperationAnalysisContext context)
        {
            var statementWalker = new StatementWalker();
            statementWalker.Visit(context.Operation);

            context.CancellationToken.ThrowIfCancellationRequested();

            if (statementWalker.IdentifiersAssigned.Count > 1)
            {
                string identifiers = FormatIdentifierList(statementWalker.IdentifiersAssigned.ToList());
                Location location = context.Operation.Syntax.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, identifiers));
            }
        }

        [NotNull]
        private string FormatIdentifierList([NotNull] [ItemNotNull] IList<string> variableNames)
        {
            var messageBuilder = new StringBuilder();

            for (int index = 0; index < variableNames.Count - 1; index++)
            {
                string variableName = variableNames[index];

                if (messageBuilder.Length > 0)
                {
                    messageBuilder.Append(", ");
                }

                messageBuilder.Append("'" + variableName + "'");
            }

            messageBuilder.Append(" and '");
            messageBuilder.Append(variableNames[variableNames.Count - 1] + "'");

            return messageBuilder.ToString();
        }

        private sealed class StatementWalker : OperationWalker
        {
            [NotNull]
            [ItemNotNull]
            public ICollection<string> IdentifiersAssigned { get; } = new HashSet<string>();

            public override void VisitVariableDeclaration([NotNull] IVariableDeclaration operation)
            {
                if (operation.InitialValue != null)
                {
                    IdentifiersAssigned.Add(operation.Variable.Name);
                }

                base.VisitVariableDeclaration(operation);
            }

            public override void VisitAssignmentExpression([NotNull] IAssignmentExpression operation)
            {
                RegisterAssignment(operation.Target);

                base.VisitAssignmentExpression(operation);
            }

            private void RegisterAssignment([NotNull] IOperation operation)
            {
                IdentifierInfo identifierInfo = operation.TryGetIdentifierInfo();
                if (identifierInfo != null)
                {
                    IdentifiersAssigned.Add(identifierInfo.LongName);
                }
            }

            public override void VisitIfStatement([NotNull] IIfStatement operation)
            {
                Visit(operation.Condition);
            }

            public override void VisitForLoopStatement([NotNull] IForLoopStatement operation)
            {
                VisitArray(operation.Before);
                Visit(operation.Condition);
                VisitArray(operation.AtLoopBottom);
            }

            public override void VisitForEachLoopStatement([NotNull] IForEachLoopStatement operation)
            {
                Visit(operation.Collection);
            }

            public override void VisitWhileUntilLoopStatement([NotNull] IWhileUntilLoopStatement operation)
            {
                Visit(operation.Condition);
            }

            public override void VisitLockStatement([NotNull] ILockStatement operation)
            {
                Visit(operation.LockedObject);
            }

            public override void VisitUsingStatement([NotNull] IUsingStatement operation)
            {
                Visit(operation.Declaration);
                Visit(operation.Value);
            }

            public override void VisitSwitchCase([NotNull] ISwitchCase operation)
            {
                VisitArray(operation.Clauses);
            }

            private void VisitArray([CanBeNull] [ItemNotNull] IEnumerable<IOperation> operations)
            {
                if (operations != null)
                {
                    foreach (IOperation operation in operations)
                    {
                        Visit(operation);
                    }
                }
            }
        }
    }
}