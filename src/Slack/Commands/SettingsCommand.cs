using Slack.Localization;
using Slack.Models;
using Spectre.Console;

using Slack.Infrastructure;

namespace Slack.Commands;

[AutoRegisterCommand]
public class SettingsCommand : ICommand
{
    public string Name => "settings";
    public string Description => Strings.SettingsDescription;
    public string[] Aliases => [];

    public int Execute(string[] args)
    {
        var config = GlobalConfig.Load();

        while (true)
        {
            Console.Clear();
            ShowCurrentSettings(config);

            var editOption = Strings.EditConfig;
            var backOption = Strings.CancelOption;

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]{Strings.SelectOperation}[/]")
                    .AddChoices([editOption, backOption]));

            if (choice == editOption)
            {
                EditSettings(config);
            }
            else
            {
                return 0;
            }
        }
    }

    private static void ShowCurrentSettings(GlobalConfig config)
    {
        AnsiConsole.Write(new FigletText("settings").Color(Color.Blue));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn($"[bold]{Strings.ConfigItem}[/]").Centered())
            .AddColumn(new TableColumn($"[bold]{Strings.CurrentValue}[/]").Centered());

        table.AddRow(Strings.Locale, $"[cyan]{config.Locale ?? Strings.LocaleAuto}[/]");
        table.AddRow(Strings.SettingsScrollSpeed, $"[cyan]{GetLocalizedSpeed(config.ScrollSpeed)}[/]");
        table.AddRow(Strings.SettingsTheme, $"[cyan]{GetLocalizedTheme(config.Theme)}[/]");
        table.AddRow(Strings.SettingsAutoUpdate, $"[cyan]{(config.AutoCheckUpdate ? Strings.Yes : Strings.No)}[/]");
        table.AddRow(Strings.SettingsAnimation, $"[cyan]{GetLocalizedAnimation(config.AnimationStyle)}[/]");

        AnsiConsole.Write(table);
    }

    private static void EditSettings(GlobalConfig config)
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]{Strings.SystemSettings}[/]"));
        AnsiConsole.WriteLine();

        // 1. 选择界面语言
        AnsiConsole.MarkupLine($"[bold]1. {Strings.Locale}[/]\n");
        var localeOptions = new List<string>
        {
            Strings.LocaleFollowSystem,
            "🇨🇳 中文 (zh-CN)",
            "🇺🇸 English (en-US)"
        };
        var selectedLocale = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SelectLocalePrompt)
                .AddChoices(localeOptions));
        config.Locale = selectedLocale == Strings.LocaleFollowSystem ? null :
            selectedLocale.Contains("zh-CN") ? "zh-CN" : "en-US";
        Strings.ResetLocale();

        // 2. 滚动速度
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]2. {Strings.SettingsScrollSpeed}[/]\n");
        var speedOptions = new Dictionary<string, string>
        {
            [Strings.OptionSlow] = "Slow",
            [Strings.OptionNormal] = "Normal",
            [Strings.OptionFast] = "Fast"
        };
        var selectedSpeed = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SettingsSelectSpeed)
                .AddChoices(speedOptions.Keys));
        config.ScrollSpeed = speedOptions[selectedSpeed];

        // 3. 颜色主题
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]3. {Strings.SettingsTheme}[/]\n");
        var themeOptions = new Dictionary<string, string>
        {
            [Strings.OptionDefault] = "Default",
            [Strings.OptionDark] = "Dark",
            [Strings.OptionMonokai] = "Monokai",
            [Strings.OptionMatrix] = "Matrix"
        };
        var selectedTheme = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SettingsSelectTheme)
                .AddChoices(themeOptions.Keys));
        config.Theme = themeOptions[selectedTheme];

        // 4. 自动检查更新
        AnsiConsole.WriteLine();
        config.AutoCheckUpdate = AnsiConsole.Confirm(Strings.SettingsAutoUpdatePrompt, config.AutoCheckUpdate);

        // 5. 动画风格
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]5. {Strings.SettingsAnimation}[/]\n");
        var animOptions = new Dictionary<string, string>
        {
            [Strings.OptionClassic] = "Classic",
            [Strings.OptionModern] = "Modern",
            [Strings.OptionMinimal] = "Minimal",
            [Strings.OptionFancy] = "Fancy"
        };
        var selectedAnim = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SettingsSelectAnimation)
                .AddChoices(animOptions.Keys));
        config.AnimationStyle = animOptions[selectedAnim];

        // 保存配置
        config.Save();

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[green]{Strings.ConfigSaved}[/]").RuleStyle("green"));
        Thread.Sleep(1500);
    }
    
    private static string GetLocalizedSpeed(string key) => key switch
    {
        "Slow" => Strings.OptionSlow,
        "Normal" => Strings.OptionNormal,
        "Fast" => Strings.OptionFast,
        _ => key
    };

    private static string GetLocalizedTheme(string key) => key switch
    {
        "Default" => Strings.OptionDefault,
        "Dark" => Strings.OptionDark,
        "Monokai" => Strings.OptionMonokai,
        "Matrix" => Strings.OptionMatrix,
        _ => key
    };

    private static string GetLocalizedAnimation(string key) => key switch
    {
        "Classic" => Strings.OptionClassic,
        "Modern" => Strings.OptionModern,
        "Minimal" => Strings.OptionMinimal,
        "Fancy" => Strings.OptionFancy,
        _ => key
    };
}
