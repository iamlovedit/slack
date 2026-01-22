namespace Slack.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string[] Aliases { get; }
    int Execute(string[] args);
}
