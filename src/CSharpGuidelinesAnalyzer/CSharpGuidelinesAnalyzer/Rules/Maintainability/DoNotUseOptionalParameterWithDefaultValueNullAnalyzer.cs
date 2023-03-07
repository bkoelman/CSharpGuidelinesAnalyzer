using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseOptionalParameterWithDefaultValueNullAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Do not use optional parameters with default value null for strings, collections or tasks";
        private const string MessageFormat = "Optional parameter '{0}' of type '{1}' has default value 'null'";
        private const string Description = "Only use optional parameters to replace overloads.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1553";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext, IList<INamedTypeSymbol>, INamedTypeSymbol> AnalyzeParameterAction =
            (syntaxContext, taskTypes, callerArgumentExpressionAttributeType) => syntaxContext.SkipEmptyName(symbolContext =>
                AnalyzeParameter(symbolContext, taskTypes, callerArgumentExpressionAttributeType));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            IList<INamedTypeSymbol> taskTypes = ResolveTaskTypes(startContext.Compilation).ToList();

            INamedTypeSymbol callerArgumentExpressionAttributeType =
                KnownTypes.SystemRuntimeCompilerServicesCallerArgumentExpressionAttribute(startContext.Compilation);

            startContext.RegisterSyntaxNodeAction(context => AnalyzeParameterAction(context, taskTypes, callerArgumentExpressionAttributeType),
                SyntaxKind.Parameter);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<INamedTypeSymbol> ResolveTaskTypes([NotNull] Compilation compilation)
        {
            foreach (INamedTypeSymbol taskType in new[]
            {
                KnownTypes.SystemThreadingTasksTaskT(compilation),
                KnownTypes.SystemThreadingTasksTask(compilation),
                KnownTypes.SystemThreadingTasksValueTask(compilation),
                KnownTypes.SystemThreadingTasksValueTaskT(compilation)
            })
            {
                if (taskType != null)
                {
                    yield return taskType;
                }
            }
        }

        private static void AnalyzeParameter(SymbolAnalysisContext context, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes,
            [CanBeNull] INamedTypeSymbol callerArgumentExpressionAttributeType)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.IsOptional && parameter.HasExplicitDefaultValue && parameter.ExplicitDefaultValue == null &&
                !HasCallerArgumentExpressionAttribute(parameter, callerArgumentExpressionAttributeType))
            {
                if (parameter.Type.IsOrImplementsIEnumerable() || IsTask(parameter.Type, taskTypes))
                {
                    SyntaxReference syntaxReference = parameter.DeclaringSyntaxReferences.First();
                    string typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                    var location = Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);

                    var diagnostic = Diagnostic.Create(Rule, location, parameter.Name, typeName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool HasCallerArgumentExpressionAttribute([NotNull] IParameterSymbol parameter,
            [CanBeNull] INamedTypeSymbol callerArgumentExpressionAttributeType)
        {
            return callerArgumentExpressionAttributeType != null &&
                parameter.GetAttributes().Any(attr => Equals(attr.AttributeClass, callerArgumentExpressionAttributeType));
        }

        private static bool IsTask([NotNull] ITypeSymbol type, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
        {
            ITypeSymbol unwrappedType = type.UnwrapNullableValueType();
            return taskTypes.Any(taskType => taskType.IsEqualTo(unwrappedType.OriginalDefinition));
        }
    }
}
