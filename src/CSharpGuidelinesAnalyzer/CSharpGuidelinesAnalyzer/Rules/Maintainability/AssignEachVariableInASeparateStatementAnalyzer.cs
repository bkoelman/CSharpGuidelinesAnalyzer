using System.Collections.Immutable;
using System.Text;
using CSharpGuidelinesAnalyzer.Extensions;
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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private readonly ImmutableArray<OperationKind> statementKinds = ImmutableArray.Create(OperationKind.VariableDeclarationGroup, OperationKind.Switch,
        OperationKind.Conditional, OperationKind.Loop, OperationKind.Throw, OperationKind.Return, OperationKind.Lock, OperationKind.Using,
        OperationKind.YieldReturn, OperationKind.ExpressionStatement);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeStatement, statementKinds);
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

    private static void AnalyzeForLoop(IForLoopOperation forLoopOperation, OperationAnalysisContext context)
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

    private static void AnalyzeForLoopSection(ForLoopSection section, IOperation operation, OperationAnalysisContext context)
    {
        var statementWalker = new StatementWalker(section);
        AnalyzeVisitOperation(operation, statementWalker, context);
    }

    private static void AnalyzeVisitOperation(IOperation operation, StatementWalker statementWalker, OperationAnalysisContext context)
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

    private static Location GetLocation(IOperation operation)
    {
        return operation.TryGetLocationForKeyword(DoWhileLoopLookupKeywordStrategy.PreferWhileKeyword) ?? operation.Syntax.GetLocation();
    }

    private static string FormatIdentifierList(IList<string> variableNames)
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

    private static void AppendVariableName(string variableName, StringBuilder messageBuilder)
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

        public ISet<string> IdentifiersAssigned { get; } = new HashSet<string>();

        public override void DefaultVisit(IOperation operation)
        {
            // Check should be replaced with empty override of VisitWith(IWithOperation operation),
            // after upgrade to recent version of Microsoft.CodeAnalysis.
            if (operation.GetType().Name != "WithOperation")
            {
                base.DefaultVisit(operation);
            }
        }

        public override void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
        {
            IVariableInitializerOperation initializer = operation.GetVariableInitializer();

            if (initializer != null)
            {
                IdentifiersAssigned.Add(operation.Symbol.Name);
            }

            base.VisitVariableDeclarator(operation);
        }

        public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
        {
        }

        public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitSimpleAssignment(operation);
        }

        public override void VisitCompoundAssignment(ICompoundAssignmentOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitCompoundAssignment(operation);
        }

        public override void VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation)
        {
            RegisterAssignment(operation.Target);
            base.VisitIncrementOrDecrement(operation);
        }

        private void RegisterAssignment(IOperation operation)
        {
            IdentifierInfo? identifierInfo = operation.TryGetIdentifierInfo();

            if (identifierInfo != null)
            {
                IdentifiersAssigned.Add(identifierInfo.Name.LongName);
            }
        }

        public override void VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation)
        {
        }

        public override void VisitObjectCreation(IObjectCreationOperation operation)
        {
        }

        public override void VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation)
        {
        }

        public override void VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation)
        {
        }

        public override void VisitConditional(IConditionalOperation operation)
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

        public override void VisitForLoop(IForLoopOperation operation)
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

        public override void VisitForEachLoop(IForEachLoopOperation operation)
        {
            Visit(operation.LoopControlVariable);
            Visit(operation.Collection);
        }

        public override void VisitWhileLoop(IWhileLoopOperation operation)
        {
            Visit(operation.Condition);
        }

        public override void VisitLock(ILockOperation operation)
        {
            Visit(operation.LockedValue);
        }

        public override void VisitUsing(IUsingOperation operation)
        {
            Visit(operation.Resources);
        }

        public override void VisitSwitchCase(ISwitchCaseOperation operation)
        {
            VisitArray(operation.Clauses);
        }

        private void VisitArray(IEnumerable<IOperation>? operations)
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
