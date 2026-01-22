using Slack.Localization;
using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public class ConfigCommand : ICommand
{
    public string Name => "config";
    public string Description => Strings.ConfigDescription;
    public string[] Aliases => [];

    public static string[] GetLanguages() => TechStacksByLanguage.Keys.ToArray();
    public static string[] GetTechStacks(string language) => TechStacksByLanguage.GetValueOrDefault(language, []);
    public static (string manager, string version) GetDefaultsForLanguage(string language, string stack) => GetDefaults(language, stack);

    // 语言 -> 技术栈映射
    private static readonly Dictionary<string, string[]> TechStacksByLanguage = new()
    {
        ["C#"] = ["ASP.NET Core Web API", "Blazor", "MAUI", "Console App", "WPF", "gRPC Service", "Unity Game", "Custom..."],
        ["TypeScript"] = ["Next.js", "React + Vite", "Vue 3", "NestJS", "Express", "Nuxt", "Angular", "Svelte", "Deno", "Custom..."],
        ["JavaScript"] = ["React", "Vue", "Express", "Fastify", "Electron", "Node.js CLI", "Custom..."],
        ["Python"] = ["FastAPI", "Django", "Flask", "Celery Worker", "CLI Tool", "PyTorch ML", "Scrapy", "Custom..."],
        ["Go"] = ["Gin", "Echo", "Fiber", "gRPC", "CLI Tool", "Kubernetes Operator", "Custom..."],
        ["Rust"] = ["Actix-web", "Axum", "Rocket", "Tokio", "CLI Tool", "Tauri", "WASM", "Custom..."],
        ["Java"] = ["Spring Boot", "Quarkus", "Micronaut", "Gradle Plugin", "Android", "Custom..."],
        ["Kotlin"] = ["Ktor", "Spring Boot", "Android", "Compose Desktop", "Compose Multiplatform", "Custom..."],
        ["Swift"] = ["iOS App", "macOS App", "SwiftUI", "Vapor", "CLI Tool", "Custom..."],
        ["PHP"] = ["Laravel", "Symfony", "WordPress Plugin", "Slim", "Custom..."],
        ["Ruby"] = ["Ruby on Rails", "Sinatra", "Jekyll", "CLI Tool", "Custom..."],
        ["C++"] = ["Qt", "Unreal Engine", "CLI Tool", "Embedded", "CMake Project", "Custom..."],
        ["C"] = ["Embedded", "Linux Kernel", "CLI Tool", "Custom..."],
        ["Scala"] = ["Play Framework", "Akka", "Spark Job", "ZIO", "Custom..."],
        ["Elixir"] = ["Phoenix", "LiveView", "Nerves IoT", "CLI Tool", "Custom..."],
        ["Dart"] = ["Flutter", "Flutter Web", "CLI Tool", "Custom..."],
        ["Lua"] = ["Love2D Game", "Neovim Plugin", "OpenResty", "Custom..."],
        ["Zig"] = ["CLI Tool", "Embedded", "WASM", "Custom..."],
        ["Haskell"] = ["Yesod", "Servant", "CLI Tool", "Custom..."],
        ["Clojure"] = ["Luminus", "Ring", "ClojureScript", "Custom..."],
        ["F#"] = ["Giraffe", "Saturn", "SAFE Stack", "CLI Tool", "Custom..."],
        ["OCaml"] = ["Dream", "CLI Tool", "Custom..."],
        ["Nim"] = ["Jester", "CLI Tool", "Custom..."],
        ["V"] = ["Vweb", "CLI Tool", "Custom..."],
        ["Gleam"] = ["Wisp", "CLI Tool", "Custom..."],
    };

    public int Execute(string[] args)
    {
        // 处理 --reset 参数
        if (args.Contains("--reset"))
        {
            var newConfig = new SlackConfig();
            newConfig.Save();
            AnsiConsole.MarkupLine($"[green]{Strings.ConfigResetSuccess}[/]");
            return 0;
        }

        var config = SlackConfig.Load();

        while (true)
        {
            Console.Clear();
            ShowCurrentConfig(config);

            var editOption = Strings.EditConfig;
            var resetOption = Strings.ResetToDefault;
            var backOption = Strings.CancelOption;

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]{Strings.SelectOperation}[/]")
                    .AddChoices([editOption, resetOption, backOption]));

            if (choice == editOption)
            {
                EditConfig(config);
            }
            else if (choice == resetOption)
            {
                config = new SlackConfig();
                config.Save();
                Strings.ResetLocale();
                AnsiConsole.MarkupLine($"[green]{Strings.ConfigResetSuccess}[/]");
                Thread.Sleep(1000);
            }
            else
            {
                return 0;
            }
        }
    }

    private static void ShowCurrentConfig(SlackConfig config)
    {
        AnsiConsole.Write(new FigletText("slack config").Color(Color.Blue));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn($"[bold]{Strings.ConfigItem}[/]").Centered())
            .AddColumn(new TableColumn($"[bold]{Strings.CurrentValue}[/]").Centered());

        table.AddRow(Strings.ProjectName, $"[cyan]{config.ProjectName}[/]");
        table.AddRow(Strings.Language, $"[green]{config.Language}[/]");
        table.AddRow(Strings.TechStack, $"[yellow]{config.TechStack}[/]");
        table.AddRow(Strings.PackageManager, $"[magenta]{config.PackageManager}[/]");
        table.AddRow(Strings.RuntimeVersion, $"[blue]{config.RuntimeVersion}[/]");
        table.AddRow(Strings.Environment, $"[white]{config.EnvName}[/]");
        table.AddRow(Strings.Locale, $"[cyan]{config.Locale ?? Strings.LocaleAuto}[/]");

        AnsiConsole.Write(table);

        // 显示模块预览
        var modules = config.GetModules();
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[dim]{Strings.ModulePreview}[/]").RuleStyle("grey").LeftJustified());
        var moduleList = string.Join("  ", modules.Take(6).Select(m => $"[grey]{m}[/]"));
        AnsiConsole.MarkupLine(moduleList);
        if (modules.Length > 6)
        {
            AnsiConsole.MarkupLine($"[dim]{Strings.Format(Strings.MoreModules, modules.Length - 6)}[/]");
        }
    }

    private static void EditConfig(SlackConfig config)
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]{Strings.ConfigWizard}[/]"));
        AnsiConsole.WriteLine();

        // 第一步：选择语言
        AnsiConsole.MarkupLine($"[bold]{Strings.Step1SelectLanguage}[/]\n");

        var languages = TechStacksByLanguage.Keys.ToList();
        var selectedLanguage = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.SelectLanguagePrompt)
                .PageSize(10)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(languages));

        config.Language = selectedLanguage;

        // 第二步：选择技术栈
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{Strings.Step2SelectTechStack}[/]\n");

        var techStacks = TechStacksByLanguage[selectedLanguage];
        var selectedStack = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Strings.Format(Strings.SelectTechStackPrompt, selectedLanguage))
                .PageSize(10)
                .HighlightStyle(new Style(Color.Yellow))
                .AddChoices(techStacks));

        if (selectedStack == "Custom...")
        {
            selectedStack = AnsiConsole.Prompt(
                new TextPrompt<string>(Strings.EnterCustomTechStack)
                    .PromptStyle("green"));
        }

        config.TechStack = selectedStack;

        // 自动设置包管理器和运行时版本
        var (defaultManager, defaultVersion) = GetDefaults(selectedLanguage, selectedStack);
        config.PackageManager = defaultManager;
        config.RuntimeVersion = defaultVersion;

        // 第三步：项目信息
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{Strings.Step3ProjectInfo}[/]\n");

        config.ProjectName = AnsiConsole.Prompt(
            new TextPrompt<string>(Strings.ProjectNamePrompt)
                .DefaultValue(config.ProjectName)
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

        // 第四步：选择界面语言
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{Strings.Step4SelectLocale}[/]\n");

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

        if (selectedLocale == Strings.LocaleFollowSystem)
        {
            config.Locale = null;
        }
        else if (selectedLocale.Contains("zh-CN"))
        {
            config.Locale = "zh-CN";
        }
        else
        {
            config.Locale = "en-US";
        }

        // 保存配置
        config.Save();
        Strings.ResetLocale(); // 重置语言缓存以应用新设置

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[green]{Strings.ConfigSaved}[/]").RuleStyle("green"));
        Thread.Sleep(1500);
    }

    private static (string manager, string version) GetDefaults(string language, string stack)
    {
        return language switch
        {
            "C#" or "F#" => ("NuGet", ".NET 10.0"),
            "TypeScript" => stack.Contains("Deno") ? ("deno", "Deno 2.0") : ("pnpm", "Node v22.1.0"),
            "JavaScript" => ("npm", "Node v22.1.0"),
            "Python" => stack.Contains("Django") || stack.Contains("FastAPI") 
                ? ("pip", "Python 3.12") 
                : ("poetry", "Python 3.12"),
            "Go" => ("go mod", "Go 1.23"),
            "Rust" => ("Cargo", "Rust 1.82"),
            "Java" => stack.Contains("Spring") ? ("Maven", "Java 21") : ("Gradle", "Java 21"),
            "Kotlin" => ("Gradle", "Kotlin 2.0"),
            "Swift" => ("SPM", "Swift 5.10"),
            "PHP" => ("Composer", "PHP 8.3"),
            "Ruby" => ("Bundler", "Ruby 3.3"),
            "C++" => ("CMake", "C++23"),
            "C" => ("Make", "C17"),
            "Scala" => ("sbt", "Scala 3.4"),
            "Elixir" => ("Mix", "Elixir 1.16"),
            "Dart" => ("pub", "Dart 3.3"),
            "Lua" => ("LuaRocks", "Lua 5.4"),
            "Zig" => ("zig build", "Zig 0.13"),
            "Haskell" => ("Cabal", "GHC 9.8"),
            "Clojure" => ("Leiningen", "Clojure 1.11"),
            "OCaml" => ("opam", "OCaml 5.1"),
            "Nim" => ("Nimble", "Nim 2.0"),
            "V" => ("v", "V 0.4"),
            "Gleam" => ("gleam", "Gleam 1.0"),
            _ => ("unknown", "unknown")
        };
    }
}
