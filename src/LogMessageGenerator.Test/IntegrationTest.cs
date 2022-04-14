using Microsoft.Extensions.Logging;

namespace LogMessageGenerator.Test
{
    internal class IntegrationTest
    {
        void Test()
        {

            ILogger logger = default!;

            logger.LogInformation("Test");

            // => <ProjectReference Include="..\LogMessageGenerator\LogMessageGenerator.csproj" OutputItemType="Analyzer" />
            // even though we include the analyzers project with OutputItemType="Analyzer", most of the times the analyzer is not called or it fails internally.

            //logger.LogEVENT_1();
            //logger.LogEVENT_2("Text");
            //logger.LogEVENT_3(42);
        }
    }
}
