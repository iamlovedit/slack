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

## 📦 Installation

### Compile & Install

#### Windows (Recommended)

The project provides a one-click publish script that generates a standalone single-file executable by default:

```powershell
# Generate single-file release (located in publish/ directory)
.\publish.ps1
```

> **Note**: Defaults to standard single-file publish (good compatibility). If you have Visual Studio C++ Desktop Development tools installed, you can enable AOT compilation in the script for smaller file size.

#### GitHub Actions (CI/CD)

This project has GitHub Actions configured. Every time you push to the `release` branch or create a tag, it will automatically build single-file executables for the following platforms:

- `win-x64` (Windows x64)
- `linux-x64` (Linux x64)
- `osx-x64` (macOS Intel)
- `osx-arm64` (macOS Apple Silicon)

You can download the build artifacts from the GitHub Actions page.

#### Manual Publish

```bash
# Publish as single file (Example: macOS Apple Silicon)
dotnet publish src/Slack/Slack.csproj -c Release -r osx-arm64
```

## 🚀 Usage

### Start "Working"

```bash
slack up
```

Press `Q` or `Escape` to gracefully exit (shows build success). Press `Ctrl+C` to interrupt build.

### Configuration

```bash
# Enter interactive configuration wizard
slack config

# Reset to default configuration
slack config --reset
```

### Configuration Options

| Option | Description | Example |
|--------|-------------|---------|
| Project Name | Project Name | `my-api` |
| Tech Stack | Technology Stack | `Node.js`/`.NET`/`Python` |
| Package Manager | Package Manager | `pnpm`/`NuGet`/`pip` |
| Runtime Version | Runtime Version | `v22.1.0`/`10.0` |
| Environment | Environment | `Production`/`Development` |
| Locale | Display Language | `zh-CN`/`en-US`/`Follow System` |

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
│   ├── UpCommand.cs        # Main logic
│   └── ConfigCommand.cs    # Configuration logic
└── Models/
    └── SlackConfig.cs      # Configuration model
```

## 🛠️ Tech Stack

- **.NET 10** - Latest LTS version
- **Spectre.Console** - Beautiful Console UI
- **AOT Compilation** - Native executable, no runtime required

## 📄 License

MIT License
