using System.Text;

static class FormatStringParser
{
    private static readonly char[] FormatDelimiters = { ',', ':', '@' };

    public static IReadOnlyList<FormatParameter> GetFormatParameters(ref string format)
    {
        var scanIndex = 0;
        var endIndex = format.Length;
        List<FormatParameter> formatParameters = new();
        List<(int Start, int Length)> typeDeclarations = new();

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
                var typeDelimiterIndex = FindIndexOf(format, '@', formatDelimiterIndex, closeBraceIndex);

                var namePart = format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1);
                // var formatPart = format.Substring(formatDelimiterIndex, typeDelimiterIndex - formatDelimiterIndex);
                var typePart = format.Substring(typeDelimiterIndex, closeBraceIndex - typeDelimiterIndex);
                if (string.IsNullOrEmpty(typePart))
                {
                    typePart = "string";
                }
                else
                {
                    typeDeclarations.Add((typeDelimiterIndex, typePart.Length));
                    typePart = typePart.TrimStart('@');
                }

                formatParameters.Add(new FormatParameter(namePart, typePart));

                scanIndex = closeBraceIndex + 1;
            }
        }

        if (typeDeclarations.Count > 0)
        {
            var newFormat = new StringBuilder(format);

            foreach (var (start, length) in typeDeclarations.AsEnumerable().Reverse())
            {
                newFormat.Remove(start, length);
            }

            format = newFormat.ToString();
        }

        return formatParameters.AsReadOnly();
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

