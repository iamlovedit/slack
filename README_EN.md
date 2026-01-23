# 🐟 Slack - The Ultimate Slacking Tool

> [简体中文](./README.md) | **English**

> When you are reading novels or slacking off, type `slack up`, and the terminal will display realistic build output, making you look very busy!

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![AOT](https://img.shields.io/badge/AOT-Native-green)
![License](https://img.shields.io/badge/License-MIT-blue)

## ✨ Features

- 🚀 **Realistic Build Output** - Progress bars, compile logs, dependency trees, everything you need.
- ⚡ **AOT Native Compilation** - Fast startup, small size (~5MB).
- 🎨 **Multi-Stack Support** - Node.js, .NET, Python, Go, Rust, Java.
- 🌐 **Multi-Language Support** - Supports Simplified Chinese and English, automatically follows system or manual configuration.
- ⚠️ **Highly Realistic** - Random WARNINGs, recoverable ERRORs, progress bars that stall.
- 🔧 **Highly Configurable** - Customize project name, tech stack, environment.

## 🚀 Usage

### Command Overview

| Command | Aliases | Description |
|---------|---------|-------------|
| `slack up` | - | Start "working", display realistic build output |
| `slack init [path]` | - | Initialize project configuration in current or specified directory |
| `slack config` | - | Enter interactive configuration wizard |
| `slack version` | `-v`, `--version` | Show version |
| `slack help` | `-h`, `--help` | Show help |

### Start "Working"

```bash
slack up
```

- Press `Q` or `Escape` to gracefully exit (shows build summary)
- Press `Ctrl+C` to interrupt build

### Initialize Project

```bash
# Initialize configuration in current directory
slack init

# Initialize configuration in specified path
slack init ./my-project
```

The init command will guide you through the configuration wizard and create a `.slack.json` configuration file in the target directory.

### Configuration

```bash
# Enter interactive configuration wizard
slack config

# Reset to default configuration
slack config --reset
```

### Version & Help

```bash
# Show version
slack version
slack -v

# Show help
slack help
slack -h
```

### Configuration Options

| Option | Description | Example |
|--------|-------------|---------|
| Project Name | Project name | `my-api` |
| Language | Primary programming language | `C#`/`TypeScript`/`Python`/`Go`/`Rust` |
| Tech Stack | Framework or tech stack | `ASP.NET Core`/`Next.js`/`FastAPI` |
| Package Manager | Package manager | `NuGet`/`pnpm`/`pip`/`Cargo` |
| Runtime Version | Runtime version | `.NET 10.0`/`Node v22.1.0` |
| Environment | Runtime environment | `Production`/`Development`/`Staging` |
| Locale | Display language | `zh-CN`/`en-US`/Follow System |

## 🎭 Showcase

After running `slack up`, you will see randomly generated output:

### Progress Bar (Stalls at 90%)
```
Compiling Services.Auth ━━━━━━━━━━━━━━━━━━━━  94% ⣯
```

### Compile Logs (With Warnings)
```
  Controllers.Api:139 (26ms)
  ⚠ Data.Repository:270 - Deprecated method call
  Services.Auth:250 (173ms)
  └─ Completed with warnings
```

### Random Warnings
```
⚠ WARN: TODO found: 'Fix this later' in Extensions.DI:445
⚠ WARN: Security: Input not sanitized in Data.Repository
```

### Recoverable Errors
```
✗ ERROR: Connection timeout, retrying...
⠦ Recovering...
✓ Recovered successfully
```

### Dependency Tree
```
📦 Controllers.Api
├── Services.Auth
│   ├── Data.Repository
│   └── Cache.Redis ⚠
└── Middleware.Logging
```

### Build Summary
```
─────────────── Build Summary ───────────────
Status      SUCCESS
Project     awesome-api
Duration    25.3s
Warnings    7
Errors      0

✓ awesome-api build completed successfully!
```

## 📁 Project Structure

```
src/Slack/
├── Program.cs              # Entry point, CLI parsing
├── Slack.csproj            # Project configuration
├── Commands/
│   ├── UpCommand.cs        # Slacking command implementation
│   ├── InitCommand.cs      # Project initialization command
│   ├── ConfigCommand.cs    # Configuration command implementation
│   ├── VersionCommand.cs   # Version command implementation
│   └── HelpCommand.cs      # Help command implementation
├── Localization/           # Multi-language support
└── Models/
    └── SlackConfig.cs      # Configuration model
```

## 🛠️ Tech Stack

- **.NET 10** - Latest LTS version
- **Spectre.Console** - Beautiful Console UI
- **AOT Compilation** - Native executable, no runtime required

## 📄 License

MIT License
