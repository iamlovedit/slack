using Slack.Infrastructure;

namespace Slack.Tests.Helpers;

/// <summary>
/// 用于测试的假命令实现
/// </summary>
public class FakeCommand : ICommand
{
    public string Name { get; }
    public string Description { get; }
    public string[] Aliases { get; }

    private readonly Func<string[], int>? _executeFunc;

    public FakeCommand(
        string name,
        string description = "Fake command for testing",
        string[]? aliases = null,
        Func<string[], int>? executeFunc = null)
    {
        Name = name;
        Description = description;
        Aliases = aliases ?? [];
        _executeFunc = executeFunc;
    }

    public int Execute(string[] args)
    {
        return _executeFunc?.Invoke(args) ?? 0;
    }
}
