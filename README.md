# era_mcp

从 [Emuera](https://ja.osdn.net/projects/emuera/)（era 解释器，WinForms）抽取出的**跨平台无头 era 语法分析核心**，外加一个**纯 C# 的 MCP 服务器**。
给定一个 era 项目（含 `csv/` 与 `erb/`），运行加载 + 解析管线，返回结构化诊断（消息 / 文件 / 行号 / 等级），用于快速检查语法/结构/标识符/参数错误。

- 🐧 **跨平台**：在 Linux / Windows 上运行（无需 WinForms / GUI）
- 🔌 **MCP 服务器**：工具 `analyze_era_project`，可供 Claude Code 等客户端直接调用
- 🖥️ **CLI**：`era-analyze <项目根> [目标]`
- ✅ **真实验证**：在大型项目 [eratohoK](https://github.com/wamekukyouzin/eratohoK)（2308 ERB）上分析**零误报、零崩溃**

> 本仓是 Emuera（EEv56）源码的衍生副本；分析核心与 MCP 服务器为新增部分，原 WinForms 程序保持可在 Windows 构建。

## 安装

### 方式一：下载预编译版（推荐，无需 .NET）

从 [Releases](https://github.com/passed1999/era_mcp/releases/latest) 下载对应平台的自包含包：

| 平台 | 文件 |
|---|---|
| Linux x64 | `era-tools-<版本>-linux-x64.tar.gz` |
| Windows x64 | `era-tools-<版本>-win-x64.zip` |

解压后内含 `era-mcp`（MCP 服务器）与 `era-analyze`（CLI），**目标机无需安装 .NET** 即可运行。

### 方式二：从源码构建

需 [.NET 10 SDK](https://dotnet.microsoft.com/)。

```bash
# CLI
dotnet build Emuera.Analyzer.Cli/Emuera.Analyzer.Cli.csproj -c Release
# MCP 服务器（发布到独立目录）
dotnet publish Emuera.Mcp/Emuera.Mcp.csproj -c Release -o ~/era-mcp
```

> Linux 上只构建 `Emuera.Core` / `Emuera.Mcp` / `Emuera.Analyzer.Cli`；原 WinForms 程序 `Emuera`（`net10.0-windows`）仅 Windows 可构建，分析功能不依赖它。

## CLI 用法

```bash
# 整项目分析
era-analyze <项目根目录>
# 单文件 / 子目录（仍以项目根加载 ERH/CSV 上下文）
era-analyze <项目根目录> <目标文件或文件夹>
```

输出（`[等级] 文件:行  消息`）：

```
projectRoot : .../testgame
files       : 2
success     : True
diagnostics : 1
  [warning] bad.ERB:2  対応する"ENDIF"の無い"IF"文です
```

## MCP 用法

服务器走 **stdio**，暴露工具 `analyze_era_project(projectRoot, target?)`，返回 JSON：

```json
{
  "projectRoot": "...", "target": null, "success": true,
  "fileCount": 2308, "elapsedMs": 6900, "diagnosticCount": 0,
  "diagnostics": [
    { "severity": "warning", "level": 2, "file": ".../bad.ERB", "line": 2,
      "message": "対応する\"ENDIF\"の無い\"IF\"文です" }
  ]
}
```

`level`：`0` 提示 / `1` 信息 / `2` 警告 / `3` 致命。

### 接入 Claude Code

```json
{
  "mcpServers": {
    "era-analyzer": {
      "command": "/path/to/era-mcp"
    }
  }
}
```

> 用源码构建的 framework-dependent 版本时，命令改为 `"command": "dotnet", "args": ["/path/to/era-mcp.dll"]`。
> 服务器所有日志走 stderr，stdout 仅承载 JSON-RPC。

## 工程结构

| 项目 | TFM | 作用 |
|---|---|---|
| `Emuera` | `net10.0-windows` | 原 WinForms 程序（仅 Windows 构建/运行） |
| `Emuera.Core` | `net10.0` | 无头分析核心（复用引擎加载/解析/语句/变量层，跨平台） |
| `Emuera.Analyzer.Cli` | `net10.0` | 命令行 `era-analyze` |
| `Emuera.Mcp` | `net10.0` | MCP stdio 服务器 `era-mcp` |

## 说明

- **单文件检查仍需项目根**：未定义变量/函数/宏、参数个数等跨文件语义依赖项目的 ERH/CSV。
- **只解析、不执行**：分析跑加载 + 解析，不运行游戏逻辑、不写存档、不写 `emuera.config`，对项目目录无副作用。
- **跨平台大小写**：Linux 区分大小写的文件名（如 `Abl.csv` vs `ABL.CSV`、`CSV/` vs `csv/`）已自动解析。

更多细节见 [DEPLOY.md](DEPLOY.md)（部署/接入）与 [README.analyzer.md](README.analyzer.md)（设计要点）。

## 许可

Emuera 部分遵循其原始许可（见 [`Readme/License/`](Readme/License/)）。
