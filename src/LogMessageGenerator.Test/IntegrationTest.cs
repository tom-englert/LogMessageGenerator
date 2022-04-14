using Microsoft.Extensions.Logging;

namespace LogMessageGenerator.Test
{
    internal class IntegrationTest
    {
        void Test()
        {

            ILogger logger = default!;

            logger.LogInformation("Test");

            logger.LogEVENT_1();
            logger.LogEVENT_2("Text");
            logger.LogEVENT_3(42);
        }
    }
}
