using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

class MessageReader
{
    public int LineNumber { get; private set; }
    public int LineLength { get; private set; }

    public IEnumerable<CsvRecord> ReadLogMessages(string logMessages, CancellationToken cancellationToken)
    {
        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            DetectDelimiter = true,
        };

        using var reader = new CsvReader(new StringReader(logMessages), csvConfiguration);

        long offset = 0;

        while (reader.Read())
        {
            var charCount = reader.Parser.CharCount;
            LineNumber = reader.Parser.Row;
            LineLength = (int)(charCount - offset);
            offset = charCount;

            yield return reader.GetRecord<CsvRecord>();
        }
    }
}

