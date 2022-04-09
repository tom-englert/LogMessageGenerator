using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMessageGenerator.Test
{
    public class SourceTest
    {
        [Fact]
        public void Test()
        {
            var records = new CsvRecord[]
            {
                new() { Id = 1, Event = "EVENT", LogLevel = LogLevel.Information, Message = "This is the message" }
            };

            var sourceCode = Tools.GenerateSource(records);

        }
    }
}
