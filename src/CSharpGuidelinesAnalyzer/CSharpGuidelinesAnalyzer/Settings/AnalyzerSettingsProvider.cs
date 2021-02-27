using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Settings
{
    public static class AnalyzerSettingsProvider
    {
        public const string SettingsFileName = "CSharpGuidelinesAnalyzer.config";

        [NotNull]
        internal static AnalyzerSettingsRegistry LoadSettings([NotNull] AnalyzerOptions options, CancellationToken cancellationToken)
        {
            Guard.NotNull(options, nameof(options));

            AdditionalText settingsFileOrNull = options.AdditionalFiles.FirstOrDefault(file => IsSettingsFile(file.Path));

            if (settingsFileOrNull != null)
            {
                SourceText fileText = settingsFileOrNull.GetText(cancellationToken);

                return SafeReadSourceText(fileText, cancellationToken);
            }

            return AnalyzerSettingsRegistry.ImmutableEmpty;
        }

        [NotNull]
        private static AnalyzerSettingsRegistry SafeReadSourceText([NotNull] SourceText fileText, CancellationToken cancellationToken)
        {
            try
            {
                return ReadSourceText(fileText, AnalyzerSettingsXmlConverter.ParseXml, cancellationToken);
            }
            catch (XmlException exception)
            {
                Debug.Write("Failed to parse analyzer settings file. Using default settings. Exception: " + exception);
            }

            return AnalyzerSettingsRegistry.ImmutableEmpty;
        }

        private static bool IsSettingsFile([NotNull] string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            return string.Equals(fileName, SettingsFileName, StringComparison.OrdinalIgnoreCase);
        }

        [NotNull]
        private static TResult ReadSourceText<TResult>([NotNull] SourceText sourceText, [NotNull] Func<XmlReader, TResult> readAction,
            CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            sourceText.Write(writer, cancellationToken);
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            using var xmlReader = XmlReader.Create(stream);

            return readAction(xmlReader);
        }

        [NotNull]
        public static string ToFileContent([NotNull] AnalyzerSettingsRegistry registry)
        {
            Guard.NotNull(registry, nameof(registry));

            Encoding encoding = CreateEncoding();

            return GetStringForXml(encoding, writer =>
            {
                AnalyzerSettingsXmlConverter.WriteXml(registry, writer);
            });
        }

        [NotNull]
        private static string GetStringForXml([NotNull] Encoding encoding, [NotNull] Action<XmlWriter> writeAction)
        {
            using var stream = new MemoryStream();

            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = true
            });

            writeAction(writer);
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(stream, encoding, true, 1024, true);

            return reader.ReadToEnd();
        }

        [NotNull]
        public static Encoding CreateEncoding()
        {
            return new UTF8Encoding();
        }
    }
}
