using Slack.Localization;
using Spectre.Console;
using TextCopy;

using Slack.Infrastructure;

namespace Slack.Commands;

[AutoRegisterCommand]
public class GuidCommand : ICommand
{
    public string Name => "guid";
    public string Description => Strings.GuidDescription;
    public string[] Aliases => [];

    // GUID 格式选项
    private static readonly Dictionary<string, (string Name, Func<Guid, string> Formatter)> Formats = new()
    {
        ["D"] = ("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", g => g.ToString("D")),
        ["N"] = ("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", g => g.ToString("N")),
        ["B"] = ("{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}", g => g.ToString("B")),
        ["P"] = ("(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)", g => g.ToString("P")),
        ["X"] = ("{0x...,0x...,0x...,{0x...}}", g => g.ToString("X")),
        ["UPPER"] = ("XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX", g => g.ToString("D").ToUpperInvariant()),
    };

    public int Execute(string[] args)
    {
        int count = 0;
        string? format = null;
        bool autoCopy = false;
        AnsiConsole.Write(new FigletText("guid").Color(Color.Green));
        AnsiConsole.WriteLine();
        // 解析命令行参数
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if ((arg == "-n" || arg == "--count") && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out var n) && n > 0)
                {
                    count = n;
                }
            }
            else if ((arg == "-f" || arg == "--format") && i + 1 < args.Length)
            {
                format = args[++i].ToUpperInvariant();
            }
            else if (arg == "-c" || arg == "--copy")
            {
                autoCopy = true;
            }
            else if (int.TryParse(arg, out var n) && n > 0 && count == 0)
            {
                // 支持直接传递数字作为第一个参数
                count = n;
            }
        }

        // 如果没有参数，进入交互模式
        if (count == 0 && format == null && !autoCopy)
        {
            return InteractiveMode();
        }

        // 使用默认值
        if (count == 0) count = 1;
        if (format == null) format = "D";

        // 验证格式
        if (!Formats.ContainsKey(format))
        {
            AnsiConsole.MarkupLine($"[red]{Strings.Format(Strings.GuidInvalidFormat, format)}[/]");
            ShowValidFormats();
            return 1;
        }

        // 生成并输出 GUID
        var guids = GenerateGuids(count, format);

        // 自动复制
        if (autoCopy)
        {
            CopyToClipboard(guids);
        }

        return 0;
    }

    private int InteractiveMode()
    {
        AnsiConsole.WriteLine();

        // 选择数量
        var count = AnsiConsole.Prompt(
            new TextPrompt<int>($"[bold]{Strings.GuidCountPrompt}[/]")
                .DefaultValue(1)
                .PromptStyle("cyan")
                .ValidationErrorMessage($"[red]{Strings.GuidCountValidation}[/]")
                .Validate(n => n > 0 && n <= 1000 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error($"[red]{Strings.GuidCountValidation}[/]")));

        AnsiConsole.WriteLine();

        // 选择格式
        var formatChoices = Formats.Select(f => $"{f.Key} - {f.Value.Name}").ToList();
        var selectedFormat = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]{Strings.GuidFormatPrompt}[/]")
                .PageSize(8)
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(formatChoices));

        var format = selectedFormat.Split(" - ")[0];

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[green]{Strings.GuidGenerated}[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        // 生成并输出 GUID
        var guids = GenerateGuids(count, format);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("grey"));
        
        // 询问是否复制
        if (AnsiConsole.Confirm(Strings.GuidCopyConfirm, true))
        {
             CopyToClipboard(guids);
        }
        else
        {
            // 提示复制操作
            AnsiConsole.MarkupLine($"[dim]{Strings.GuidCopyHint}[/]");
        }

        return 0;
    }

    private List<string> GenerateGuids(int count, string format)
    {
        var formatter = Formats[format].Formatter;
        var results = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewGuid();
            var formatted = formatter(guid);
            results.Add(formatted);
            
            if (count == 1)
            {
                AnsiConsole.MarkupLine($"[cyan]{formatted}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[grey]{i + 1,4}.[/] [cyan]{formatted}[/]");
            }
        }
        return results;
    }

    private void CopyToClipboard(List<string> guids)
    {
        try
        {
            var text = string.Join(Environment.NewLine, guids);
            ClipboardService.SetText(text);
            AnsiConsole.MarkupLine($"[green]{Strings.GuidCopied}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Strings.Format(Strings.GuidCopyFailed, ex.Message)}[/]");
        }
    }

    private static void ShowValidFormats()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[yellow]{Strings.GuidValidFormats}:[/]");
        foreach (var (key, (name, _)) in Formats)
        {
            AnsiConsole.MarkupLine($"  [cyan]{key}[/] - {name}");
        }
    }
}
