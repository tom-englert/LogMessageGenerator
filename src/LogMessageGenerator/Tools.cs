using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.CodeAnalysis;

namespace LogMessageGenerator
{
    public class LogParameter
    {
        public LogParameter(string name, string format, string type)
        {
            Name = name;
            Format = format;
            Type = type;
        }

        public string Name { get; }
        public string Format { get; }
        public string Type { get; }
    }

    public static class Tools
    {
        public static IReadOnlyCollection<CsvRecord> ReadLogMessages(string logMessages, CancellationToken cancellationToken)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                DetectDelimiter = true,
                DetectDelimiterValues = new[] { ",", ";" }
            };

            using var csv = new CsvReader(new StringReader(logMessages), csvConfiguration);

            return csv.GetRecords<CsvRecord>().ToList().AsReadOnly();
        }

        public static string GenerateSource(IReadOnlyCollection<CsvRecord> items, string? config = null, Compilation? compilation = null)
        {
            var namespaceName = "TODO_LogMessageGenerator"; 
            var className = "LogMessages";

            var sourceText = new StringBuilder();

            sourceText.AppendLine("/*");
            sourceText.AppendLine($"Assembly: {compilation?.Assembly.Name}");
            sourceText.AppendLine($"Config: {config}");
            sourceText.AppendLine("*/");

            sourceText.AppendLine(@"#nullable enable");
            sourceText.AppendLine(@"using System;");
            sourceText.AppendLine(@"using Microsoft.Extensions.Logging;");
            sourceText.AppendLine();

            sourceText.AppendLine($"namespace {namespaceName}");
            sourceText.AppendLine(@"{");
            sourceText.AppendLine($"    public static class {className}");
            sourceText.AppendLine(@"    {");

            foreach (var item in items)
            {
                var parameters = GetLogParameters(item.Message);
                var paramTypes = string.Join(", ", parameters.Select(item => item.Type));
                var paramNames = string.Join(", ", parameters.Select(item => item.Name));
                var paramDefinitions = string.Join(", ", parameters.Select(item => item.Type + " " + item.Name));

                string Decorate(string value, string prefix, string suffix)
                {
                    return string.IsNullOrEmpty(value) ? string.Empty : prefix + value + suffix;
                }

                sourceText.AppendLine(
                    $"        private static Action<ILogger, {Decorate(paramTypes, "", ", ")}Exception?> {item.Event}_Message = LoggerMessage.Define{Decorate(paramTypes, "<", ">")}(LogLevel.{item.LogLevel}, new EventId({item.Id}, \"{item.Event}\"), \"{item.Message}\");");
                sourceText.AppendLine(
                    $"        public static void Log_{item.Event}(this ILogger logger, {Decorate(paramDefinitions, "", ", ")}Exception? ex = null)");
                sourceText.AppendLine(@"        {");
                sourceText.AppendLine($"            {item.Event}_Message(logger, {Decorate(paramNames, "", ", ")}ex);");
                sourceText.AppendLine(@"        }");
            }

            sourceText.AppendLine(@"    }");
            sourceText.AppendLine(@"}");

            return sourceText.ToString();
        }

        /// <summary>
        /// Formatter to convert the named format items like {NamedFormatItem} to <see cref="M:string.Format"/> format.
        /// </summary>
        private static readonly char[] FormatDelimiters = { ',', ':', '@' };

        public static IReadOnlyList<LogParameter> GetLogParameters(string format)
        {
            var scanIndex = 0;
            var endIndex = format.Length;
            List<LogParameter> valueNames = new();

            while (scanIndex < endIndex)
            {
                var openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                var closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                if (closeBraceIndex == endIndex)
                {
                    scanIndex = endIndex;
                }
                else
                {
                    // Format item syntax : { index[,alignment][ :formatString][ @parameterType] }.
                    var formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);
                    var typeDelimiterIndex = FindIndexOf(format, '@', openBraceIndex, closeBraceIndex);

                    var namePart = format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1);
                    var formatPart = format.Substring(formatDelimiterIndex, typeDelimiterIndex - formatDelimiterIndex);
                    var typePart = format.Substring(typeDelimiterIndex, closeBraceIndex - typeDelimiterIndex).TrimStart('@');
                    if (string.IsNullOrEmpty(typePart))
                    {
                        typePart = "string";
                    }

                    valueNames.Add(new LogParameter(namePart, formatPart, typePart));

                    scanIndex = closeBraceIndex + 1;
                }
            }

            return valueNames.AsReadOnly();
        }

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            var braceIndex = endIndex;
            var scanIndex = startIndex;
            var braceOccurrenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurrenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                        braceOccurrenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurrenceCount == 0)
                        {
                            // For '}' pick the first occurrence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurrence.
                        braceIndex = scanIndex;
                    }

                    braceOccurrenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOf(string format, char chr, int startIndex, int endIndex)
        {
            return FindIndexOfAny(format, new[] { chr }, startIndex, endIndex);
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            var findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }
    }
}
