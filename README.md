# 🐟 Slack - 上班摸鱼神器

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

### 编译安装

```bash
# 克隆项目
git clone https://github.com/your-username/slack.git
cd slack/src/Slack

# AOT 发布
dotnet publish -c Release

# 复制到 PATH（可选）
cp bin/Release/net10.0/osx-arm64/publish/slack /usr/local/bin/
```

## 🚀 使用

### 开始"工作"

```bash
slack up
```

按 `Q` 或 `Escape` 优雅退出（显示构建成功），按 `Ctrl+C` 中断构建。

### 配置

```bash
```bash
# 进入交互式配置向导
slack config

# 重置为默认配置
slack config --reset
```

### 配置选项

| 配置项 | 说明 | 示例 |
|--------|------|------|
| 项目名称 | 项目名称 | `my-api` |
| 技术栈 | 技术栈 | `Node.js`/`.NET`/`Python` |
| 包管理器 | 包管理器 | `pnpm`/`NuGet`/`pip` |
| 运行时版本 | 运行时版本 | `v22.1.0`/`10.0` |
| 环境 | 运行环境 | `Production`/`Development` |
| 界面语言 | 界面显示语言 | `zh-CN`/`en-US`/`Follow System` |

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
│   └── ConfigCommand.cs    # 配置命令实现
└── Models/
    └── SlackConfig.cs      # 配置模型
```

## 🛠️ 技术栈

- **.NET 10** - 最新 LTS 版本
- **Spectre.Console** - 精美的终端 UI
- **AOT 编译** - 原生可执行文件，无需运行时

## 📄 License

MIT License
