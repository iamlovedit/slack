using System.Text;
using Slack.Commands;
using Slack.Infrastructure;
using Slack.Localization;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
var registry = new CommandRegistry();
registry.RegisterAllCommands();

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
    AnsiConsole.MarkupLine($"[red]{Strings.Format(Strings.UnknownCommand, commandName)}[/]");
    var help = registry.GetCommand("help");
    help?.Execute([]);
    return 1;
}
