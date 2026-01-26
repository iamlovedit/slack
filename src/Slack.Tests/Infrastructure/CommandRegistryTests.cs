using Xunit;
using Slack.Infrastructure;
using Slack.Tests.Helpers;

namespace Slack.Tests.Infrastructure;

public class CommandRegistryTests
{
    [Fact]
    public void Register_WithValidCommand_ShouldAddCommandByName()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command = new FakeCommand("test", "Test command");

        // Act
        registry.Register(command);

        // Assert
        var result = registry.GetCommand("test");
        Assert.NotNull(result);
        Assert.Same(command, result);
    }

    [Fact]
    public void Register_WithAliases_ShouldRegisterAllAliases()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command = new FakeCommand("test", "Test command", aliases: ["t", "tst"]);

        // Act
        registry.Register(command);

        // Assert
        Assert.Same(command, registry.GetCommand("test"));
        Assert.Same(command, registry.GetCommand("t"));
        Assert.Same(command, registry.GetCommand("tst"));
    }

    [Fact]
    public void GetCommand_WithCaseInsensitiveName_ShouldReturnCommand()
    {
        // Arrange
        // 注意：CommandRegistry 注册时使用原始 Name，但查询时会转换为小写
        // 所以命令名称应该是小写的，这样查询时才能匹配
        var registry = new CommandRegistry();
        var command = new FakeCommand("test", "Test command"); // 使用小写名称
        registry.Register(command);

        // Act & Assert - 查询时会转换为小写
        Assert.Same(command, registry.GetCommand("test"));
        Assert.Same(command, registry.GetCommand("TEST"));
        Assert.Same(command, registry.GetCommand("Test"));
    }

    [Fact]
    public void GetCommand_WithNonExistentName_ShouldReturnNull()
    {
        // Arrange
        var registry = new CommandRegistry();

        // Act
        var result = registry.GetCommand("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Register_WithDuplicateName_ShouldOverwritePreviousCommand()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command1 = new FakeCommand("test", "First command");
        var command2 = new FakeCommand("test", "Second command");

        // Act
        registry.Register(command1);
        registry.Register(command2);

        // Assert
        var result = registry.GetCommand("test");
        Assert.Same(command2, result);
    }

    [Fact]
    public void GetAllCommands_ShouldReturnDistinctCommands()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command1 = new FakeCommand("cmd1", "Command 1", aliases: ["c1"]);
        var command2 = new FakeCommand("cmd2", "Command 2", aliases: ["c2"]);
        registry.Register(command1);
        registry.Register(command2);

        // Act
        var allCommands = registry.GetAllCommands().ToList();

        // Assert
        Assert.Equal(2, allCommands.Count);
        Assert.Contains(command1, allCommands);
        Assert.Contains(command2, allCommands);
    }

    [Fact]
    public void GetAllCommands_WithAliases_ShouldNotDuplicateCommands()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command = new FakeCommand("test", "Test", aliases: ["t", "tst", "testing"]);
        registry.Register(command);

        // Act
        var allCommands = registry.GetAllCommands().ToList();

        // Assert
        Assert.Single(allCommands);
        Assert.Same(command, allCommands[0]);
    }

    [Fact]
    public void Register_WithEmptyAliases_ShouldOnlyRegisterByName()
    {
        // Arrange
        var registry = new CommandRegistry();
        var command = new FakeCommand("test", "Test command", aliases: []);

        // Act
        registry.Register(command);

        // Assert
        var allCommands = registry.GetAllCommands().ToList();
        Assert.Single(allCommands);
        Assert.NotNull(registry.GetCommand("test"));
    }

    [Fact]
    public void Register_MultipleCommands_ShouldMaintainAll()
    {
        // Arrange
        var registry = new CommandRegistry();
        var commands = Enumerable.Range(1, 10)
            .Select(i => new FakeCommand($"cmd{i}", $"Command {i}"))
            .ToList();

        // Act
        foreach (var cmd in commands)
        {
            registry.Register(cmd);
        }

        // Assert
        Assert.Equal(10, registry.GetAllCommands().Count());
        foreach (var cmd in commands)
        {
            Assert.Same(cmd, registry.GetCommand(cmd.Name));
        }
    }
}
