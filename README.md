# LogMessageGenerator

This project is a sample how to setup an incremental source generator project.
It's not meant to be a replacement for https://docs.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator

It shows how to

- setup the solution
- include NuGet dependencies in the generator project
- build the NuGet package
- reference the generator in a test project
- setup an integration test project that consumes the analyzers NuGet package

- consume additional files in the generator
- consume options from MSBuild variables in the generator
