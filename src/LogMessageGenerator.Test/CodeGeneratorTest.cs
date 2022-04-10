[UsesVerify]
public class CodeGeneratorTest
{
    [Fact]
    public async Task PlainMessageTest()
    {
        var records = new CsvRecord[]
        {
            new() { Id = 1, Event = "EVENT", LogLevel = LogLevel.Information, Message = "This is the message" }
        };

        var sourceCode = CodeGenerator.GenerateSource(records, Configuration.Default);

        await Verify(sourceCode);
    }

    [Fact]
    public async Task MessageWithParametersTest()
    {
        var records = new CsvRecord[]
        {
            new() { Id = 1, Event = "EVENT_EVENT", LogLevel = LogLevel.Information, Message = "This is the message {Param1}, {Param2@int}" }
        };

        var sourceCode = CodeGenerator.GenerateSource(records, Configuration.Default);

        await Verify(sourceCode);
    }
}

