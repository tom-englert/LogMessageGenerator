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

        var sourceCode = CodeGenerator.GenerateSource(records);

        await Verify(sourceCode);
    }
}

