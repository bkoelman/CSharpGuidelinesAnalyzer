using System.Collections.Generic;
using System.Xml;

namespace CSharpGuidelinesAnalyzer.Settings;

internal static class AnalyzerSettingsXmlConverter
{
    public static AnalyzerSettingsRegistry ParseXml(XmlReader reader)
    {
        var settings = new AnalyzerSettingsRegistry();

        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, Name: "setting" })
            {
                ParseSettingElement(reader, settings);
            }
        }

        return settings.IsEmpty ? AnalyzerSettingsRegistry.ImmutableEmpty : settings;
    }

    private static void ParseSettingElement(XmlReader reader, AnalyzerSettingsRegistry registry)
    {
        string? rule = reader.GetAttribute("rule");
        string? name = reader.GetAttribute("name");
        string? value = reader.GetAttribute("value");

        if (!string.IsNullOrWhiteSpace(rule) && !string.IsNullOrWhiteSpace(name))
        {
            registry.Add(rule, name, value);
        }
    }

    public static void WriteXml(AnalyzerSettingsRegistry registry, XmlWriter writer)
    {
        writer.WriteStartElement("cSharpGuidelinesAnalyzerSettings");

        foreach (KeyValuePair<AnalyzerSettingKey, string> setting in registry.GetAll())
        {
            WriteSettingElement(setting.Key, setting.Value, writer);
        }

        writer.WriteEndElement();
    }

    private static void WriteSettingElement(AnalyzerSettingKey settingKey, string settingValue, XmlWriter writer)
    {
        writer.WriteStartElement("setting");

        writer.WriteAttributeString("rule", settingKey.Rule);
        writer.WriteAttributeString("name", settingKey.Name);
        writer.WriteAttributeString("value", settingValue);

        writer.WriteEndElement();
    }
}
