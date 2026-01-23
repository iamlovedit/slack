using Slack.Localization;
using Slack.Models;
using Spectre.Console;

namespace Slack.Commands;

public class UpCommand : ICommand
{
    public string Name => "up";
    public string Description => Strings.UpDescription;
    public string[] Aliases => [];

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

    private static GlobalConfig _globalConfig = null!;
    private static SlackConfig _config = null!;
    private static string[] _modules = null!;
    private static string[] _taskPrefixes = null!;
    private static string[] _actions = null!;
    private static Random _random = null!;
    private static DateTime _startTime;
    private static int _warningCount = 0;

    public int Execute(string[] args)
    {
        _globalConfig = GlobalConfig.Load();
        _config = SlackConfig.LoadLocal();
        _modules = _config.GetModules();
        _taskPrefixes = _config.GetTaskPrefixes();
        _actions = _config.GetActions();
        
        // 合并自定义消息
        if (_config.CustomMessages.Count > 0)
        {
            var mixedActions = _actions.ToList();
            mixedActions.AddRange(_config.CustomMessages);
            _actions = mixedActions.ToArray();
        }

        _random = new Random();
        _startTime = DateTime.Now;

        var (manager, version) = _config.GetPackageInfo();

        AnsiConsole.MarkupLine($"[bold green]{Strings.Format(Strings.Building, _config.ProjectName)}[/]\n");
        Thread.Sleep(GetDelay(300));

        // 显示初始化信息
        AnsiConsole.MarkupLine($"[grey]{Strings.Format(Strings.DetectedEnv, _config.EnvName)}[/]");
        AnsiConsole.MarkupLine($"[grey]{Strings.Format(Strings.Runtime, version)}[/]");
        AnsiConsole.MarkupLine($"[grey][[INFO]] Package manager: {manager}[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            // 检查编译时长
            if (_config.BuildDuration > 0)
            {
                var elapsed = (DateTime.Now - _startTime).TotalSeconds;
                if (elapsed >= _config.BuildDuration)
                {
                    ShowBuildSuccess();
                    break;
                }
            }

            // 随机暂停 (如果启用)
            if (_config.RandomPauses && _random.Next(100) < 5)
            {
                Thread.Sleep(GetDelay(_random.Next(500, 2000)));
            }

            // 每隔一段时间随机触发警告或可恢复错误
            // 警告概率受到已产生警告数量的限制
            bool canWarn = _config.WarningMax > 0 && _warningCount < _config.WarningMax;
            bool mustWarn = _config.WarningMin > 0 && _warningCount < _config.WarningMin;
            
            int warnProb = canWarn ? 15 : 0;
            if (mustWarn) warnProb = 40; // 如果还没达到最小警告数，增加概率

            if (_random.Next(100) < warnProb)
            {
                ShowWarning();
                _warningCount++;
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
                    AnsiConsole.MarkupLine($"\n[bold yellow]{Strings.BuildInterrupted}[/]");
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

    private static int GetDelay(int baseDelay)
    {
        return _globalConfig.ScrollSpeed switch
        {
            "Slow" => baseDelay * 2,
            "Fast" => (int)(baseDelay * 0.3),
            _ => baseDelay
        };
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
        Thread.Sleep(GetDelay(_random.Next(100, 300)));
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
            .Start($"[yellow]{Strings.Recovering}[/]", ctx =>
            {
                Thread.Sleep(GetDelay(_random.Next(1500, 3500)));
            });

        AnsiConsole.MarkupLine($"[green]✓[/] [dim]{Strings.RecoveredSuccess}[/]");
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
                var spinner = GetSpinnerByStyle();
                
                while (!task.IsFinished)
                {
                    double increment;
                    int delay;

                    var situation = _random.Next(100);

                    if (situation < 10) // 10% 卡住一会儿
                    {
                        increment = _random.NextDouble() * 0.5;
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

                    if (task.Percentage > 85 && task.Percentage < 99)
                    {
                        increment = Math.Min(increment, _random.Next(1, 5));
                        delay = _random.Next(200, 500);
                    }

                    task.Increment(increment);
                    Thread.Sleep(GetDelay(delay));
                }
            });

        // 偶尔显示警告
        if (_random.Next(100) < 20)
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {taskName} [grey]{Strings.Done.ToLower()}[/] [yellow]{Strings.Format(Strings.WithWarnings, _random.Next(1, 5))}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {taskName} [grey]{Strings.Done.ToLower()}[/]");
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

                        if (task.Percentage > 85)
                            increment = Math.Min(increment, _random.Next(1, 6));

                        task.Increment(increment);
                    }
                    Thread.Sleep(GetDelay(_random.Next(50, 200)));
                }
            });

        foreach (var t in tasks)
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {t} [grey]{Strings.Done.ToLower()}[/]");
        }
    }

    private static void ShowStatusSpinner()
    {
        var action = _actions[_random.Next(_actions.Length)];
        var spinner = GetSpinnerByStyle();

        AnsiConsole.Status()
            .Spinner(spinner)
            .SpinnerStyle(Style.Parse("green"))
            .Start(action, ctx =>
            {
                Thread.Sleep(GetDelay(_random.Next(1000, 3000)));

                var changes = _random.Next(1, 4);
                for (int i = 0; i < changes; i++)
                {
                    ctx.Status(_actions[_random.Next(_actions.Length)]);
                    Thread.Sleep(GetDelay(_random.Next(500, 1500)));
                }
            });

        AnsiConsole.MarkupLine($"[green]✓[/] {action.Replace("=>", "").Trim()} [grey]{Strings.Completed}[/]");
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

            var outputType = _random.Next(100);

            if (outputType < 8)
            {
                hasWarning = true;
                AnsiConsole.MarkupLine($"[yellow]  ⚠ {module}:{lineNum} - Deprecated method call[/]");
                Thread.Sleep(GetDelay(_random.Next(80, 200)));
            }
            else if (outputType < 15)
            {
                var slowTime = _random.Next(800, 2500);
                AnsiConsole.MarkupLine($"[dim]  {module}:{lineNum} [/][red dim]({slowTime}ms)[/]");
                Thread.Sleep(GetDelay(_random.Next(100, 300)));
            }
            else
            {
                var color = time > 150 ? "yellow" : "grey";
                AnsiConsole.MarkupLine($"[{color}]  {module}:{lineNum} [dim]({time}ms)[/][/]");
                Thread.Sleep(GetDelay(_random.Next(30, 150)));
            }
        }

        if (hasWarning)
        {
            AnsiConsole.MarkupLine($"[yellow]  {Strings.CompletedWithWarnings}[/]");
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
        Thread.Sleep(GetDelay(_random.Next(500, 1000)));
    }

    private static void ShowBuildSuccess()
    {
        AnsiConsole.WriteLine();
        var elapsed = (DateTime.Now - _startTime).TotalSeconds;

        AnsiConsole.Write(new Rule($"[green]{Strings.BuildSummary}[/]").RuleStyle("green dim"));

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("")
            .AddColumn("");

        table.AddRow($"[green]{Strings.Status}[/]", $"[bold green]{Strings.Success}[/]");
        table.AddRow($"[grey]{Strings.Project}[/]", $"[cyan]{_config.ProjectName}[/]");
        table.AddRow($"[grey]{Strings.Duration}[/]", $"[white]{elapsed:F1}s[/]");

        if (_warningCount > 0)
        {
            table.AddRow($"[grey]{Strings.Warnings}[/]", $"[yellow]{_warningCount}[/]");
        }
        table.AddRow($"[grey]{Strings.Errors}[/]", "[green]0[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]{Strings.Format(Strings.BuildCompleted, _config.ProjectName)}[/]");
    }

    private static Spinner GetSpinnerByStyle()
    {
        var style = _globalConfig.AnimationStyle ?? "Modern";
        var spinners = style switch
        {
            "Classic" => new[] { Spinner.Known.Line, Spinner.Known.Dots, Spinner.Known.SimpleDots, Spinner.Known.SimpleDotsScrolling },
            "Modern" => new[] { Spinner.Known.Dots2, Spinner.Known.Dots3, Spinner.Known.Bounce, Spinner.Known.CircleHalves },
            "Minimal" => new[] { Spinner.Known.Point, Spinner.Known.SquareCorners },
            "Fancy" => new[] { Spinner.Known.Star, Spinner.Known.Flip, Spinner.Known.Hamburger, Spinner.Known.GrowVertical, Spinner.Known.Clock },
            _ => new[] { Spinner.Known.Dots }
        };
        
        return spinners[_random.Next(spinners.Length)];
    }
}
