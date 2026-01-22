using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public static class UpCommand
{
    private static readonly string[] SpinnerTypes =
    [
        "Dots", "Dots2", "Dots3", "Line", "Star", "Flip",
        "Hamburger", "GrowVertical", "GrowHorizontal", "Balloon",
        "Noise", "Bounce", "BoxBounce", "Triangle", "Arc",
        "Circle", "SquareCorners", "CircleQuarters", "CircleHalves"
    ];

    // 警告消息模板
    private static readonly string[] WarningTemplates =
    [
        "Deprecated API usage in {0}",
        "Unhandled nullable reference in {0}",
        "Performance: Consider async pattern in {0}",
        "Large file detected: {0} ({1} KB)",
        "Circular dependency detected: {0} -> {1}",
        "Missing XML documentation for public API in {0}",
        "Potential memory leak in {0}",
        "Unused variable 'result' in {0}:{1}",
        "Consider extracting method in {0} (complexity: {1})",
        "Outdated package reference: {0}",
        "Security: Input not sanitized in {0}",
        "TODO found: 'Fix this later' in {0}:{1}",
        "Implicit conversion may cause data loss in {0}",
        "Exception swallowed silently in {0}:{1}",
        "Hardcoded credential detected in {0}"
    ];

    // 错误消息（但会恢复）
    private static readonly string[] RetryableErrors =
    [
        "Connection timeout, retrying...",
        "Rate limited by registry, waiting {0}s...",
        "Checksum mismatch, re-downloading...",
        "Lock file conflict, resolving...",
        "Temporary network error, retrying ({0}/3)...",
        "Cache miss, rebuilding index...",
        "Stale lock detected, cleaning up..."
    ];

    private static SlackConfig _config = null!;
    private static string[] _modules = null!;
    private static string[] _taskPrefixes = null!;
    private static string[] _actions = null!;
    private static Random _random = null!;

    public static int Execute()
    {
        _config = SlackConfig.Load();
        _modules = _config.GetModules();
        _taskPrefixes = _config.GetTaskPrefixes();
        _actions = _config.GetActions();
        _random = new Random();

        var (manager, version) = _config.GetPackageInfo();

        AnsiConsole.MarkupLine($"[bold green]🚀 Building {_config.ProjectName}...[/]\n");
        Thread.Sleep(300);

        // 显示初始化信息
        AnsiConsole.MarkupLine($"[grey][[INFO]] Detected environment: {_config.EnvName}[/]");
        AnsiConsole.MarkupLine($"[grey][[INFO]] Runtime: {version}[/]");
        AnsiConsole.MarkupLine($"[grey][[INFO]] Package manager: {manager}[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            // 每隔一段时间随机触发警告或可恢复错误
            if (_random.Next(100) < 15) // 15% 概率
            {
                ShowWarning();
            }
            else if (_random.Next(100) < 5) // 5% 概率
            {
                ShowRetryableError();
            }

            // 随机选择一种展示方式
            var displayType = _random.Next(5);

            switch (displayType)
            {
                case 0:
                    ShowRealisticProgressBar();
                    break;
                case 1:
                    ShowStatusSpinner();
                    break;
                case 2:
                    ShowFakeCompileOutput();
                    break;
                case 3:
                    ShowTreeOutput();
                    break;
                case 4:
                    ShowMultiProgressBars();
                    break;
            }

            // 检查是否按下了退出键
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    AnsiConsole.MarkupLine("\n[bold yellow]⚠ Build interrupted by user.[/]");
                    break;
                }
                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    ShowBuildSuccess();
                    break;
                }
            }
        }

        return 0;
    }

    private static void ShowWarning()
    {
        var template = WarningTemplates[_random.Next(WarningTemplates.Length)];
        var module = _modules[_random.Next(_modules.Length)];
        var lineNum = _random.Next(50, 500);
        var size = _random.Next(100, 800);
        var complexity = _random.Next(15, 45);

        string warning = template;
        try
        {
            if (template.Contains("{1}") && template.Contains("{0}"))
            {
                if (template.Contains("KB"))
                    warning = string.Format(template, module, size);
                else if (template.Contains("complexity"))
                    warning = string.Format(template, module, complexity);
                else if (template.Contains("->"))
                    warning = string.Format(template, module, _modules[_random.Next(_modules.Length)]);
                else
                    warning = string.Format(template, module, lineNum);
            }
            else
            {
                warning = string.Format(template, module);
            }
        }
        catch
        {
            warning = string.Format(WarningTemplates[0], module);
        }

        AnsiConsole.MarkupLine($"[yellow]⚠ WARN:[/] [dim]{warning}[/]");
        Thread.Sleep(_random.Next(100, 300));
    }

    private static void ShowRetryableError()
    {
        var template = RetryableErrors[_random.Next(RetryableErrors.Length)];
        var waitTime = _random.Next(2, 8);
        var retryNum = _random.Next(1, 3);

        string error;
        try
        {
            if (template.Contains("{0}"))
            {
                if (template.Contains("/3"))
                    error = string.Format(template, retryNum);
                else
                    error = string.Format(template, waitTime);
            }
            else
            {
                error = template;
            }
        }
        catch
        {
            error = template;
        }

        AnsiConsole.MarkupLine($"[red]✗ ERROR:[/] [dim]{error}[/]");

        // 显示重试动画
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("red"))
            .Start("[yellow]Recovering...[/]", ctx =>
            {
                Thread.Sleep(_random.Next(1500, 3500));
            });

        AnsiConsole.MarkupLine($"[green]✓[/] [dim]Recovered successfully[/]");
        Thread.Sleep(200);
    }

    private static void ShowRealisticProgressBar()
    {
        var taskName = $"{_taskPrefixes[_random.Next(_taskPrefixes.Length)]} {_modules[_random.Next(_modules.Length)]}";

        AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[cyan]{taskName}[/]");

                while (!task.IsFinished)
                {
                    // 模拟不均匀的进度：有时快，有时慢，偶尔卡住
                    double increment;
                    int delay;

                    var situation = _random.Next(100);

                    if (situation < 10) // 10% 卡住一会儿
                    {
                        increment = _random.NextDouble() * 0.5; // 几乎不动
                        delay = _random.Next(300, 800);
                    }
                    else if (situation < 30) // 20% 缓慢进展
                    {
                        increment = _random.Next(1, 4);
                        delay = _random.Next(150, 350);
                    }
                    else if (situation < 70) // 40% 正常速度
                    {
                        increment = _random.Next(3, 10);
                        delay = _random.Next(50, 150);
                    }
                    else if (situation < 90) // 20% 快速
                    {
                        increment = _random.Next(8, 18);
                        delay = _random.Next(30, 80);
                    }
                    else // 10% 突然飙升
                    {
                        increment = _random.Next(15, 30);
                        delay = _random.Next(20, 50);
                    }

                    // 在90%附近经常减速（模拟最后阶段总是最慢）
                    if (task.Percentage > 85 && task.Percentage < 99)
                    {
                        increment = Math.Min(increment, _random.Next(1, 5));
                        delay = _random.Next(200, 500);
                    }

                    task.Increment(increment);
                    Thread.Sleep(delay);
                }
            });

        // 偶尔显示警告
        if (_random.Next(100) < 20)
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {taskName} [grey]done[/] [yellow](with {_random.Next(1, 5)} warnings)[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {taskName} [grey]done[/]");
        }
    }

    private static void ShowMultiProgressBars()
    {
        var taskCount = _random.Next(2, 5);
        var tasks = new List<string>();
        for (int i = 0; i < taskCount; i++)
        {
            tasks.Add($"{_taskPrefixes[_random.Next(_taskPrefixes.Length)]} {_modules[_random.Next(_modules.Length)]}");
        }

        AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var progressTasks = tasks.Select(t => ctx.AddTask($"[cyan]{t}[/]")).ToList();

                while (!ctx.IsFinished)
                {
                    foreach (var task in progressTasks.Where(t => !t.IsFinished))
                    {
                        var situation = _random.Next(100);
                        double increment;

                        if (situation < 15)
                            increment = _random.NextDouble() * 2;
                        else if (situation < 50)
                            increment = _random.Next(2, 8);
                        else if (situation < 85)
                            increment = _random.Next(5, 15);
                        else
                            increment = _random.Next(10, 25);

                        // 90%以后减速
                        if (task.Percentage > 85)
                            increment = Math.Min(increment, _random.Next(1, 6));

                        task.Increment(increment);
                    }
                    Thread.Sleep(_random.Next(50, 200));
                }
            });

        foreach (var t in tasks)
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {t} [grey]done[/]");
        }
    }

    private static void ShowStatusSpinner()
    {
        var action = _actions[_random.Next(_actions.Length)];
        var spinnerName = SpinnerTypes[_random.Next(SpinnerTypes.Length)];
        var spinner = GetSpinnerByName(spinnerName);

        AnsiConsole.Status()
            .Spinner(spinner)
            .SpinnerStyle(Style.Parse("green"))
            .Start(action, ctx =>
            {
                Thread.Sleep(_random.Next(1000, 3000));

                // 随机更换几次状态
                var changes = _random.Next(1, 4);
                for (int i = 0; i < changes; i++)
                {
                    ctx.Status(_actions[_random.Next(_actions.Length)]);
                    Thread.Sleep(_random.Next(500, 1500));
                }
            });

        AnsiConsole.MarkupLine($"[green]✓[/] {action.Replace("=>", "").Trim()} [grey]completed[/]");
    }

    private static void ShowFakeCompileOutput()
    {
        var fileCount = _random.Next(5, 15);
        bool hasWarning = false;

        for (int i = 0; i < fileCount; i++)
        {
            var module = _modules[_random.Next(_modules.Length)];
            var lineNum = _random.Next(50, 500);
            var time = _random.Next(10, 200);

            // 随机决定是普通输出、警告还是慢速编译
            var outputType = _random.Next(100);

            if (outputType < 8) // 8% 显示警告
            {
                hasWarning = true;
                AnsiConsole.MarkupLine($"[yellow]  ⚠ {module}:{lineNum} - Deprecated method call[/]");
                Thread.Sleep(_random.Next(80, 200));
            }
            else if (outputType < 15) // 7% 显示慢速
            {
                var slowTime = _random.Next(800, 2500);
                AnsiConsole.MarkupLine($"[dim]  {module}:{lineNum} [/][red dim]({slowTime}ms)[/]");
                Thread.Sleep(_random.Next(100, 300));
            }
            else
            {
                var color = time > 150 ? "yellow" : "grey";
                AnsiConsole.MarkupLine($"[{color}]  {module}:{lineNum} [dim]({time}ms)[/][/]");
                Thread.Sleep(_random.Next(30, 150));
            }
        }

        if (hasWarning)
        {
            AnsiConsole.MarkupLine($"[yellow]  └─ Completed with warnings[/]");
        }
        AnsiConsole.WriteLine();
    }

    private static void ShowTreeOutput()
    {
        var root = new Tree($"[yellow]📦 {_modules[_random.Next(_modules.Length)]}[/]");

        var childCount = _random.Next(2, 5);
        for (int i = 0; i < childCount; i++)
        {
            var child = root.AddNode($"[blue]{_modules[_random.Next(_modules.Length)]}[/]");
            var subChildCount = _random.Next(1, 4);
            for (int j = 0; j < subChildCount; j++)
            {
                // 偶尔标记为有问题的依赖
                if (_random.Next(100) < 10)
                {
                    child.AddNode($"[red]{_modules[_random.Next(_modules.Length)]} ⚠[/]");
                }
                else
                {
                    child.AddNode($"[grey]{_modules[_random.Next(_modules.Length)]}[/]");
                }
            }
        }

        AnsiConsole.Write(root);
        AnsiConsole.WriteLine();
        Thread.Sleep(_random.Next(500, 1000));
    }

    private static void ShowBuildSuccess()
    {
        AnsiConsole.WriteLine();
        var warningCount = _random.Next(0, 12);
        var time = _random.NextDouble() * 30 + 10; // 10-40秒

        AnsiConsole.Write(new Rule("[green]Build Summary[/]").RuleStyle("green dim"));

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("")
            .AddColumn("");

        table.AddRow("[green]Status[/]", "[bold green]SUCCESS[/]");
        table.AddRow("[grey]Project[/]", $"[cyan]{_config.ProjectName}[/]");
        table.AddRow("[grey]Duration[/]", $"[white]{time:F1}s[/]");

        if (warningCount > 0)
        {
            table.AddRow("[grey]Warnings[/]", $"[yellow]{warningCount}[/]");
        }
        table.AddRow("[grey]Errors[/]", "[green]0[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ {_config.ProjectName} build completed successfully![/]");
    }

    private static Spinner GetSpinnerByName(string name) => name switch
    {
        "Dots" => Spinner.Known.Dots,
        "Dots2" => Spinner.Known.Dots2,
        "Dots3" => Spinner.Known.Dots3,
        "Line" => Spinner.Known.Line,
        "Star" => Spinner.Known.Star,
        "Flip" => Spinner.Known.Flip,
        "Hamburger" => Spinner.Known.Hamburger,
        "GrowVertical" => Spinner.Known.GrowVertical,
        "GrowHorizontal" => Spinner.Known.GrowHorizontal,
        "Balloon" => Spinner.Known.Balloon,
        "Noise" => Spinner.Known.Noise,
        "Bounce" => Spinner.Known.Bounce,
        "BoxBounce" => Spinner.Known.BoxBounce,
        "Triangle" => Spinner.Known.Triangle,
        "Arc" => Spinner.Known.Arc,
        "Circle" => Spinner.Known.Circle,
        "SquareCorners" => Spinner.Known.SquareCorners,
        "CircleQuarters" => Spinner.Known.CircleQuarters,
        "CircleHalves" => Spinner.Known.CircleHalves,
        _ => Spinner.Known.Default
    };
}
