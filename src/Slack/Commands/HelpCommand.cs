using Slack.Localization;
using Spectre.Console;

namespace Slack.Commands;

public class HelpCommand : ICommand
{
    private readonly CommandRegistry _registry;

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry;
    }

    public string Name => "help";
    public string Description => Strings.HelpDescription;
    public string[] Aliases => ["-h", "--help"];

    public int Execute(string[] args)
    {
        AnsiConsole.Write(new FigletText("slack").Color(Color.Green));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn($"[bold]{Strings.Command}[/]")
            .AddColumn($"[bold]{Strings.Description}[/]");

        foreach (var command in _registry.GetAllCommands())
        {
            var aliases = command.Aliases.Length > 0 ? $", {string.Join(", ", command.Aliases)}" : "";
            table.AddRow($"[cyan]{command.Name}{aliases}[/]", command.Description);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]{Strings.Usage}: slack <command> [[options]][/]");
        AnsiConsole.MarkupLine($"[dim]{Strings.Example}: slack up[/]");
        AnsiConsole.MarkupLine("[dim]         slack config -i[/]");

        return 0;
    }
}
