using Slack.Localization;
using Spectre.Console;
using System.Reflection;

namespace Slack.Commands;

public class VersionCommand : ICommand
{
    public string Name => "version";
    public string Description => Strings.VersionDescription;
    public string[] Aliases => ["-v", "--version"];

    public int Execute(string[] args)
    {
        // AOT-safe way to get version
        var version = typeof(VersionCommand).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
        
        // Remove commit hash if present (e.g. 1.0.0+commit)
        if (version.Contains('+'))
        {
            version = version.Split('+')[0];
        }
        AnsiConsole.MarkupLine($"[green]slack[/] version [cyan]v{version}[/]");
        return 0;
    }
}
