# era 语法分析 — 无头核心 + MCP 服务器

从 Emuera（WinForms）抽取出的**跨平台无头语法分析核心**，加上一个**纯 C# 的 MCP 服务器**，
可对一个 era 项目（`.ERB`/`.ERH`/`csv`）做快速的语法/结构/标识符/参数静态检查，返回结构化诊断。

## 工程结构

| 项目 | TFM | 说明 |
|---|---|---|
| `Emuera` | `net10.0-windows` | 原 WinForms 程序（仅在 Windows 构建/运行；本仓只做了与解耦兼容的小改动） |
| `Emuera.Core` | `net10.0` | **无头分析核心**：复用引擎的加载/解析/语句/变量层 + 视图模型/图像；用 `IConsoleOutput`/`HeadlessConsole` + WinForms 垫片替换 GUI。跨平台。 |
| `Emuera.Analyzer.Cli` | `net10.0` | 命令行冒烟测试：`era-analyze <projectRoot> [target]` |
| `Emuera.Mcp` | `net10.0` | **MCP stdio 服务器**，工具 `analyze_era_project` |

## 前置

- .NET 10 SDK（本机已装于 `~/.dotnet`；确保 `dotnet` 在 PATH）

## 构建

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build Emuera.Mcp/Emuera.Mcp.csproj -c Release
```

## CLI 用法

```bash
dotnet run --project Emuera.Analyzer.Cli -- <项目根目录> [目标文件或文件夹]
# 例：整项目
dotnet run --project Emuera.Analyzer.Cli -- ./testgame
# 例：单文件（仍以项目根加载 ERH/CSV 上下文）
dotnet run --project Emuera.Analyzer.Cli -- ./testgame ./testgame/erb/bad.ERB
```

## MCP 工具

`analyze_era_project(projectRoot, target?)` → 返回 JSON：

```json
{
  "projectRoot": "...", "target": null, "success": true,
  "fileCount": 2, "elapsedMs": 411, "diagnosticCount": 2,
  "diagnostics": [
    { "severity": "warning", "level": 2, "file": ".../bad.ERB", "line": 2,
      "message": "対応する\"ENDIF\"の無い\"IF\"文です" }
  ]
}
```

- `projectRoot`：项目根（含 `csv/` 与 `erb/`）的绝对路径。
- `target`：可选，单个 `.ERB` 文件或子目录；留空解析整个 `erb/`。
- `level`：0 提示 / 1 信息 / 2 警告 / 3 致命；`severity` 为其可读映射。

### 接入 MCP 客户端（如 Claude Code）

```json
{
  "mcpServers": {
    "era-analyzer": {
      "command": "dotnet",
      "args": ["/home/island/emuera-analyzer/Emuera.Mcp/bin/Release/net10.0/era-mcp.dll"]
    }
  }
}
```

## 设计要点 / 注意

- **诊断采集点在 `HeadlessConsole.PrintWarning`**：引擎在加载/解析管线中多次 `FlushWarningList()`
  会清空内部告警列表，故必须在推送点采集，不能事后读 `warningList`。
- **静态单例**：引擎重度依赖 `Program.*`/`GlobalStatic`/`ParserMediator` 等静态状态，
  `EmueraAnalyzer.AnalyzeProject` 以全局锁串行化并在每次调用前 `ResetStatics()` 复位，保证 MCP 长驻进程多次调用互不污染。
- **单文件检查仍需项目根**：跨文件语义（未定义变量/函数/宏、参数个数）依赖 ERH/CSV，
  故 `target` 为单文件时仍以 `projectRoot` 加载上下文。
- **App 仍是独立单体**：`Emuera.Core` 与 `Emuera`（App）是两套独立程序集、共享 `Runtime` 源码；
  App 不引用 Core（避免类型重复），其在 Windows 上的构建未受影响。GUI 渲染/图形/声音的执行期类型在 Core 中
  以垫片/桩满足"编译但不执行"，语法分析绝不触达。
