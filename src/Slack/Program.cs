using Slack.Commands;
using Spectre.Console;

var registry = new CommandRegistry();
registry.Register(new UpCommand());
registry.Register(new InitCommand());
registry.Register(new ConfigCommand());
registry.Register(new VersionCommand());
registry.Register(new HelpCommand(registry));

if (args.Length == 0)
{
    var help = registry.GetCommand("help");
    return help?.Execute([]) ?? 1;
}

var commandName = args[0].ToLower();
var command = registry.GetCommand(commandName);

if (command != null)
{
    // Pass remaining arguments to the command
    return command.Execute(args.Skip(1).ToArray());
}
else
{
    AnsiConsole.MarkupLine($"[red]Unknown command: {commandName}[/]");
    var help = registry.GetCommand("help");
    help?.Execute([]);
    return 1;
}
