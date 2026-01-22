using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public class ConfigCommand : ICommand
{
    public string Name => "config";
    public string Description => "配置技术栈、项目名称等参数";
    public string[] Aliases => [];

    public static string[] GetLanguages() => TechStacksByLanguage.Keys.ToArray();
    public static string[] GetTechStacks(string language) => TechStacksByLanguage.GetValueOrDefault(language, []);
    public static (string manager, string version) GetDefaultsForLanguage(string language, string stack) => GetDefaults(language, stack);

    // 语言 -> 技术栈映射
    private static readonly Dictionary<string, string[]> TechStacksByLanguage = new()
    {
        ["C#"] = ["ASP.NET Core Web API", "Blazor", "MAUI", "Console App", "WPF", "gRPC Service", "Unity Game", "自定义..."],
        ["TypeScript"] = ["Next.js", "React + Vite", "Vue 3", "NestJS", "Express", "Nuxt", "Angular", "Svelte", "Deno", "自定义..."],
        ["JavaScript"] = ["React", "Vue", "Express", "Fastify", "Electron", "Node.js CLI", "自定义..."],
        ["Python"] = ["FastAPI", "Django", "Flask", "Celery Worker", "CLI Tool", "PyTorch ML", "Scrapy", "自定义..."],
        ["Go"] = ["Gin", "Echo", "Fiber", "gRPC", "CLI Tool", "Kubernetes Operator", "自定义..."],
        ["Rust"] = ["Actix-web", "Axum", "Rocket", "Tokio", "CLI Tool", "Tauri", "WASM", "自定义..."],
        ["Java"] = ["Spring Boot", "Quarkus", "Micronaut", "Gradle Plugin", "Android", "自定义..."],
        ["Kotlin"] = ["Ktor", "Spring Boot", "Android", "Compose Desktop", "Compose Multiplatform", "自定义..."],
        ["Swift"] = ["iOS App", "macOS App", "SwiftUI", "Vapor", "CLI Tool", "自定义..."],
        ["PHP"] = ["Laravel", "Symfony", "WordPress Plugin", "Slim", "自定义..."],
        ["Ruby"] = ["Ruby on Rails", "Sinatra", "Jekyll", "CLI Tool", "自定义..."],
        ["C++"] = ["Qt", "Unreal Engine", "CLI Tool", "Embedded", "CMake Project", "自定义..."],
        ["C"] = ["Embedded", "Linux Kernel", "CLI Tool", "自定义..."],
        ["Scala"] = ["Play Framework", "Akka", "Spark Job", "ZIO", "自定义..."],
        ["Elixir"] = ["Phoenix", "LiveView", "Nerves IoT", "CLI Tool", "自定义..."],
        ["Dart"] = ["Flutter", "Flutter Web", "CLI Tool", "自定义..."],
        ["Lua"] = ["Love2D Game", "Neovim Plugin", "OpenResty", "自定义..."],
        ["Zig"] = ["CLI Tool", "Embedded", "WASM", "自定义..."],
        ["Haskell"] = ["Yesod", "Servant", "CLI Tool", "自定义..."],
        ["Clojure"] = ["Luminus", "Ring", "ClojureScript", "自定义..."],
        ["F#"] = ["Giraffe", "Saturn", "SAFE Stack", "CLI Tool", "自定义..."],
        ["OCaml"] = ["Dream", "CLI Tool", "自定义..."],
        ["Nim"] = ["Jester", "CLI Tool", "自定义..."],
        ["V"] = ["Vweb", "CLI Tool", "自定义..."],
        ["Gleam"] = ["Wisp", "CLI Tool", "自定义..."],
    };

    public int Execute(string[] args)
    {
        // 处理 --reset 参数
        if (args.Contains("--reset"))
        {
            var newConfig = new SlackConfig();
            newConfig.Save();
            AnsiConsole.MarkupLine("[green]✓ 配置已重置为默认值[/]");
            return 0;
        }

        var config = SlackConfig.Load();

        while (true)
        {
            Console.Clear();
            ShowCurrentConfig(config);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]请选择操作:[/]")
                    .AddChoices(["✏️  修改配置", "🔄 重置为默认", "🚪 返回"]));

            switch (choice)
            {
                case "✏️  修改配置":
                    EditConfig(config);
                    break;
                case "🔄 重置为默认":
                    config = new SlackConfig();
                    config.Save();
                    AnsiConsole.MarkupLine("[green]✓ 已重置为默认配置[/]");
                    Thread.Sleep(1000);
                    break;
                case "🚪 返回":
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
            .AddColumn(new TableColumn("[bold]配置项[/]").Centered())
            .AddColumn(new TableColumn("[bold]当前值[/]").Centered());

        table.AddRow("🏷️  项目名称", $"[cyan]{config.ProjectName}[/]");
        table.AddRow("💻 语言", $"[green]{config.Language}[/]");
        table.AddRow("🛠️  技术栈", $"[yellow]{config.TechStack}[/]");
        table.AddRow("📦 包管理器", $"[magenta]{config.PackageManager}[/]");
        table.AddRow("🔢 运行时版本", $"[blue]{config.RuntimeVersion}[/]");
        table.AddRow("🌍 环境", $"[white]{config.EnvName}[/]");

        AnsiConsole.Write(table);

        // 显示模块预览
        var modules = config.GetModules();
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[dim]模块预览[/]").RuleStyle("grey").LeftJustified());
        var moduleList = string.Join("  ", modules.Take(6).Select(m => $"[grey]{m}[/]"));
        AnsiConsole.MarkupLine(moduleList);
        if (modules.Length > 6)
        {
            AnsiConsole.MarkupLine($"[dim]... 还有 {modules.Length - 6} 个模块[/]");
        }
    }

    private static void EditConfig(SlackConfig config)
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[bold blue]🔧 配置向导[/]"));
        AnsiConsole.WriteLine();

        // 第一步：选择语言
        AnsiConsole.MarkupLine("[bold]第 1 步：选择编程语言[/]\n");

        var languages = TechStacksByLanguage.Keys.ToList();
        var selectedLanguage = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("选择你正在使用的语言:")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(languages));

        config.Language = selectedLanguage;

        // 第二步：选择技术栈
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]第 2 步：选择技术栈[/]\n");

        var techStacks = TechStacksByLanguage[selectedLanguage];
        var selectedStack = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"选择 [cyan]{selectedLanguage}[/] 技术栈:")
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
        var (defaultManager, defaultVersion) = GetDefaults(selectedLanguage, selectedStack);
        config.PackageManager = defaultManager;
        config.RuntimeVersion = defaultVersion;

        // 第三步：项目信息
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]第 3 步：项目信息[/]\n");

        config.ProjectName = AnsiConsole.Prompt(
            new TextPrompt<string>("项目名称:")
                .DefaultValue(config.ProjectName)
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

        // 保存配置
        config.Save();

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[green]✓ 配置已保存[/]").RuleStyle("green"));
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
