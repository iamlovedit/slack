using Spectre.Console;

namespace Slack.Commands;

public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = [];

    public void Register(ICommand command)
    {
        _commands[command.Name] = command;
        foreach (var alias in command.Aliases)
        {
            _commands[alias] = command;
        }
    }

    public ICommand? GetCommand(string name)
    {
        return _commands.GetValueOrDefault(name.ToLower());
    }

    public IEnumerable<ICommand> GetAllCommands()
    {
        return _commands.Values.Distinct();
    }
}
