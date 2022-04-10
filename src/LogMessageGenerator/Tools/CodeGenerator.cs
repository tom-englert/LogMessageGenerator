using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CodeAnalysis;

static class CodeGenerator
{
    public static string GenerateSource(IEnumerable<CsvRecord> lines, Configuration configuration, Compilation? compilation = null)
    {
        var namespaceName = configuration.Namespace ?? compilation?.Assembly.Name ?? "LogMessageGenerator";
        var className = configuration.ClassName ?? "LogMessages";

        var sourceText = new StringBuilder();

        sourceText.AppendLine("/*");
        sourceText.AppendLine("Configuration:");
        sourceText.AppendLine($"  Namespace: '{configuration.Namespace}' => '{namespaceName}'");
        sourceText.AppendLine($"  Classname: '{configuration.ClassName}' => '{className}'");
        sourceText.AppendLine("*/");

        sourceText.AppendLine(@"#nullable enable");
        sourceText.AppendLine(@"using System;");
        sourceText.AppendLine(@"using Microsoft.Extensions.Logging;");
        sourceText.AppendLine();

        sourceText.AppendLine($"namespace {namespaceName}");
        sourceText.AppendLine(@"{");
        sourceText.AppendLine($"    public static class {className}");
        sourceText.AppendLine(@"    {");

        foreach (var line in lines)
        {
            var message = line.Message;

            var parameters = FormatStringParser.GetFormatParameters(ref message);
            message = message.Replace("\"", "\\\"");

            var paramTypes = string.Join(", ", parameters.Select(item => item.Type));
            var paramNames = string.Join(", ", parameters.Select(item => item.Name));
            var paramDefinitions = string.Join(", ", parameters.Select(item => item.Type + " " + item.Name));

            sourceText.AppendLine(
                $"        private static Action<ILogger, {Decorate(paramTypes, ", ")}Exception?> {line.Event}_Message = LoggerMessage.Define{Decorate(paramTypes, "<", ">")}(LogLevel.{line.LogLevel}, new EventId({line.Id}, \"{line.Event}\"), \"{message}\");");
            sourceText.AppendLine(
                $"        public static void Log_{line.Event}(this ILogger logger, {Decorate(paramDefinitions, ", ")}Exception? ex = null)");
            sourceText.AppendLine(@"        {");
            sourceText.AppendLine($"            {line.Event}_Message(logger, {Decorate(paramNames, ", ")}ex);");
            sourceText.AppendLine(@"        }");
        }

        sourceText.AppendLine(@"    }");
        sourceText.AppendLine(@"}");

        return sourceText.ToString();
    }

    static string Decorate(string value, string prefix, string suffix)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : prefix + value + suffix;
    }

    static string Decorate(string value, string suffix)
    {
        return Decorate(value, string.Empty, suffix);
    }
}

