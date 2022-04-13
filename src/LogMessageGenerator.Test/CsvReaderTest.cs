[UsesVerify]
public class CsvReaderTest
{
    private readonly MessageReader _messageReader = new();

    [Theory]
    [InlineData("")]
    [InlineData("1,EVENT_1,Information,The message")]
    [InlineData("1,EVENT_1,Debug,\"The message\"")]
    [InlineData("1,EVENT_1,Warning,\"The \"\"message\"\"\"")]
    public async Task ReadProperCsvTest(string text)
    {
        var records = _messageReader.ReadLogMessages(text);

        await Verify(records).UseParameters(text);
    }

    [Theory]
    [InlineData("1,EVENT_1,Information,The message,something more")]
    [InlineData("1,EVENT_1,Information,\"The \"\"message\"\"")]
    public async Task ReadBuggyCsvTest(string text)
    {
        var records = _messageReader.ReadLogMessages(text);

        await Verify(records).UseParameters(text);
    }


    [Theory]
    [InlineData("1,EVENT_1", 1)]
    [InlineData("1,EVENT_1,Test,Message", 1)]
    [InlineData("1, EVENT_1, Information, The message\r\n1,EVENT_1,Test,Message", 2)]
    [InlineData("1, EVENT_1, Information, The message\r\n\r\n1,EVENT_1,Test,Message", 3)]
    public void ReadInvalidCsvTest(string text, int lineNumber)
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
        {
            _ = _messageReader.ReadLogMessages(text).ToList();
        });

        Assert.StartsWith("CsvHelper", ex.GetType().Namespace);
        Assert.Equal(lineNumber, _messageReader.LineNumber);
    }
}

