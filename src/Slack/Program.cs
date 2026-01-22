using Slack.Commands;
using Spectre.Console;

if (args.Length == 0)
{
    ShowHelp();
    return 0;
}

var command = args[0].ToLower();

switch (command)
{
    case "up":
        return UpCommand.Execute();

    case "config":
        return ConfigCommand.Execute(args.Skip(1).ToArray());

    case "-h":
    case "--help":
    case "help":
        ShowHelp();
        return 0;

    default:
        AnsiConsole.MarkupLine($"[red]Unknown command: {command}[/]");
        ShowHelp();
        return 1;
}

static void ShowHelp()
{
    AnsiConsole.Write(new FigletText("slack").Color(Color.Green));
    AnsiConsole.WriteLine();

    var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("[bold]Command[/]")
        .AddColumn("[bold]Description[/]");

    table.AddRow("[cyan]up[/]", "让你看起来很忙！终端刷屏输出，假装在工作");
    table.AddRow("[cyan]config[/]", "配置技术栈、项目名称等参数");
    table.AddRow("[cyan]help[/]", "显示帮助信息");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[dim]Usage: slack <command> [[options]][/]");
    AnsiConsole.MarkupLine("[dim]Example: slack up[/]");
    AnsiConsole.MarkupLine("[dim]         slack config -i[/]");
}
