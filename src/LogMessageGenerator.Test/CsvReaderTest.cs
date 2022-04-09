namespace LogMessageGenerator.Test
{
    [UsesVerify]
    public class CsvReaderTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("1,EVENT_1,Information,The message")]
        [InlineData("1,EVENT_1,Debug,\"The message\"")]
        [InlineData("1,EVENT_1,Warning,\"The \"\"message\"\"\"")]
        public async Task ReadProperCsvTest(string text)
        {
            var records = Tools.ReadLogMessages(text, CancellationToken.None);

            await Verify(records).UseParameters(text);
        }

        [Theory]
        [InlineData("1,EVENT_1,Information,The message,something more")]
        [InlineData("1,EVENT_1,Information,\"The \"\"message\"\"")]
        public async Task ReadBuggyCsvTest(string text)
        {
            var records = Tools.ReadLogMessages(text, CancellationToken.None);

            await Verify(records).UseParameters(text);
        }


        [Theory]
        [InlineData("1,EVENT_1")]
        [InlineData("1,EVENT_1,Test,Message")]
        public void ReadInvalidCsvTest(string text)
        {
            var ex = Assert.ThrowsAny<Exception>(() =>
            {
                var records = Tools.ReadLogMessages(text, CancellationToken.None);
            });

            Assert.StartsWith("CsvHelper", ex.GetType().Namespace);
        }
    }
}
