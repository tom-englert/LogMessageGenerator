using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LogMessageGenerator
{
    [Generator]
    public class LogMessageSourceGenerator : ISourceGenerator
    {
        public static void Main()
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            File.AppendAllText(@"c:\temp\test.log", $"{DateTime.Now}: Execute called");

            var cancellationToken = context.CancellationToken;

            var sourceFile = context.AdditionalFiles
                .FirstOrDefault(file => file.Path.EndsWith("LogMessages.csv", StringComparison.OrdinalIgnoreCase));

            var source = sourceFile?.GetText(cancellationToken)?.ToString();
            if (source == null)
                return;

            try
            {
                var logMessages = Tools.ReadLogMessages(source, cancellationToken);

                context.AnalyzerConfigOptions.GetOptions(sourceFile)
                    .TryGetValue("build_metadata.additionalfiles.Configuration", out var config);

                var generatedSource = Tools.GenerateSource(logMessages, config, context.Compilation);

                File.WriteAllText(@"c:\temp\generated.cs", generatedSource);

                context.AddSource("LogMessages.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                var message = ex.GetType() + ": " + ex;
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOGC", "LogMessageGenerator", "Exception {0}", "SourceGenerator", DiagnosticSeverity.Error, true), Location.Create(sourceFile.Path, new TextSpan(), new LinePositionSpan()), message));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
