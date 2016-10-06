using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Test
{
    public class ParsedSourceCode
    {
        public const string FixMarker = "<annotate/>";
        private const string ImportMarker = "<import/>";

        [NotNull]
        private readonly string text;

        [NotNull]
        private readonly List<SourceLocation> codeFixPoints = new List<SourceLocation>();

        [NotNull]
        private readonly List<SourceLocation> importPoints = new List<SourceLocation>();

        [NotNull]
        public string Filename { get; }

        [NotNull]
        [ItemNotNull]
        public ImmutableHashSet<MetadataReference> References { get; private set; }

        [NotNull]
        private string CodeNamespaceImport { get; }

        public bool ReIndentExpected { get; }

        [NotNull]
        private readonly string attributePrefix;

        public ParsedSourceCode([NotNull] string text, [NotNull] string filename,
            [NotNull] [ItemNotNull] ImmutableHashSet<MetadataReference> references,
            [ItemNotNull] [NotNull] IList<string> nestedTypes, [NotNull] string codeNamespaceImport, bool reIndent)
        {
            Guard.NotNull(text, nameof(text));
            Guard.NotNull(filename, nameof(filename));
            Guard.NotNull(references, nameof(references));
            Guard.NotNull(nestedTypes, nameof(nestedTypes));
            Guard.NotNull(codeNamespaceImport, nameof(codeNamespaceImport));

            this.text = Parse(text);
            Filename = filename;
            References = references;
            attributePrefix = ExtractAttributePrefix(nestedTypes);
            CodeNamespaceImport = codeNamespaceImport;
            ReIndentExpected = reIndent;
        }

        [NotNull]
        private static string ExtractAttributePrefix([NotNull] [ItemNotNull] IList<string> nestedTypes)
        {
            var attributePrefixBuilder = new StringBuilder();
            foreach (string nestedType in nestedTypes)
            {
                int lastSpaceIndex = nestedType.LastIndexOf(' ');
                string typeName = lastSpaceIndex != -1 ? nestedType.Substring(lastSpaceIndex + 1) : nestedType;

                attributePrefixBuilder.Append(typeName);
                attributePrefixBuilder.Append('.');
            }
            return attributePrefixBuilder.ToString();
        }

        [NotNull]
        private string Parse([NotNull] string sourceText)
        {
            var outputBuilder = new StringBuilder();
            using (var reader = new StringReader(sourceText))
            {
                int lineNumber = 1;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Tuple<MarkerType, int> startMarker = GetNextMarkerStart(line);
                    while (startMarker != null)
                    {
                        if (startMarker.Item1 == MarkerType.FixPlaceholder)
                        {
                            int fixMarkerIndex = startMarker.Item2;

                            line = line.Substring(0, fixMarkerIndex) + line.Substring(fixMarkerIndex + FixMarker.Length);

                            var marker = new SourceLocation(lineNumber, fixMarkerIndex + 1);
                            codeFixPoints.Add(marker);
                        }
                        else if (startMarker.Item1 == MarkerType.ImportPlaceholder)
                        {
                            int importMarkerIndex = startMarker.Item2;

                            line = line.Substring(0, importMarkerIndex) +
                                line.Substring(importMarkerIndex + ImportMarker.Length);

                            var marker = new SourceLocation(lineNumber, importMarkerIndex + 1);
                            importPoints.Add(marker);
                        }

                        startMarker = GetNextMarkerStart(line);
                    }

                    outputBuilder.AppendLine(line);
                    lineNumber++;
                }
            }

            return outputBuilder.ToString();
        }

        [CanBeNull]
        private static Tuple<MarkerType, int> GetNextMarkerStart([NotNull] string line)
        {
            IOrderedEnumerable<Tuple<MarkerType, int>> query =
                from value in
                new[]
                {
                    Tuple.Create(MarkerType.FixPlaceholder, line.IndexOf(FixMarker, StringComparison.Ordinal)),
                    Tuple.Create(MarkerType.ImportPlaceholder, line.IndexOf(ImportMarker, StringComparison.Ordinal))
                }
                where value.Item2 != -1
                orderby value.Item2
                select value;

            return query.FirstOrDefault();
        }

        [NotNull]
        public string GetText()
        {
            return text;
        }

        [NotNull]
        public virtual string GetExpectedTextForAttribute([NotNull] string attributeName)
        {
            Guard.NotNull(attributeName, nameof(attributeName));

            attributeName = GetAttributeNameWithPrefix(attributeName);

            if (!importPoints.Any() && !codeFixPoints.Any())
            {
                return text;
            }

            // Assumption: import points are always located above fix points.
            string text1 = GetTextWithFixPointsExpanded(text, attributeName);
            string text2 = GetTextWithImportPointsExpanded(text1);
            return text2;
        }

        [NotNull]
        private string GetAttributeNameWithPrefix([NotNull] string attributeName)
        {
            return "[" + attributePrefix + attributeName + "]";
        }

        [NotNull]
        private string GetTextWithImportPointsExpanded([NotNull] string original)
        {
            var importPointsQueue = new Stack<SourceLocation>(importPoints);

            string usingDeclaration = "using " + CodeNamespaceImport + ";";
            return GetExpandedText(original, usingDeclaration, importPointsQueue);
        }

        [NotNull]
        private string GetTextWithFixPointsExpanded([NotNull] string original, [NotNull] string fixText)
        {
            var fixPointsQueue = new Stack<SourceLocation>(codeFixPoints);
            return GetExpandedText(original, fixText, fixPointsQueue);
        }

        [NotNull]
        private static string GetExpandedText([NotNull] string original, [NotNull] string expansion,
            [NotNull] Stack<SourceLocation> pointQueue)
        {
            var outputBuilder = new StringBuilder();
            using (var reader = new StringReader(original))
            {
                int lineNumber = 1;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    while (pointQueue.Any() && pointQueue.Peek().Line == lineNumber)
                    {
                        int column = pointQueue.Pop().Column - 1;
                        line = line.Substring(0, column) + expansion + line.Substring(column);
                    }

                    outputBuilder.AppendLine(line);
                    lineNumber++;
                }
            }
            return outputBuilder.ToString();
        }

        private struct SourceLocation
        {
            public int Line { get; }
            public int Column { get; }

            public SourceLocation(int line, int column)
            {
                if (line < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(line), "line must be >= 1");
                }

                if (column < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(column), "column must be >= 1");
                }

                Line = line;
                Column = column;
            }

            public override string ToString()
            {
                return "(" + Line + "," + Column + ")";
            }
        }

        private enum MarkerType
        {
            FixPlaceholder,
            ImportPlaceholder
        }
    }
}