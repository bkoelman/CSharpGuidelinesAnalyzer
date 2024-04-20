using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssignEachVariableInASeparateStatementAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Assign each property, field, parameter or variable in a separate statement";
    private const string MessageFormat = "{0} are assigned in a single statement";
    private const string Description = "Assign each variable in a separate statement.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1522";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<OperationAnalysisContext> AnalyzeStatementAction = context => context.SkipInvalid(AnalyzeStatement);

    private readonly ImmutableArray<OperationKind> statementKinds = ImmutableArray.Create(OperationKind.VariableDeclarationGroup, OperationKind.Switch,
        OperationKind.Conditional, OperationKind.Loop, OperationKind.Throw, OperationKind.Return, OperationKind.Lock, OperationKind.Using,
        OperationKind.YieldReturn, OperationKind.ExpressionStatement);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeStatementAction, statementKinds);
    }

    private static void AnalyzeStatement(OperationAnalysisContext context)
    {
        if (!context.Operation.IsStatement())
        {
            return;
        }

        if (context.Operation is IForLoopOperation forLoopOperation)
        {
            AnalyzeForLoop(forLoopOperation, context);
        }
        else
        {
            var statementWalker = new StatementWalker(ForLoopSection.None);
            AnalyzeVisitOperation(context.Operation, statementWalker, context);
        }
    }

    private static void AnalyzeForLoop([NotNull] IForLoopOperation forLoopOperation, OperationAnalysisContext context)
    {
        foreach (IOperation beforeOperation in forLoopOperation.Before)
        {
            AnalyzeForLoopSection(ForLoopSection.Before, beforeOperation, context);
        }

        AnalyzeForLoopSection(ForLoopSection.Condition, forLoopOperation.Condition, context);

        foreach (IOperation bottomOperation in forLoopOperation.AtLoopBottom)
        {
            AnalyzeForLoopSection(ForLoopSection.AtLoopBottom, bottomOperation, context);
        }
    }

    private static void AnalyzeForLoopSection(ForLoopSection section, [NotNull] IOperation operation, OperationAnalysisContext context)
    {
        var statementWalker = new StatementWalker(section);
        AnalyzeVisitOperation(operation, statementWalker, context);
    }

    private static void AnalyzeVisitOperation([NotNull] IOperation operation, [NotNull] StatementWalker statementWalker, OperationAnalysisContext context)
    {
        statementWalker.Visit(operation);

        context.CancellationToken.ThrowIfCancellationRequested();

        if (statementWalker.IdentifiersAssigned.Count > 1)
        {
            List<string> identifiersAssigned = statementWalker.IdentifiersAssigned.ToList();
            string identifiers = FormatIdentifierList(identifiersAssigned);
            Location location = GetLocation(operation);

            var diagnostic = Diagnostic.Create(Rule, location, identifiers);
            context.ReportDiagnostic(diagnostic);
        }
    }

    [NotNull]
    private static Location GetLocation([NotNull] IOperation operation)
    {
        return operation.TryGetLocationForKeyword(DoWhileLoopLookupKeywordStrategy.PreferWhileKeyword) ?? operation.Syntax.GetLocation();
    }

    [NotNull]
    private static string FormatIdentifierList([NotNull] [ItemNotNull] IList<string> variableNames)
    {
        var messageBuilder = new StringBuilder();

        for (int index = 0; index < variableNames.Count - 1; index++)
        {
            AppendVariableName(variableNames[index], messageBuilder);
        }

        messageBuilder.Append(" and '");
        messageBuilder.Append(variableNames[variableNames.Count - 1]);
        messageBuilder.Append("'");

        return messageBuilder.ToString();
    }

    private static void AppendVariableName([NotNull] string variableName, [NotNull] StringBuilder messageBuilder)
    {
        if (messageBuilder.Length > 0)
        {
            messageBuilder.Append(", ");
        }

        messageBuilder.Append("'");
        messageBuilder.Append(variableName);
        messageBuilder.Append("'");
    }

    /// <summary>
    /// Collects assignment expressions in the current statement, but without descending into nested statement blocks.
    /// </summary>
    private sealed class StatementWalker(ForLoopSection section) : OperationWalker
    {
        private readonly ForLoopSection section = section;

        [NotNull]
        [ItemNotNull]
        public ISet<string> IdentifiersAssigned { get; } = new HashSet<string>();

        public override void DefaultVisit([NotNull] IOperation operation)
        {
            // Check should be replaced with empty override of VisitWith(IWithOperation operation),
            // after upgrade to recent version of Microsoft.CodeAnalysis.
            if (operation.GetType().Name != "WithOperation")
            {
                base.DefaultVisit(operation);
            }
        }

        public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
        {
            IVariableInitializerOperation initializer = operation.GetVariableInitializer();

            if (initializer != null)
            {
                IdentifiersAssigned.Add(operation.Symbol.Name);
            }

            base.VisitVariableDeclarator(operation);
        }

        public override void VisitAnonymousFunction([NotNull] IAnonymousFunctionOperation operation)
        {
        }

        public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitSimpleAssignment(operation);
        }

        public override void VisitCompoundAssignment([NotNull] ICompoundAssignmentOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitCompoundAssignment(operation);
        }

        public override void VisitIncrementOrDecrement([NotNull] IIncrementOrDecrementOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitIncrementOrDecrement(operation);
        }

        private void RegisterAssignment([NotNull] IOperation operation)
        {
            IdentifierInfo identifierInfo = operation.TryGetIdentifierInfo();

            if (identifierInfo != null)
            {
                IdentifiersAssigned.Add(identifierInfo.Name.LongName);
            }
        }

        public override void VisitAnonymousObjectCreation([NotNull] IAnonymousObjectCreationOperation operation)
        {
        }

        public override void VisitObjectCreation([NotNull] IObjectCreationOperation operation)
        {
        }

        public override void VisitDynamicObjectCreation([NotNull] IDynamicObjectCreationOperation operation)
        {
        }

        public override void VisitTypeParameterObjectCreation([NotNull] ITypeParameterObjectCreationOperation operation)
        {
        }

        public override void VisitConditional([NotNull] IConditionalOperation operation)
        {
            if (operation.IsStatement())
            {
                Visit(operation.Condition);
            }
            else
            {
                base.VisitConditional(operation);
            }
        }

        public override void VisitForLoop([NotNull] IForLoopOperation operation)
        {
            if (section == ForLoopSection.Before)
            {
                VisitArray(operation.Before);
            }
            else if (section == ForLoopSection.Condition)
            {
                Visit(operation.Condition);
            }
            else if (section == ForLoopSection.AtLoopBottom)
            {
                VisitArray(operation.AtLoopBottom);
            }
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                // No action required.
            }
        }

        public override void VisitForEachLoop([NotNull] IForEachLoopOperation operation)
        {
            Visit(operation.LoopControlVariable);
            Visit(operation.Collection);
        }

        public override void VisitWhileLoop([NotNull] IWhileLoopOperation operation)
        {
            Visit(operation.Condition);
        }

        public override void VisitLock([NotNull] ILockOperation operation)
        {
            Visit(operation.LockedValue);
        }

        public override void VisitUsing([NotNull] IUsingOperation operation)
        {
            Visit(operation.Resources);
        }

        public override void VisitSwitchCase([NotNull] ISwitchCaseOperation operation)
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

    private enum ForLoopSection
    {
        None,
        Before,
        Condition,
        AtLoopBottom
    }
}