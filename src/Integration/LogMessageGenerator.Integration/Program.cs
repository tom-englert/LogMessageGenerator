using System;
using LogMessageGenerator.Integration;
using Microsoft.Extensions.Logging;
using TODO_LogMessageGenerator;

var logger = LoggerFactory.Create((builder => builder.AddConsole())).CreateLogger("Test");

Console.WriteLine("Hello, World!");

logger.LogInformation("This is the {Message}", "message");


logger.Log_FILE_SKIPPED("message");

logger.Log_EVENT_1();
logger.Log_EVENT_2("The message");

