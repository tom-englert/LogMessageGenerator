using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class LogMessageSourceGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor ExceptionDescriptor = new("LOGG", "LogMessageGenerator", "Exception {0}", "SourceGenerator", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvokedDescriptor = new("LOGG", "LogMessageGenerator", "Generator invoked: '{0}'", "SourceGenerator", DiagnosticSeverity.Warning, true);
    private static readonly DiagnosticDescriptor SourceGeneratedDescriptor = new("LOGG", "LogMessageGenerator", "Source generated: '{0}'", "SourceGenerator", DiagnosticSeverity.Warning, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var textFiles = context.AdditionalTextsProvider
            .Where(source => source.Path.EndsWith("LogMessages.csv", StringComparison.OrdinalIgnoreCase));

        var parameters = textFiles
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Combine(context.CompilationProvider)
            .Select((((AdditionalText, AnalyzerConfigOptionsProvider), Compilation) args, CancellationToken _) =>
            {
                var ((text, options), compilation) = args;

                options.GetOptions(text)
                    .TryGetValue("build_metadata.additionalfiles.Configuration", out var configuration);

                return (text.Path, text.GetText()?.ToString(), configuration, compilation.AssemblyName);
            });

        context.RegisterImplementationSourceOutput(parameters, GenerateSource);
    }

    private void GenerateSource(SourceProductionContext context, (string path, string? text, string? options, string? assemblyName) args)
    {
        var (path, text, options, assemblyName) = args;

        context.ReportDiagnostic(Diagnostic.Create(InvokedDescriptor, Location.None, path));

        if (text == null)
            return;

        var reader = new MessageReader();

        try
        {
            var logMessages = reader.ReadLogMessages(text);

            var configuration = Configuration.Read(options);

            var generatedSource = CodeGenerator.GenerateSource(logMessages, configuration, assemblyName);

            if (!string.IsNullOrEmpty(configuration.DebugOutput))
            {
                File.WriteAllText(configuration.DebugOutput, generatedSource);
            }

            context.AddSource("LogMessages.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
            context.ReportDiagnostic(Diagnostic.Create(SourceGeneratedDescriptor, Location.None, configuration.DebugOutput));
        }
        catch (Exception ex)
        {
            var linePosition = new LinePosition(reader.LineNumber - 1, 0);

            context.ReportDiagnostic(Diagnostic.Create(ExceptionDescriptor, Location.Create(path, new TextSpan(0, text.Length), new LinePositionSpan(linePosition, linePosition)), ex.Message));
        }
    }
}

