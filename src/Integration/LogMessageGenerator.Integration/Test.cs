using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using VerifyXunit;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LogMessageGenerator.Integration
{
    [UsesVerify]
    public class Test
    {
        [Fact]
        public async Task IntegrationTest()
        {
            using var context = new Context();
            var logger = context.CreateLogger();

            logger.Log_EVENT_1();
            logger.Log_EVENT_2("TextParam");
            logger.Log_EVENT_3(42);
            logger.Log_EVENT_4(12.3);

            await Verifier.Verify(context.GetLogAsync()).ConfigureAwait(false);
        }

        private sealed class Context : IDisposable
        {
            private static readonly Regex DateTimeRegex = new Regex(@"[\d-]+T[\d:.]+Z");

            private readonly string _fileName = Path.GetTempFileName();
            private readonly ILoggerFactory _loggerFactory;

            public Context()
            {
                var logger = new LoggerConfiguration()
                    .WriteTo.File(new CompactJsonFormatter(), _fileName)
                    .CreateLogger();

                _loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(logger, true));
            }

            public async Task<string> GetLogAsync()
            {
                _loggerFactory.Dispose();

                return DateTimeRegex.Replace(await File.ReadAllTextAsync(_fileName).ConfigureAwait(false), "$TimeStamp");
            }

            public ILogger CreateLogger()
            {
                return _loggerFactory.CreateLogger("UnitTestLogger");
            }

            public void Dispose()
            {
                _loggerFactory.Dispose();

                File.Delete(_fileName);
            }
        }
    }
}
