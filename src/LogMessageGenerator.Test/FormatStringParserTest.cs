[UsesVerify]
public class FormatStringParserTest
{
    [Theory]
    [InlineData(@"This is {Parameter1} and {Parameter2}")]
    [InlineData(@"This is {Parameter1} {{and}} {{{Integer@int}}}")]
    [InlineData(@"This is {Parameter1}, {Integer,4:5@int}")]
    [InlineData(@"This is {Dooublie@double} # ""{Integer,4:5@int}""")]
    public async Task Test(string message)
    {
        var parameters = FormatStringParser.GetFormatParameters(ref message);
        var result = new { Message = message, Parameters = parameters };

        await Verify(result).UseParameters(message);
    }
}

