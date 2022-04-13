using Microsoft.Extensions.Logging;

namespace LogMessageGenerator.Test
{
    internal class IntegrationTest
    {
        private static Action<ILogger, Exception?> EVENT_1_Message = LoggerMessage.Define(LogLevel.Information, new EventId(1, "EVENT_1"), "some text");

        void Test()
        {

            ILogger logger = default!;

            logger.LogInformation("Test");

            // logger.Log
        }
    }
}
