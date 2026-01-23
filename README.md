# 🐟 Slack - 上班摸鱼神器

> **简体中文** | [English](./README_EN.md)

> 当你正在看小说或摸鱼时，敲下 `slack up`，终端就会出现逼真的构建输出刷屏，让你看起来很忙！

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![AOT](https://img.shields.io/badge/AOT-Native-green)
![License](https://img.shields.io/badge/License-MIT-blue)

## ✨ 特性

- 🚀 **逼真的构建输出** - 进度条、编译日志、依赖树，应有尽有
- ⚡ **AOT 原生编译** - 启动快，体积小 (~5MB)
- 🎨 **多技术栈支持** - Node.js、.NET、Python、Go、Rust、Java
- 🌐 **多语言支持** - 支持简体中文和英文，自动跟随系统或手动指定
- ⚠️ **真实感满满** - 随机 WARNING、可恢复 ERROR、忽快忽慢的进度条
- 🔧 **高度可配置** - 自定义项目名、技术栈、运行环境

## 📦 安装

### 一键安装

**Linux / macOS:**

```bash
curl -fsSL https://raw.githubusercontent.com/iamlovedit/slack/master/install.sh | bash
```

**Windows (PowerShell):**

```powershell
irm https://raw.githubusercontent.com/iamlovedit/slack/master/install.ps1 | iex
```

### Homebrew (macOS)

```bash
brew tap iamlovedit/tap
brew install slack
```

### 手动安装

从 [GitHub Releases](https://github.com/iamlovedit/slack/releases) 下载对应平台的压缩包，解压后将可执行文件放到 PATH 目录下即可。

| 平台 | 架构 | 文件 |
|------|------|------|
| Windows | x64 | `slack-win-x64.zip` |
| Windows | ARM64 | `slack-win-arm64.zip` |
| Linux | x64 | `slack-linux-x64.tar.gz` |
| Linux | ARM64 | `slack-linux-arm64.tar.gz` |
| Linux (musl) | x64 | `slack-linux-musl-x64.tar.gz` |
| Linux (musl) | ARM64 | `slack-linux-musl-arm64.tar.gz` |
| macOS | x64 | `slack-osx-x64.tar.gz` |
| macOS | ARM64 | `slack-osx-arm64.tar.gz` |

## 🔄 升级

### 一键升级

**Linux / macOS:**

```bash
curl -fsSL https://raw.githubusercontent.com/iamlovedit/slack/main/upgrade.sh | bash
```

**Windows (PowerShell):**

```powershell
irm https://raw.githubusercontent.com/iamlovedit/slack/main/upgrade.ps1 | iex
```

### Homebrew (macOS)

```bash
brew upgrade slack
```

## 🚀 使用

### 命令一览

| 命令 | 别名 | 说明 |
|------|------|------|
| `slack up` | - | 开始"工作"，显示逼真的构建输出 |
| `slack init [path]` | - | 在当前目录或指定路径初始化项目配置 |
| `slack config` | - | 进入交互式配置向导 |
| `slack upgrade` | - | 检查并升级到最新版本 |
| `slack version` | `-v`, `--version` | 显示版本号 |
| `slack help` | `-h`, `--help` | 显示帮助信息 |

### 开始"工作"

```bash
slack up
```

- 按 `Q` 或 `Escape` 优雅退出（显示构建成功总结）
- 按 `Ctrl+C` 中断构建

### 初始化项目

```bash
# 在当前目录初始化配置
slack init

# 在指定路径初始化配置
slack init ./my-project
```

初始化命令会引导你完成配置向导，并在目标目录下创建 `.slack.json` 配置文件。

### 配置

```bash
# 进入交互式配置向导
slack config

# 重置为默认配置
slack config --reset
```

### 版本与帮助

```bash
# 查看版本
slack version
slack -v

# 查看帮助
slack help
slack -h
```

### 配置选项

| 配置项 | 说明 | 示例 |
|--------|------|------|
| 项目名称 | 项目名称 | `my-api` |
| 编程语言 | 主要编程语言 | `C#`/`TypeScript`/`Python`/`Go`/`Rust` |
| 技术栈 | 框架或技术栈 | `ASP.NET Core`/`Next.js`/`FastAPI` |
| 包管理器 | 包管理器 | `NuGet`/`pnpm`/`pip`/`Cargo` |
| 运行时版本 | 运行时版本 | `.NET 10.0`/`Node v22.1.0` |
| 环境 | 运行环境 | `Production`/`Development`/`Staging` |
| 界面语言 | 界面显示语言 | `zh-CN`/`en-US`/自动跟随系统 |

## 🎭 效果展示

运行 `slack up` 后会随机显示：

### 进度条（忽快忽慢，90%会卡住）
```
Compiling Services.Auth ━━━━━━━━━━━━━━━━━━━━  94% ⣯
```

### 编译日志（带警告）
```
  Controllers.Api:139 (26ms)
  ⚠ Data.Repository:270 - Deprecated method call
  Services.Auth:250 (173ms)
  └─ Completed with warnings
```

### 随机警告
```
⚠ WARN: TODO found: 'Fix this later' in Extensions.DI:445
⚠ WARN: Security: Input not sanitized in Data.Repository
```

### 可恢复错误
```
✗ ERROR: Connection timeout, retrying...
⠦ Recovering...
✓ Recovered successfully
```

### 依赖树
```
📦 Controllers.Api
├── Services.Auth
│   ├── Data.Repository
│   └── Cache.Redis ⚠
└── Middleware.Logging
```

### 构建成功总结
```
─────────────── Build Summary ───────────────
Status      SUCCESS
Project     awesome-api
Duration    25.3s
Warnings    7
Errors      0

✓ awesome-api build completed successfully!
```

## 📁 项目结构

```
src/Slack/
├── Program.cs              # 程序入口，命令行解析
├── Slack.csproj            # 项目配置
├── Commands/
│   ├── UpCommand.cs        # 摸鱼命令实现
│   ├── InitCommand.cs      # 项目初始化命令
│   ├── ConfigCommand.cs    # 配置命令实现
│   ├── VersionCommand.cs   # 版本命令实现
│   └── HelpCommand.cs      # 帮助命令实现
├── Localization/           # 多语言支持
└── Models/
    └── SlackConfig.cs      # 配置模型
```

## 🛠️ 技术栈

- **.NET 10** - 最新 LTS 版本
- **Spectre.Console** - 精美的终端 UI
- **AOT 编译** - 原生可执行文件，无需运行时

## 📄 License

MIT License
