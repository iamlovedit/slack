using Slack.Localization;
using Spectre.Console;
using Spectre.Console.Rendering;
using TextCopy;

using Slack.Infrastructure;

namespace Slack.Commands;

[AutoRegisterCommand]
public class FigletCommand : ICommand
{
    public string Name => "figlet";
    public string Description => Strings.FigletDescription;
    public string[] Aliases => [];

    public int Execute(string[] args)
    {
        // 如果没有提供文本，进入交互模式
        if (args.Length == 0)
        {
            return InteractiveMode();
        }

        // 直接输出 FigletText
        var text = string.Join(" ", args);
        AnsiConsole.Write(new FigletText(text).Color(Color.Cyan1));
        return 0;
    }

    private int InteractiveMode()
    {
        AnsiConsole.Write(new FigletText("figlet").Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        // 输入文本
        var text = AnsiConsole.Prompt(
            new TextPrompt<string>($"[bold]{Strings.FigletTextPrompt}[/]")
                .PromptStyle("cyan")
                .Validate(input => 
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error($"[red]{Strings.FigletEmptyText}[/]");
                    
                    if (input.Any(c => c > 127))
                        return ValidationResult.Error($"[red]{Strings.FigletAsciiOnly}[/]");
                        
                    return ValidationResult.Success();
                }));

        // 渲染并输出
        Console.Clear();
        var figlet = new FigletText(text).Color(Color.Cyan1);
        AnsiConsole.Write(figlet);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("grey"));

        // 询问是否复制
        if (AnsiConsole.Confirm(Strings.FigletCopyConfirm, true))
        {
            var content = RenderToString(new FigletText(text));
            CopyToClipboard(content);
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]{Strings.FigletCopyHint}[/]");
        }

        return 0;
    }

    private static string RenderToString(IRenderable renderable)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(writer),
            Interactive = InteractionSupport.No
        });
        
        console.Profile.Width = int.MaxValue;
        console.Write(renderable);
        
        // 去掉每行末尾的多余空格
        var lines = writer.ToString().Split('\n');
        var trimmedLines = lines.Select(line => line.TrimEnd());
        return string.Join(Environment.NewLine, trimmedLines);
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            ClipboardService.SetText(text);
            AnsiConsole.MarkupLine($"[green]{Strings.FigletCopied}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Strings.Format(Strings.FigletCopyFailed, ex.Message)}[/]");
        }
    }
}
