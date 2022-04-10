﻿using Microsoft.CodeAnalysis;

static class CodeGenerator
{
    public static string GenerateSource(IEnumerable<CsvRecord> lines, Configuration configuration, Compilation? compilation = null)
    {
        var namespaceName = configuration.Namespace ?? compilation?.Assembly.Name ?? "LogMessageGenerator";
        var className = configuration.ClassName ?? "LogMessages";

        var source = new CodeBuilder();

        source
            .Add("/*")
            .Add("Configuration:")
            .Add($"  Namespace: '{configuration.Namespace}' => '{namespaceName}'")
            .Add($"  Classname: '{configuration.ClassName}' => '{className}'")
            .Add("*/");

        source
            .Add(@"#nullable enable")
            .Add(@"using System;")
            .Add(@"using Microsoft.Extensions.Logging;")
            .Add("");

        using (source.AddBlock("namespace {0}", namespaceName))
        {
            using (source.AddBlock($"public static class {className}"))
            {
                foreach (var line in lines)
                {
                    var message = line.Message;

                    var parameters = FormatStringParser.GetFormatParameters(ref message);
                    var paramTypes = string.Join(", ", parameters.Select(item => item.Type));
                    var paramNames = string.Join(", ", parameters.Select(item => item.Name));
                    var paramDefinitions = string.Join(", ", parameters.Select(item => item.Type + " " + item.Name));

                    source
                        .Add($"private static Action<ILogger, {Decorate(paramTypes, ", ")}Exception?> {line.Event}_Message = LoggerMessage.Define{Decorate(paramTypes, "<", ">")}(LogLevel.{line.LogLevel}, new EventId({line.Id}, \"{line.Event}\"), \"{message.Replace("\"", "\\\"")}\");");

                    using (source.AddBlock($"public static void Log_{line.Event}(this ILogger logger, {Decorate(paramDefinitions, ", ")}Exception? ex = null)"))
                    {
                        source.Add($"{line.Event}_Message(logger, {Decorate(paramNames, ", ")}ex);");
                    }
                }
            }
        }

        return source.ToString();
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

