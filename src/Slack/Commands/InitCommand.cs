using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public static class InitCommand
{
    public static int Execute()
    {
        var localConfigPath = Path.Combine(Directory.GetCurrentDirectory(), ".slack", "config.json");
        var configExists = File.Exists(localConfigPath);

        Console.Clear();
        AnsiConsole.Write(new FigletText("slack init").Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        if (configExists)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ 当前目录已存在 .slack 配置[/]\n");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择操作:")
                    .AddChoices(["📝 重新配置", "👀 查看当前配置", "🚪 取消"]));

            switch (action)
            {
                case "📝 重新配置":
                    break;
                case "👀 查看当前配置":
                    var existingConfig = SlackConfig.LoadLocal();
                    ShowConfig(existingConfig);
                    AnsiConsole.WriteLine();
                    if (!AnsiConsole.Confirm("是否重新配置?", false))
                    {
                        return 0;
                    }
                    break;
                case "🚪 取消":
                    return 0;
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]将在当前目录创建配置: [/][cyan]{localConfigPath}[/]\n");
        }

        var config = RunConfigWizard();
        config.SaveLocal();

        Console.Clear();
        AnsiConsole.Write(new Rule("[bold green]✓ 项目配置已初始化[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();
        ShowConfig(config);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]运行 [cyan]slack up[/] 开始摸鱼！[/]");
        AnsiConsole.MarkupLine("[dim]运行 [cyan]slack config[/] 修改配置[/]");

        return 0;
    }

    private static SlackConfig RunConfigWizard()
    {
        var config = new SlackConfig();

        // 第一步：选择语言
        AnsiConsole.Write(new Rule("[bold blue]第 1 步：选择编程语言[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var languages = ConfigCommand.GetLanguages();
        config.Language = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("选择你正在使用的语言:")
                .PageSize(12)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(languages));

        // 第二步：选择技术栈
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]第 2 步：选择技术栈[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var techStacks = ConfigCommand.GetTechStacks(config.Language);
        var selectedStack = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"选择 [cyan]{config.Language}[/] 技术栈:")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Yellow))
                .AddChoices(techStacks));

        if (selectedStack == "自定义...")
        {
            selectedStack = AnsiConsole.Prompt(
                new TextPrompt<string>("请输入你的技术栈名称:")
                    .PromptStyle("green"));
        }

        config.TechStack = selectedStack;

        // 自动设置包管理器和运行时版本
        var (defaultManager, defaultVersion) = ConfigCommand.GetDefaultsForLanguage(config.Language, config.TechStack);
        config.PackageManager = defaultManager;
        config.RuntimeVersion = defaultVersion;

        // 第三步：项目信息
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]第 3 步：项目信息[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // 尝试从目录名推断项目名
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var suggestedName = currentDir.Name.ToLower().Replace(" ", "-");

        config.ProjectName = AnsiConsole.Prompt(
            new TextPrompt<string>("项目名称:")
                .DefaultValue(suggestedName)
                .PromptStyle("cyan"));

        config.EnvName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("运行环境:")
                .AddChoices(["Production", "Staging", "Development", "CI/CD", "Local"]));

        // 可选：自定义包管理器和版本
        if (AnsiConsole.Confirm("需要自定义包管理器和运行时版本吗?", false))
        {
            config.PackageManager = AnsiConsole.Prompt(
                new TextPrompt<string>("包管理器:")
                    .DefaultValue(config.PackageManager)
                    .PromptStyle("magenta"));

            config.RuntimeVersion = AnsiConsole.Prompt(
                new TextPrompt<string>("运行时版本:")
                    .DefaultValue(config.RuntimeVersion)
                    .PromptStyle("blue"));
        }

        return config;
    }

    private static void ShowConfig(SlackConfig config)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn(new TableColumn("[bold]配置项[/]").Centered())
            .AddColumn(new TableColumn("[bold]值[/]").Centered());

        table.AddRow("🏷️  项目名称", $"[cyan]{config.ProjectName}[/]");
        table.AddRow("💻 语言", $"[green]{config.Language}[/]");
        table.AddRow("🛠️  技术栈", $"[yellow]{config.TechStack}[/]");
        table.AddRow("📦 包管理器", $"[magenta]{config.PackageManager}[/]");
        table.AddRow("🔢 运行时版本", $"[blue]{config.RuntimeVersion}[/]");
        table.AddRow("🌍 环境", $"[white]{config.EnvName}[/]");

        AnsiConsole.Write(table);
    }
}
