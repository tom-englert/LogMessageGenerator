namespace LogMessageGenerator.Test
{
    [UsesVerify]
    public class LogParametersTest
    {
        [Fact]
        public async Task Test1()
        {
            var result = Tools.GetLogParameters("This is {Message} # {2,4:5@int}");

            await Verify(result);
        }
    }
}
