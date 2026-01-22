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
    public string Description => "显示帮助信息";
    public string[] Aliases => ["-h", "--help"];

    public int Execute(string[] args)
    {
        AnsiConsole.Write(new FigletText("slack").Color(Color.Green));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Command[/]")
            .AddColumn("[bold]Description[/]");

        foreach (var command in _registry.GetAllCommands())
        {
            var aliases = command.Aliases.Length > 0 ? $", {string.Join(", ", command.Aliases)}" : "";
            table.AddRow($"[cyan]{command.Name}{aliases}[/]", command.Description);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Usage: slack <command> [[options]][/]");
        AnsiConsole.MarkupLine("[dim]Example: slack up[/]");
        AnsiConsole.MarkupLine("[dim]         slack config -i[/]");

        return 0;
    }
}
