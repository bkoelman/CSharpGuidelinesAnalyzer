using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotPassNullOnEventInvocationAnalyzer : DiagnosticAnalyzer
{
    private const string SenderTitle = "'sender' argument is null in non-static event invocation";
    private const string SenderMessageFormat = "'sender' argument is null in non-static event invocation";
    private const string ArgsTitle = "Argument for second parameter is null in event invocation";
    private const string ArgsMessageFormat = "'{0}' argument is null in event invocation";
    private const string Description = "Don't pass null as the sender argument when raising an event.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1235";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    private static readonly DiagnosticDescriptor SenderRule = new(DiagnosticId, SenderTitle, SenderMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor ArgsRule = new(DiagnosticId, ArgsTitle, ArgsMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning,
        true, Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SenderRule, ArgsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        INamedTypeSymbol? systemEventArgs = KnownTypes.SystemEventArgs(startContext.Compilation);

        if (systemEventArgs != null)
        {
            startContext.SafeRegisterOperationAction(context => AnalyzeInvocation(context, systemEventArgs), OperationKind.Invocation);
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol systemEventArgs)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.Arguments.Length != 2)
        {
            return;
        }

        AnalyzeEventInvocation(invocation, context, systemEventArgs);
    }

    private static void AnalyzeEventInvocation(IInvocationOperation invocation, OperationAnalysisContext context, INamedTypeSymbol systemEventArgs)
    {
        bool? targetsStaticEvent = IsStaticEvent(invocation.Instance, context.Compilation);

        if (targetsStaticEvent != null)
        {
            if (invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke)
            {
                if (!targetsStaticEvent.Value)
                {
                    AnalyzeSenderArgument(invocation, context);
                }

                AnalyzeArgsArgument(invocation, systemEventArgs, context);
            }
        }
    }

    private static bool? IsStaticEvent(IOperation? operation, Compilation compilation)
    {
        return IsStaticEventInvocation(operation) ?? IsStaticEventInvocationUsingNullConditionalAccessOperator(operation, compilation);
    }

    private static bool? IsStaticEventInvocation(IOperation? operation)
    {
        if (operation is IEventReferenceOperation eventReference)
        {
            return eventReference.Instance == null;
        }

        return null;
    }

    private static bool? IsStaticEventInvocationUsingNullConditionalAccessOperator(IOperation? operation, Compilation compilation)
    {
        if (operation is IConditionalAccessInstanceOperation)
        {
            SemanticModel model = operation.GetSemanticModel(compilation);

            if (model.GetSymbolInfo(operation.Syntax).Symbol is IEventSymbol eventSymbol)
            {
                return eventSymbol.IsStatic;
            }
        }

        return null;
    }

    private static void AnalyzeSenderArgument(IInvocationOperation invocation, OperationAnalysisContext context)
    {
        IArgumentOperation? senderArgument = GetSenderArgument(invocation);

        if (senderArgument != null && IsNullConstant(senderArgument.Value))
        {
            Location location = senderArgument.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(SenderRule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IArgumentOperation? GetSenderArgument(IInvocationOperation invocation)
    {
        IArgumentOperation? argument = invocation.Arguments.FirstOrDefault(nextArgument => nextArgument.Parameter?.Name == "sender");

        return argument != null && argument.Parameter?.Type.SpecialType == SpecialType.System_Object ? argument : null;
    }

    private static void AnalyzeArgsArgument(IInvocationOperation invocation, INamedTypeSymbol systemEventArgs, OperationAnalysisContext context)
    {
        IArgumentOperation? argsArgument = GetArgsArgument(invocation, systemEventArgs);

        if (argsArgument != null && IsNullConstant(argsArgument.Value) && argsArgument.Parameter != null)
        {
            Location location = argsArgument.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(ArgsRule, location, argsArgument.Parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IArgumentOperation? GetArgsArgument(IInvocationOperation invocation, INamedTypeSymbol systemEventArgs)
    {
        return invocation.Arguments.FirstOrDefault(argument =>
            !string.IsNullOrEmpty(argument.Parameter?.Name) && IsEventArgs(argument.Parameter?.Type, systemEventArgs));
    }

    private static bool IsEventArgs(ITypeSymbol? type, INamedTypeSymbol systemEventArgs)
    {
        ITypeSymbol? nextType = type;

        while (nextType != null)
        {
            if (EqualityComparer<ISymbol>.Default.Equals(nextType, systemEventArgs))
            {
                return true;
            }

            nextType = nextType.BaseType;
        }

        return false;
    }

    private static bool IsNullConstant(IOperation operation)
    {
        return operation.ConstantValue is { HasValue: true, Value: null };
    }
}
