using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Diagnostics;

[Serializable]
public class Configuration
{
    private static readonly XmlSerializer Serializer = new(typeof(Configuration));

    public static Configuration Default => new();

    public string? Namespace { get; set; }

    public string? ClassName { get; set; }

    public string? DebugOutput { get; set; }

    public static Configuration Read(AnalyzerConfigOptions configOptions)
    {
        try
        {
            if (configOptions.TryGetValue("build_metadata.additionalfiles.Configuration", out var configuration) &&
                !string.IsNullOrEmpty(configuration))
            {
                return Deserialize(configuration);
            }
        }
        catch
        {
            // just go with default options
        }

        return new Configuration();
    }

    private static Configuration Deserialize(string configuration)
    {
        using var stringReader = new StringReader($"<Configuration>{configuration}</Configuration>");
        using var xmlReader = new CaseInsensitiveXmlReader(stringReader);

        return (Configuration)Serializer.Deserialize(xmlReader);
    }

    private class CaseInsensitiveXmlReader : XmlTextReader
    {
        public CaseInsensitiveXmlReader(TextReader reader) : base(reader) { }

        public override string ReadElementString()
        {
            var text = base.ReadElementString();

            // bool TryParse accepts case-insensitive 'true' and 'false'
            if (bool.TryParse(text, out var result))
            {
                text = XmlConvert.ToString(result);
            }

            return text;
        }
    }
}