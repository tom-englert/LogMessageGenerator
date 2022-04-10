using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class LogMessageSourceGenerator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor ExceptionDescriptor = new("LOGG", "LogMessageGenerator", "Exception {0}", "SourceGenerator", DiagnosticSeverity.Error, true);

    public void Execute(GeneratorExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        var sourceFile = context.AdditionalFiles
            .FirstOrDefault(file => file.Path.EndsWith("LogMessages.csv", StringComparison.OrdinalIgnoreCase));
        var sourceText = sourceFile?.GetText(cancellationToken);
        var source = sourceText?.ToString();
        if (source == null)
            return;

        var reader = new MessageReader();

        try
        {
            var logMessages = reader.ReadLogMessages(source, cancellationToken);

            var configuration = Configuration.Read(context.AnalyzerConfigOptions.GetOptions(sourceFile));

            var generatedSource = CodeGenerator.GenerateSource(logMessages, configuration, context.Compilation);

            if (!string.IsNullOrEmpty(configuration.DebugOutput))
            {
                File.WriteAllText(configuration.DebugOutput, generatedSource);
            }

            context.AddSource("LogMessages.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            var linePosition = new LinePosition(reader.LineNumber - 1, 0);

            context.ReportDiagnostic(Diagnostic.Create(ExceptionDescriptor, Location.Create(sourceFile.Path, new TextSpan(0, sourceText.Length), new LinePositionSpan(linePosition, linePosition)), ex.Message));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}

