# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Slack is a CLI tool that simulates realistic build/compilation output in the terminal - a humorous "slacking" utility built with .NET 10 and AOT (Ahead-of-Time) compilation. The project produces native executables (~5MB) for 8 platform variants (Windows, Linux, macOS on x64/ARM64).

## Build Commands

### Development Build
```bash
dotnet build src/Slack/Slack.csproj
```

### Release Build (AOT native compilation)
```bash
dotnet publish src/Slack/Slack.csproj -c Release -r <RID> -p:PublishSingleFile=true --self-contained true -o publish/<RID>
```

Runtime Identifiers (RID):
- Windows: `win-x64`, `win-arm64`
- Linux: `linux-x64`, `linux-arm64`, `linux-musl-x64`, `linux-musl-arm64`
- macOS: `osx-x64`, `osx-arm64`

### Run Locally
```bash
dotnet run --project src/Slack/Slack.csproj -- <command> [args]
```

Example:
```bash
dotnet run --project src/Slack/Slack.csproj -- up
dotnet run --project src/Slack/Slack.csproj -- help
```

## Architecture

### Command Pattern System

Commands implement `ICommand` interface:
```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string[] Aliases { get; }
    int Execute(string[] args);
}
```

### Automatic Command Registration

Commands are automatically registered via **source code generation** at compile-time:

1. Mark command class with `[AutoRegisterCommand]` attribute
2. The `CommandRegistrationGenerator` (Roslyn analyzer) scans for these attributes during compilation
3. Generates `RegisterAllCommands()` extension method in `CommandRegistry`
4. `Program.cs:10` calls this generated method to populate the registry

**Order property**: Use `Order` in the attribute to control initialization sequence. Commands requiring `CommandRegistry` injection (like `HelpCommand`) should use high values (e.g., `int.MaxValue`).

Example:
```csharp
[AutoRegisterCommand]
public class MyCommand : ICommand { ... }

[AutoRegisterCommand(Order = int.MaxValue)]
public class HelpCommand : ICommand
{
    private readonly CommandRegistry _registry;
    public HelpCommand(CommandRegistry registry) => _registry = registry;
}
```

### Configuration System

**Two-tier hierarchy:**

1. **Global Config** (`~/.config/slack/global.json`)
   - Model: `GlobalConfig.cs` (src/Slack/Models/)
   - Settings: Locale, ScrollSpeed, ColorTheme, AnimationStyle, AutoUpdate

2. **Local Config** (`./.slack/config.json`)
   - Model: `SlackConfig.cs` (src/Slack/Models/)
   - Settings: ProjectName, Language, TechStack, PackageManager, RuntimeVersion, Environment, CustomModules, CustomTasks, BuildDurationRange, WarningCountRange

Local config overrides global config where applicable.

### Localization (i18n)

**Multi-language support** via source generation:

- Locale files: `src/Slack/Localization/locales/` (en.json, zh-CN.json)
- Generated code: `Strings.cs` (partial class)
- The `I18nGenerator` reads JSON files (marked as `<AdditionalFiles>` in .csproj) and generates string properties at compile-time
- Usage: `Strings.Format(Strings.KeyName, args)`
- Locale resolution: User config → System culture → Default (en-US)

### AOT Compatibility

The project uses **Native AOT compilation** (`<PublishAot>true`):

- **No reflection** at runtime - all code generation happens at compile-time
- JSON serialization uses `AppJsonContext` (src/Slack/Serialization/) with `System.Text.Json` source generation
- Version info uses `MinVer` for semantic versioning from git tags

## Key Files

| File | Purpose |
|------|---------|
| `src/Slack/Program.cs` | Entry point, command routing |
| `src/Slack/Commands/*.cs` | Command implementations (11 commands) |
| `src/Slack/Infrastructure/ICommand.cs` | Command interface |
| `src/Slack/Infrastructure/CommandRegistry.cs` | In-memory command registry |
| `src/Slack/Infrastructure/AutoRegisterCommandAttribute.cs` | Attribute for auto-registration |
| `src/Slack.Generators/CommandRegistrationGenerator.cs` | Roslyn analyzer for command registration |
| `src/Slack.Generators/I18nGenerator.cs` | Roslyn analyzer for localization code generation |
| `src/Slack/Models/SlackConfig.cs` | Local project config model |
| `src/Slack/Models/GlobalConfig.cs` | Global user settings model |
| `src/Slack/Serialization/AppJsonContext.cs` | AOT-compatible JSON serialization |

## Adding New Commands

1. Create new class in `src/Slack/Commands/` implementing `ICommand`
2. Add `[AutoRegisterCommand]` attribute to the class
3. Implement required properties: `Name`, `Description`, `Aliases`, `Execute()`
4. Build - the command will be automatically registered

If your command needs access to `CommandRegistry` (e.g., to list all commands like `HelpCommand`):
```csharp
[AutoRegisterCommand(Order = int.MaxValue)]
public class MyCommand : ICommand
{
    private readonly CommandRegistry _registry;

    public MyCommand(CommandRegistry registry)
    {
        _registry = registry;
    }
    // ...
}
```

## Adding Localization Strings

1. Add key-value pairs to both `src/Slack/Localization/locales/en.json` and `zh-CN.json`
2. Rebuild - the `I18nGenerator` will create static properties in `Strings.cs`
3. Use in code: `Strings.YourKeyName` or `Strings.Format(Strings.KeyName, arg1, arg2)`

## Dependencies

- **Spectre.Console** (v0.50.0) - Rich terminal UI framework
- **TextCopy** (v6.2.1) - Cross-platform clipboard operations
- **MinVer** (v6.0.0) - Git-based semantic versioning

## CI/CD

GitHub Actions workflow (`.github/workflows/publish.yml`):
- Triggers on version tags (`v*`) or manual dispatch
- Builds 8 platform variants in parallel
- Creates GitHub release with native binaries
- Archives: `.tar.gz` for Unix, `.zip` for Windows

## Testing Commands Locally

```bash
# After building, run the binary directly
./publish/osx-arm64/slack help
./publish/osx-arm64/slack up
./publish/osx-arm64/slack version

# Or use dotnet run for development
dotnet run --project src/Slack/Slack.csproj -- init
dotnet run --project src/Slack/Slack.csproj -- config
```

## Code Style Notes

- **No reflection**: All runtime behavior must be AOT-compatible
- **Minimal abstractions**: Commands are self-contained, no over-engineering
- **Direct dependencies**: Commands can directly instantiate models and utilities
- **Localization required**: All user-facing strings must use `Strings` class
- **Exit codes**: Commands return `0` for success, non-zero for errors
