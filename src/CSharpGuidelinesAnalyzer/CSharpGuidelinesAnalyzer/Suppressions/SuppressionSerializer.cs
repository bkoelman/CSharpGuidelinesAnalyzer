using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CSharpGuidelinesAnalyzer.Suppressions.Storage;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Suppressions
{
    public sealed class SuppressionSerializer
    {
        [NotNull]
        public string Serialize([NotNull] SuppressionRoot root)
        {
            Guard.NotNull(root, nameof(root));

            var tree = new XElement("CSharpGuidelinesAnalyzerSuppressions", new XAttribute("version", root.Version),
                new XElement("Rules", root.Rules.Select(SerializeRule)));

            return tree.ToString();
        }

        [NotNull]
        private XElement SerializeRule([NotNull] SuppressionRule rule)
        {
            return new XElement("Rule", new XAttribute("id", rule.Id),
                new XElement("Locations", rule.Locations.Select(SerializeLocation)));
        }

        [NotNull]
        private XElement SerializeLocation([NotNull] SuppressionLocation location)
        {
            return new XElement("Location", new XAttribute("relativeTo", location.RelativeTo),
                new XAttribute("value", location.Value), new XAttribute("line", location.Position.Line),
                new XAttribute("column", location.Position.Column));
        }

        [NotNull]
        public SuppressionRoot Deserialize([NotNull] string xml)
        {
            Guard.NotNull(xml, nameof(xml));

            XElement document = XElement.Parse(xml);

            Version version = Version.Parse(GetAttributeValue(document, "version"));
            IEnumerable<SuppressionRule> rules = GetElement(document, "Rules").Elements("Rule").Select(DeserializeRule);

            return new SuppressionRoot(version, rules);
        }

        [NotNull]
        private SuppressionRule DeserializeRule([NotNull] XElement ruleElement)
        {
            string id = GetAttributeValue(ruleElement, "id");
            IEnumerable<SuppressionLocation> locations =
                GetElement(ruleElement, "Locations").Elements("Location").Select(DeserializeLocation);

            return new SuppressionRule(id, locations);
        }

        [NotNull]
        private SuppressionLocation DeserializeLocation([NotNull] XElement locationElement)
        {
            var relativeTo =
                (SuppressionOffset)Enum.Parse(typeof(SuppressionOffset), GetAttributeValue(locationElement, "relativeTo"));
            string value = GetAttributeValue(locationElement, "value");
            int line = int.Parse(GetAttributeValue(locationElement, "line"));
            int column = int.Parse(GetAttributeValue(locationElement, "column"));

            return new SuppressionLocation(relativeTo, value, new SuppressionPosition(line, column));
        }

        [NotNull]
        private static XElement GetElement([NotNull] XElement element, [NotNull] string name)
        {
            XElement result = element.Element(name);
            if (result == null)
            {
                throw new FormatException($"Expected XML element '{name}' as child of '{element.Name}' element");
            }
            return result;
        }

        [NotNull]
        private static string GetAttributeValue([NotNull] XElement element, [NotNull] string name)
        {
            XAttribute result = element.Attribute(name);
            if (result == null)
            {
                throw new FormatException($"Expected XML attribute '{name}' on element '{element.Name}'");
            }
            return result.Value;
        }
    }
}
