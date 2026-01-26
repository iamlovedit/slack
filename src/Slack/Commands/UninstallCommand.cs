using System.Diagnostics;
using System.Runtime.InteropServices;
using Slack.Localization;
using Slack.Models;
using Spectre.Console;

using Slack.Infrastructure;

namespace Slack.Commands;

[AutoRegisterCommand]
public class UninstallCommand : ICommand
{
    public string Name => "uninstall";
    public string Description => Strings.UninstallDescription;
    public string[] Aliases => [];

    public int Execute(string[] args)
    {
        var skipConfirm = args.Contains("--yes") || args.Contains("-y");

        Console.Clear();
        AnsiConsole.Write(new FigletText(Strings.UninstallTitle).Color(Color.Red));
        AnsiConsole.WriteLine();

        // Show registered projects
        var projects = ProjectRegistry.GetAllProjects();
        if (projects.Count > 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{Strings.Format(Strings.UninstallRegisteredProjects, projects.Count)}[/]");
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn(Strings.ProjectName).Centered())
                .AddColumn(new TableColumn("Path").Centered());

            foreach (var project in projects)
            {
                table.AddRow(
                    $"[cyan]{project.Name}[/]",
                    $"[dim]{project.Path}[/]");
            }
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]{Strings.UninstallNoProjects}[/]\n");
        }

        // Confirmation
        if (!skipConfirm)
        {
            if (!AnsiConsole.Confirm(Strings.UninstallConfirm, false))
            {
                AnsiConsole.MarkupLine($"[yellow]{Strings.UninstallCancelled}[/]");
                return 0;
            }
        }

        // Select cleanup level
        var cleanupLevel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"\n[bold]{Strings.UninstallSelectLevel}[/]")
                .AddChoices([
                    Strings.UninstallLevelBinary,
                    Strings.UninstallLevelGlobal,
                    Strings.UninstallLevelFull
                ]));

        var cleanGlobal = cleanupLevel != Strings.UninstallLevelBinary;
        var cleanProjects = cleanupLevel == Strings.UninstallLevelFull;

        AnsiConsole.WriteLine();

        // Clean project configs
        if (cleanProjects && projects.Count > 0)
        {
            foreach (var project in projects)
            {
                var slackDir = Path.Combine(project.Path, ".slack");
                if (Directory.Exists(slackDir))
                {
                    AnsiConsole.MarkupLine($"[dim]{Strings.Format(Strings.UninstallCleaningProject, project.Path)}[/]");
                    try
                    {
                        Directory.Delete(slackDir, true);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]  Failed: {ex.Message}[/]");
                    }
                }
            }
        }

        // Clean global config
        if (cleanGlobal)
        {
            var globalConfigDir = ProjectRegistry.GetGlobalConfigDir();
            if (Directory.Exists(globalConfigDir))
            {
                AnsiConsole.MarkupLine($"[dim]{Strings.UninstallCleaningGlobal}[/]");
                try
                {
                    Directory.Delete(globalConfigDir, true);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]  Failed: {ex.Message}[/]");
                }
            }
        }

        // Remove binary (self-delete)
        AnsiConsole.MarkupLine($"[dim]{Strings.UninstallRemovingBinary}[/]");
        var exePath = Environment.ProcessPath;

        if (string.IsNullOrEmpty(exePath))
        {
            AnsiConsole.MarkupLine("[red]  Could not determine executable path[/]");
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold green]{Strings.UninstallSuccess}[/]").RuleStyle("green"));

            // Platform-specific self-deletion
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: use cmd to delete after a short delay
                var deleteScript = $"/c timeout /t 2 /nobreak > nul && del /f /q \"{exePath}\"";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = deleteScript,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else
            {
                // Unix: can delete running binary directly
                try
                {
                    File.Delete(exePath);
                }
                catch
                {
                    // If direct deletion fails, try rm command
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "rm",
                        Arguments = $"-f \"{exePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
        }

        return 0;
    }
}
