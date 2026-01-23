using Slack.Localization;
using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public class InitCommand : ICommand
{
    public string Name => "init";
    public string Description => Strings.InitDescription;
    public string[] Aliases => [];

    public int Execute(string[] args)
    {
        // 解析目标路径：支持 slack init 或 slack init <path>
        var targetPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

        // 转换为绝对路径
        if (!Path.IsPathRooted(targetPath))
        {
            targetPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), targetPath));
        }

        // 如果目标目录不存在，询问是否创建
        if (!Directory.Exists(targetPath))
        {
            Console.Clear();
            AnsiConsole.Write(new FigletText(Strings.InitTitle).Color(Color.Cyan1));
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]{Strings.Format(Strings.DirNotExist, targetPath)}[/]\n");

            if (!AnsiConsole.Confirm(Strings.CreateDirConfirm, true))
            {
                AnsiConsole.MarkupLine($"[red]{Strings.InitCancelled}[/]");
                return 1;
            }

            Directory.CreateDirectory(targetPath);
            AnsiConsole.MarkupLine($"[green]{Strings.Format(Strings.DirCreated, targetPath)}[/]\n");
        }

        var localConfigPath = SlackConfig.GetLocalConfigPath(targetPath);
        var configExists = File.Exists(localConfigPath);

        Console.Clear();
        AnsiConsole.Write(new FigletText(Strings.InitTitle).Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        if (configExists)
        {
            AnsiConsole.MarkupLine($"[yellow]{Strings.Format(Strings.ConfigExists, targetPath)}[/]\n");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(Strings.SelectAction)
                    .AddChoices([
                        Strings.ReconfigureOption,
                        Strings.ViewConfigOption,
                        Strings.CancelOption
                    ]));

            if (action == Strings.ReconfigureOption)
            {
                // Continue to reconfigure
            }
            else if (action == Strings.ViewConfigOption)
            {
                var existingConfig = SlackConfig.LoadLocal(targetPath);
                ShowConfig(existingConfig);
                AnsiConsole.WriteLine();
                if (!AnsiConsole.Confirm(Strings.ReconfigureConfirm, false))
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]{Strings.Format(Strings.WillCreateConfigAt, targetPath)}[/]");
            AnsiConsole.MarkupLine($"[dim]{Strings.Format(Strings.ConfigFilePath, localConfigPath)}[/]\n");
        }

        var config = RunConfigWizard(targetPath);
        config.SaveLocal(targetPath);
        ProjectRegistry.RegisterProject(targetPath, config.ProjectName);

        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold green]{Strings.ProjectInitialized}[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]{Strings.Format(Strings.ProjectPath, targetPath)}[/]\n");
        ShowConfig(config);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]{Strings.RunSlackUp}[/]");
        AnsiConsole.MarkupLine($"[dim]{Strings.RunSlackConfig}[/]");

        return 0;
    }

    private static SlackConfig RunConfigWizard(string targetPath)
    {
        var config = new SlackConfig();

        // 第一步：选择语言
        AnsiConsole.Write(new Rule($"[bold blue]{Strings.Step1SelectLanguage}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var languages = ConfigCommand.GetLanguages();
        config.Language = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SelectLanguagePrompt)
                .PageSize(12)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(languages));

        // 第二步：选择技术栈
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold blue]{Strings.Step2SelectTechStack}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var techStacks = ConfigCommand.GetTechStacks(config.Language);
        var selectedStack = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.Format(Strings.SelectTechStackPrompt, config.Language))
                .PageSize(10)
                .HighlightStyle(new Style(Color.Yellow))
                .AddChoices(techStacks));

        if (selectedStack == Strings.Custom)
        {
            selectedStack = AnsiConsole.Prompt(
                new TextPrompt<string>(Strings.EnterCustomTechStack)
                    .PromptStyle("green"));
        }

        config.TechStack = selectedStack;

        // 自动设置包管理器和运行时版本
        var (defaultManager, defaultVersion) = ConfigCommand.GetDefaultsForLanguage(config.Language, config.TechStack);
        config.PackageManager = defaultManager;
        config.RuntimeVersion = defaultVersion;

        // 第三步：项目信息
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold blue]{Strings.Step3ProjectInfo}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // 尝试从目标目录名推断项目名
        var currentDir = new DirectoryInfo(targetPath);
        var suggestedName = currentDir.Name.ToLower().Replace(" ", "-");

        config.ProjectName = AnsiConsole.Prompt(
            new TextPrompt<string>(Strings.ProjectNamePrompt)
                .DefaultValue(suggestedName)
                .PromptStyle("cyan"));

        config.EnvName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.RuntimeEnvPrompt)
                .AddChoices(["Production", "Staging", "Development", "CI/CD", "Local"]));

        // 可选：自定义包管理器和版本
        if (AnsiConsole.Confirm(Strings.CustomPackageManagerConfirm, false))
        {
            config.PackageManager = AnsiConsole.Prompt(
                new TextPrompt<string>(Strings.PackageManagerPrompt)
                    .DefaultValue(config.PackageManager)
                    .PromptStyle("magenta"));

            config.RuntimeVersion = AnsiConsole.Prompt(
                new TextPrompt<string>(Strings.RuntimeVersionPrompt)
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
            .AddColumn(new TableColumn($"[bold]{Strings.ConfigItem}[/]").Centered())
            .AddColumn(new TableColumn($"[bold]{Strings.Value}[/]").Centered());

        table.AddRow(Strings.ProjectName, $"[cyan]{config.ProjectName}[/]");
        table.AddRow(Strings.Language, $"[green]{config.Language}[/]");
        table.AddRow(Strings.TechStack, $"[yellow]{config.TechStack}[/]");
        table.AddRow(Strings.PackageManager, $"[magenta]{config.PackageManager}[/]");
        table.AddRow(Strings.RuntimeVersion, $"[blue]{config.RuntimeVersion}[/]");
        table.AddRow(Strings.Environment, $"[white]{config.EnvName}[/]");

        AnsiConsole.Write(table);
    }
}
