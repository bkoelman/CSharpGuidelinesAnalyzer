using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable AV1532 // Loop statement contains nested loop

namespace CSharpGuidelinesAnalyzer.Rules.Layout
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PlaceMembersInCallingOrderAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Member should be moved down.";
        private const string MessageFormat = "{0} '{1}' should be moved down. Valid file order:{2}";
        private const string Description = "Place members in a well-defined order.";

        public const string DiagnosticId = "AV2406";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Layout;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

        [NotNull]
        private static readonly Action<SemanticModelAnalysisContext, FileScanner, DependencySorter> AnalyzeSemanticModelAction = AnalyzeSemanticModel;

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
            Guard.NotNull(startContext, nameof(startContext));

            var scanner = new FileScanner();
            var sorter = new DependencySorter();

            startContext.RegisterSemanticModelAction(context => AnalyzeSemanticModelAction(context, scanner, sorter));
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context, [NotNull] FileScanner scanner, [NotNull] DependencySorter sorter)
        {
            IEnumerable<FileMember> treeInSourceOrder = scanner.ScanFile(context.SemanticModel, context.CancellationToken);
            IList<FileMember> listInSourceOrder = Flatten(treeInSourceOrder).ToList();
            IList<FileMember> listInSortedOrder = sorter.Sort(listInSourceOrder);

            CompareOrder(listInSourceOrder, listInSortedOrder, context);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<FileMember> Flatten([NotNull] [ItemNotNull] IEnumerable<FileMember> members)
        {
            foreach (FileMember member in members)
            {
                yield return member;

                foreach (FileMember next in Flatten(member.Members))
                {
                    yield return next;
                }
            }
        }

        private static void CompareOrder([NotNull] [ItemNotNull] IList<FileMember> membersInSourceOrder,
            [NotNull] [ItemNotNull] IList<FileMember> membersInSortedOrder, SemanticModelAnalysisContext context)
        {
            if (membersInSourceOrder.Count != membersInSortedOrder.Count)
            {
                throw new InvalidOperationException("Internal error: Count mismatch!");
            }

            for (int index = 0; index < membersInSourceOrder.Count; index++)
            {
                FileMember beforeMember = membersInSourceOrder[index];
                FileMember afterMember = membersInSortedOrder[index];

                if (beforeMember != afterMember)
                {
                    ReportMember(beforeMember, membersInSortedOrder, context);
                    return;
                }
            }
        }

        private static void ReportMember([NotNull] FileMember member, [NotNull] [ItemNotNull] IList<FileMember> membersInSortedOrder,
            SemanticModelAnalysisContext context)
        {
            string kind = member.AsType != null ? "Type" : "Member";
            IEnumerable<string> memberNamesInSortedOrder = membersInSortedOrder.Select(item => item.SymbolText);
            string memberLines = Environment.NewLine + string.Join(Environment.NewLine, memberNamesInSortedOrder);
            Location location = member.GetNameToken().GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, kind, member.SymbolText, memberLines);
            context.ReportDiagnostic(diagnostic);
        }

        private sealed class FileScanner
        {
            [NotNull]
            [ItemNotNull]
            public IEnumerable<FileMember> ScanFile([NotNull] SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                SyntaxNode syntaxRoot = semanticModel.SyntaxTree.GetRoot(cancellationToken);

                var typeWalker = new TopLevelTypeWalker();
                typeWalker.Visit(syntaxRoot);

                var factory = new FileMemberFactory();

                foreach (TypeDeclarationSyntax typeSyntax in typeWalker.TopLevelTypes)
                {
                    yield return ScanType(typeSyntax, semanticModel, factory, cancellationToken);
                }
            }

            [NotNull]
            private FileMember ScanType([NotNull] TypeDeclarationSyntax typeSyntax, [NotNull] SemanticModel semanticModel, [NotNull] FileMemberFactory factory,
                CancellationToken cancellationToken)
            {
                INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(typeSyntax, cancellationToken);
                ImmutableArray<ISymbol> memberSymbols = typeSymbol.GetMembers();

                FileMember type = factory.GetOrCreate(typeSyntax, typeSymbol);

                foreach (ISymbol memberSymbol in memberSymbols)
                {
                    foreach (MemberDeclarationSyntax memberSyntax in MemberToSyntax(memberSymbol, semanticModel.SyntaxTree, typeSyntax.Span))
                    {
                        if (memberSyntax is TypeDeclarationSyntax nestedTypeSyntax)
                        {
                            if (CanContainBody(nestedTypeSyntax))
                            {
                                FileMember nestedType = ScanType(nestedTypeSyntax, semanticModel, factory, cancellationToken);
                                type.IncludeMember(nestedType);
                            }
                        }
                        else
                        {
                            FileMember member = ScanMember(memberSyntax, memberSymbol, semanticModel, factory);
                            type.IncludeMember(member);
                        }
                    }
                }

                return type;
            }

            [NotNull]
            [ItemNotNull]
            private static IEnumerable<MemberDeclarationSyntax> MemberToSyntax([NotNull] ISymbol member, [NotNull] SyntaxTree syntaxTree,
                [CanBeNull] TextSpan? containingTypeSpan)
            {
                foreach (SyntaxNode syntaxNode in GetSyntaxNodesInSpan(member, syntaxTree, containingTypeSpan))
                {
                    if (syntaxNode is MemberDeclarationSyntax memberSyntax)
                    {
                        yield return memberSyntax;
                    }
                    else if (syntaxNode is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax fieldSyntax } })
                    {
                        yield return fieldSyntax;
                    }
                }
            }

            [NotNull]
            [ItemNotNull]
            private static IEnumerable<SyntaxNode> GetSyntaxNodesInSpan([NotNull] ISymbol symbol, [NotNull] SyntaxTree syntaxTree, [CanBeNull] TextSpan? span)
            {
                ImmutableArray<SyntaxReference> references = symbol.DeclaringSyntaxReferences;

                if (symbol is IMethodSymbol methodSymbol)
                {
                    IMethodSymbol otherPart = methodSymbol.PartialImplementationPart ?? methodSymbol.PartialDefinitionPart;

                    if (otherPart != null)
                    {
                        references = references.AddRange(otherPart.DeclaringSyntaxReferences);
                    }
                }

                references = span != null
                    ? references.Where(syntaxRef => syntaxRef.SyntaxTree == syntaxTree && span.Value.IntersectsWith(syntaxRef.Span)).ToImmutableArray()
                    : references.Where(syntaxRef => syntaxRef.SyntaxTree == syntaxTree).ToImmutableArray();

                return references.Select(syntaxRef => syntaxRef.GetSyntax());
            }

            private bool CanContainBody([NotNull] TypeDeclarationSyntax typeSyntax)
            {
                return typeSyntax.Keyword.IsKind(SyntaxKind.ClassKeyword) || typeSyntax.Keyword.IsKind(SyntaxKind.StructKeyword);
            }

            [NotNull]
            private FileMember ScanMember([NotNull] MemberDeclarationSyntax memberSyntax, [NotNull] ISymbol memberSymbol, [NotNull] SemanticModel semanticModel,
                [NotNull] FileMemberFactory factory)
            {
                FileMember member = factory.GetOrCreate(memberSyntax, memberSymbol);

                foreach (FileMember reference in AnalyzeMemberBody(memberSyntax, semanticModel, factory))
                {
                    // A nested type member referencing a member on its parent type must not affect ordering, so don't track.
                    if (reference.AsType == null && member.AsType == null && member.ContainingType!.IsNestedTypeOf(reference.ContainingType))
                    {
                        continue;
                    }

                    member.SetReferenceTo(reference);
                }

                return member;
            }

            [NotNull]
            [ItemNotNull]
            private static IList<FileMember> AnalyzeMemberBody([NotNull] MemberDeclarationSyntax memberSyntax, [NotNull] SemanticModel model,
                [NotNull] FileMemberFactory factory)
            {
                var walker = new BodyInvocationWalker(memberSyntax, model, factory);
                return walker.GetReferences();
            }

            private sealed class TopLevelTypeWalker : CSharpSyntaxWalker
            {
                [NotNull]
                [ItemNotNull]
                public readonly IList<TypeDeclarationSyntax> TopLevelTypes = new List<TypeDeclarationSyntax>();

                private bool isInNestedScope;

                public override void VisitClassDeclaration([NotNull] ClassDeclarationSyntax classSyntax)
                {
                    AddTopLevel(classSyntax, () => base.VisitClassDeclaration(classSyntax));
                }

                public override void VisitStructDeclaration([NotNull] StructDeclarationSyntax structSyntax)
                {
                    AddTopLevel(structSyntax, () => base.VisitStructDeclaration(structSyntax));
                }

                private void AddTopLevel([NotNull] TypeDeclarationSyntax typeSyntax, [NotNull] Action action)
                {
                    if (!isInNestedScope)
                    {
                        TopLevelTypes.Add(typeSyntax);

                        isInNestedScope = true;
                        action();
                        isInNestedScope = false;
                    }
                }
            }

            public sealed class FileMemberFactory
            {
                [NotNull]
                private readonly Dictionary<MemberDeclarationSyntax, FileMember> instanceCache = new Dictionary<MemberDeclarationSyntax, FileMember>();

                [NotNull]
                public FileMember GetOrCreate([NotNull] MemberDeclarationSyntax syntax, [NotNull] ISymbol memberSymbol)
                {
                    if (instanceCache.ContainsKey(syntax))
                    {
                        return instanceCache[syntax];
                    }

                    var member = new FileMember(syntax, memberSymbol);
                    instanceCache.Add(syntax, member);

                    return member;
                }
            }

            private sealed class BodyInvocationWalker : CSharpSyntaxWalker
            {
                [NotNull]
                private readonly SemanticModel semanticModel;

                [NotNull]
                private readonly FileMemberFactory factory;

                [NotNull]
                private readonly MemberDeclarationSyntax memberSyntax;

                [NotNull]
                [ItemNotNull]
                private readonly IList<FileMember> references = new List<FileMember>();

                public BodyInvocationWalker([NotNull] MemberDeclarationSyntax memberSyntax, [NotNull] SemanticModel semanticModel,
                    [NotNull] FileMemberFactory factory)
                {
                    this.semanticModel = semanticModel;
                    this.factory = factory;
                    this.memberSyntax = memberSyntax;
                }

                [NotNull]
                [ItemNotNull]
                public IList<FileMember> GetReferences()
                {
                    Visit(memberSyntax);
                    return references;
                }

                public override void VisitConstructorInitializer([NotNull] ConstructorInitializerSyntax node)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(node);
                    InspectReference(info);

                    base.VisitConstructorInitializer(node);
                }

                public override void VisitInvocationExpression([NotNull] InvocationExpressionSyntax node)
                {
                    if (node.Expression is MemberAccessExpressionSyntax)
                    {
                        SymbolInfo info = semanticModel.GetSymbolInfo(node.Expression);
                        InspectReference(info);
                    }

                    base.VisitInvocationExpression(node);
                }

                public override void VisitIdentifierName([NotNull] IdentifierNameSyntax node)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(node);
                    InspectReference(info);

                    base.VisitIdentifierName(node);
                }

                private void InspectReference(SymbolInfo info)
                {
                    if (info.Symbol is IMethodSymbol || info.Symbol is IPropertySymbol || info.Symbol is INamedTypeSymbol)
                    {
                        foreach (MemberDeclarationSyntax remoteMemberSyntax in MemberToSyntax(info.Symbol, semanticModel.SyntaxTree, null))
                        {
                            if (ReferenceEquals(remoteMemberSyntax, memberSyntax))
                            {
                                // Skip recursion into self.
                                continue;
                            }

                            FileMember remoteMember = factory.GetOrCreate(remoteMemberSyntax, info.Symbol);

                            if (!references.Contains(remoteMember))
                            {
                                references.Add(remoteMember);
                            }
                        }
                    }
                }
            }
        }

        private sealed class DependencySorter
        {
            [NotNull]
            [ItemNotNull]
            public IList<FileMember> Sort([NotNull] [ItemNotNull] IEnumerable<FileMember> members)
            {
                IEnumerable<FileMember> membersSorted = TotalSort(members);
                FileMember[] membersSortedGrouped = GroupByMemberKind(membersSorted).ToArray();
                IEnumerable<FileMember> memberSortedTypeGrouped = GroupByType(membersSortedGrouped);

                return memberSortedTypeGrouped.ToList();
            }

            [NotNull]
            [ItemNotNull]
            private IEnumerable<FileMember> GroupByMemberKind([NotNull] [ItemNotNull] IEnumerable<FileMember> members)
            {
                IDictionary<MemberKind, List<FileMember>> membersPerKind = CollectMembersByKind(members);

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                foreach (FileMember member in Enum
                             .GetValues(typeof(MemberKind))
                             .Cast<MemberKind>()
                             .Where(kind => membersPerKind.ContainsKey(kind))
                             .SelectMany(kind => membersPerKind[kind]))
                {
                    yield return member;
                }

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore
            }

            [NotNull]
            private static IDictionary<MemberKind, List<FileMember>> CollectMembersByKind([NotNull] [ItemNotNull] IEnumerable<FileMember> members)
            {
                var membersPerKind = new Dictionary<MemberKind, List<FileMember>>();

                foreach (FileMember member in members)
                {
                    MemberKind kind = member.Classify();

                    if (!membersPerKind.ContainsKey(kind))
                    {
                        membersPerKind[kind] = new List<FileMember>();
                    }

                    membersPerKind[kind].Add(member);
                }

                return membersPerKind;
            }

            [NotNull]
            [ItemNotNull]
            private static IEnumerable<FileMember> TotalSort([NotNull] [ItemNotNull] IEnumerable<FileMember> members)
            {
                IEnumerable<FileMember> unusedMembers = members.Where(item => !item.Usages.Any()).Reverse();
                var pendingStack = new Stack<FileMember>(unusedMembers);
                var seenSet = new HashSet<FileMember>();

                while (pendingStack.Any())
                {
                    FileMember next = pendingStack.Pop();

                    if (!seenSet.Contains(next))
                    {
                        seenSet.Add(next);
                        yield return next;

                        PushReferences(next, pendingStack, seenSet);
                    }
                }
            }

            private static void PushReferences([NotNull] FileMember member, [NotNull] [ItemNotNull] Stack<FileMember> pendingStack,
                [NotNull] [ItemNotNull] ISet<FileMember> seenSet)
            {
                foreach (FileMember reference in member.References.Where(reference => !seenSet.Contains(reference)).Reverse())
                {
                    pendingStack.Push(reference);
                }
            }

            [NotNull]
            [ItemNotNull]
            private static IEnumerable<FileMember> GroupByType([NotNull] [ItemNotNull] IList<FileMember> members)
            {
                // We cannot emit a nested type before its containing types, so we build a type dependency tree first.

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                InsertionOrderPreservingDictionary<TypeDefinition, List<FileMember>> membersByContainingType = members
                    .Where(member => member.AsType == null)
                    .GroupBy(member => member.ContainingType)
                    .ToInsertionOrderPreservingDictionary(grouping => grouping.Key, grouping => grouping.ToList());

                InsertionOrderPreservingDictionary<TypeDefinition, FileMember> types = members
                    .Where(member => member.AsType != null)
                    .ToInsertionOrderPreservingDictionary(member => member.AsType);

                IEnumerable<TypeDefinition> typeDefinitions = types.Select(tuple => tuple.Key);

                IEnumerable<TypeDefinition> uniqueTypes = membersByContainingType
                    .Select(tuple => tuple.Key)
                    .Concat(typeDefinitions)
                    .Distinct();

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                TypeNode hiddenRoot = BuildTypeDependencyTree(uniqueTypes);

                foreach (TypeNode node in hiddenRoot.Traverse().Skip(1))
                {
                    yield return types[node.Type].Item;

                    if (membersByContainingType.Contains(node.Type))
                    {
                        foreach (FileMember member in membersByContainingType[node.Type].Item)
                        {
                            yield return member;
                        }
                    }
                }
            }

            [NotNull]
            private static TypeNode BuildTypeDependencyTree([NotNull] [ItemNotNull] IEnumerable<TypeDefinition> types)
            {
                var hiddenRoot = new TypeNode(null);

                foreach (TypeDefinition type in types)
                {
                    Stack<TypeDefinition> parentTypeStack = PushContainingTypes(type);
                    TypeNode currentNode = hiddenRoot;

                    while (parentTypeStack.Any())
                    {
                        TypeDefinition nextType = parentTypeStack.Pop();
                        currentNode = currentNode.GetOrAdd(nextType);
                    }
                }

                return hiddenRoot;
            }

            [NotNull]
            [ItemNotNull]
            private static Stack<TypeDefinition> PushContainingTypes([NotNull] TypeDefinition typeDefinition)
            {
                var parentTypeStack = new Stack<TypeDefinition>();
                TypeDefinition current = typeDefinition;

                while (current != null)
                {
                    parentTypeStack.Push(current);
                    current = current.ContainingType;
                }

                return parentTypeStack;
            }

            private sealed class TypeNode
            {
                [NotNull]
                public TypeDefinition Type { get; }

                [NotNull]
                public InsertionOrderPreservingDictionary<TypeDefinition, TypeNode> NestedTypes { get; } =
                    new InsertionOrderPreservingDictionary<TypeDefinition, TypeNode>();

                public TypeNode([NotNull] TypeDefinition type)
                {
                    Type = type;
                }

                [NotNull]
                public TypeNode GetOrAdd([NotNull] TypeDefinition type)
                {
                    if (NestedTypes.Contains(type))
                    {
                        return NestedTypes[type].Item;
                    }

                    var newNode = new TypeNode(type);
                    NestedTypes.Add((type, newNode));
                    return newNode;
                }

                [NotNull]
                [ItemNotNull]
                public IEnumerable<TypeNode> Traverse()
                {
                    yield return this;

                    foreach (TypeNode next in NestedTypes.SelectMany(child => child.Item.Traverse()))
                    {
                        yield return next;
                    }
                }
            }
        }

        private sealed class FileMember
        {
            [NotNull]
            [ItemNotNull]
            private readonly List<FileMember> members = new List<FileMember>();

            [NotNull]
            [ItemNotNull]
            private readonly List<FileMember> references = new List<FileMember>();

            [NotNull]
            [ItemNotNull]
            private readonly List<FileMember> usages = new List<FileMember>();

            [NotNull]
            public MemberDeclarationSyntax Syntax { get; }

            [NotNull]
            public ISymbol Symbol { get; }

            [CanBeNull]
            public TypeDefinition AsType => Symbol is INamedTypeSymbol typeSymbol ? new TypeDefinition(Syntax, typeSymbol) : null;

            [CanBeNull]
            public TypeDefinition ContainingType
            {
                get
                {
                    TypeDefinition type = AsType;

                    if (type != null)
                    {
                        return type.ContainingType;
                    }

                    BaseTypeDeclarationSyntax containingTypeSyntax = Syntax.TryGetContainingTypeDeclaration();
                    return new TypeDefinition(containingTypeSyntax!, Symbol.ContainingType);
                }
            }

            [NotNull]
            [ItemNotNull]
            public IReadOnlyList<FileMember> Members => members.AsReadOnly();

            [CanBeNull]
            public FileMember Container { get; private set; }

            [NotNull]
            [ItemNotNull]
            public IReadOnlyList<FileMember> References => references.AsReadOnly();

            [NotNull]
            [ItemNotNull]
            public IReadOnlyList<FileMember> Usages => usages.AsReadOnly();

            [NotNull]
            public string SymbolText => Symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

            public FileMember([NotNull] MemberDeclarationSyntax syntax, [NotNull] ISymbol symbol)
            {
                Guard.NotNull(syntax, nameof(syntax));
                Guard.NotNull(symbol, nameof(symbol));

                Syntax = syntax;
                Symbol = symbol;
            }

            public void SetReferenceTo([NotNull] FileMember reference)
            {
                references.Add(reference);
                reference.usages.Add(this);
            }

            public void IncludeMember([NotNull] FileMember member)
            {
                member.Container = this;
                members.Add(member);
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append(Symbol.Kind);
                builder.Append(": ");
                builder.Append(SymbolText);
                WritePartialMethodClassifier(builder);

                WriteReferences(builder);
                WriteUsages(builder);

                return builder.ToString();
            }

            private void WritePartialMethodClassifier([NotNull] StringBuilder builder)
            {
                if (Symbol is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.PartialDefinitionPart != null || methodSymbol.PartialImplementationPart != null)
                    {
                        IMethodSymbol impSymbol = methodSymbol.PartialImplementationPart ?? methodSymbol;

                        if (impSymbol.DeclaringSyntaxReferences.Any(syntaxRef => syntaxRef.GetSyntax() == Syntax))
                        {
                            builder.Append(" imp");
                        }

                        IMethodSymbol defSymbol = methodSymbol.PartialDefinitionPart ?? methodSymbol;

                        if (defSymbol.DeclaringSyntaxReferences.Any(syntaxRef => syntaxRef.GetSyntax() == Syntax))
                        {
                            builder.Append(" def");
                        }
                    }
                }
            }

            private void WriteReferences([NotNull] StringBuilder builder)
            {
                if (References.Any())
                {
                    builder.Append(" -> { ");

                    IEnumerable<string> names = References.Select(reference => reference.SymbolText);
                    string text = string.Join(", ", names);
                    builder.Append(text);

                    builder.Append(" }");
                }
            }

            private void WriteUsages([NotNull] StringBuilder builder)
            {
                if (Usages.Any())
                {
                    builder.Append(" <- { ");

                    IEnumerable<string> names = Usages.Select(usage => usage.SymbolText);
                    string text = string.Join(", ", names);
                    builder.Append(text);

                    builder.Append(" }");
                }
            }

            public SyntaxToken GetNameToken()
            {
                return Syntax.Kind() switch
                {
                    SyntaxKind.EnumDeclaration => ((EnumDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.ClassDeclaration => ((TypeDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.InterfaceDeclaration => ((TypeDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.StructDeclaration => ((TypeDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.DelegateDeclaration => ((DelegateDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.FieldDeclaration => ((FieldDeclarationSyntax)Syntax).Declaration.Variables.First().Identifier,
                    SyntaxKind.EventFieldDeclaration => ((EventFieldDeclarationSyntax)Syntax).Declaration.Variables.First().Identifier,
                    SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.EventDeclaration => ((EventDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.ConstructorDeclaration => ((ConstructorDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.DestructorDeclaration => ((DestructorDeclarationSyntax)Syntax).Identifier,
                    SyntaxKind.IndexerDeclaration => ((IndexerDeclarationSyntax)Syntax).ThisKeyword,
                    SyntaxKind.OperatorDeclaration => ((OperatorDeclarationSyntax)Syntax).OperatorToken,
                    SyntaxKind.ConversionOperatorDeclaration => ((ConversionOperatorDeclarationSyntax)Syntax).OperatorKeyword,
                    var unknown => throw new InvalidOperationException($"Internal error: kind '{unknown}' in '{Syntax}'.")
                };
            }

            public MemberKind Classify()
            {
                return Symbol switch
                {
                    INamedTypeSymbol _ => MemberKind.Type,
                    IFieldSymbol { IsConst: true } => MemberKind.Constant,
                    IFieldSymbol { IsStatic: true } fieldSymbol => fieldSymbol.IsReadOnly ? MemberKind.StaticReadonlyField : MemberKind.StaticField,
                    IFieldSymbol { IsReadOnly: true } => MemberKind.InstanceReadonlyField,
                    IFieldSymbol _ => MemberKind.InstanceField,
                    IPropertySymbol { IsIndexer: true } => MemberKind.Indexer,
                    IPropertySymbol { IsStatic: true } => MemberKind.StaticProperty,
                    IPropertySymbol _ => MemberKind.InstanceProperty,
                    IEventSymbol { IsStatic: true } => MemberKind.StaticEvent,
                    IEventSymbol _ => MemberKind.InstanceEvent,
                    IMethodSymbol { MethodKind: MethodKind.StaticConstructor } => MemberKind.StaticConstructor,
                    IMethodSymbol { MethodKind: MethodKind.Constructor } => MemberKind.InstanceConstructor,
                    IMethodSymbol { MethodKind: MethodKind.Destructor } => MemberKind.Destructor,
                    IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.UserDefinedOperator ||
                        methodSymbol.MethodKind == MethodKind.Conversion => MemberKind.Operator,
                    IMethodSymbol methodSymbol when methodSymbol.IsUnitTestMethod() => MemberKind.TestMethod,
                    IMethodSymbol _ => MemberKind.Method,
                    _ => throw new Exception($"Internal error: No ordering defined for member '{SymbolText}' of symbol type '{Symbol.GetType()}'.")
                };
            }
        }

        private sealed class TypeDefinition : IEquatable<TypeDefinition>
        {
            [NotNull]
            public MemberDeclarationSyntax Syntax { get; }

            [NotNull]
            public INamedTypeSymbol Symbol { get; }

            [CanBeNull]
            public TypeDefinition ContainingType
            {
                get
                {
                    BaseTypeDeclarationSyntax containingTypeSyntax = Syntax.TryGetContainingTypeDeclaration();
                    return containingTypeSyntax == null ? null : new TypeDefinition(containingTypeSyntax, Symbol.ContainingType);
                }
            }

            public TypeDefinition([NotNull] MemberDeclarationSyntax syntax, [NotNull] INamedTypeSymbol symbol)
            {
                Guard.NotNull(syntax, nameof(syntax));
                Guard.NotNull(symbol, nameof(symbol));

                Syntax = syntax;
                Symbol = symbol;
            }

            public bool IsNestedTypeOf([CanBeNull] TypeDefinition other)
            {
                TypeDefinition current = ContainingType;

                while (current != null)
                {
                    if (Equals(current, other))
                    {
                        return true;
                    }

                    current = current.ContainingType;
                }

                return false;
            }

            public override bool Equals(object other)
            {
                return Equals(other as TypeDefinition);
            }

            public bool Equals(TypeDefinition other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Syntax.Equals(other.Syntax);
            }

            public override int GetHashCode()
            {
                return Syntax.GetHashCode();
            }
        }

        private enum MemberKind
        {
            Type,
            Constant,
            StaticReadonlyField,
            StaticField,
            InstanceReadonlyField,
            InstanceField,
            StaticProperty,
            InstanceProperty,
            Indexer,
            StaticEvent,
            InstanceEvent,
            StaticConstructor,
            InstanceConstructor,
            TestMethod,
            Method,
            Operator,
            Destructor
        }
    }
}
