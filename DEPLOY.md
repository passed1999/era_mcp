# 部署与使用 — era 语法分析器 / MCP 服务器

无头跨平台的 era 语法分析核心，提供两种用法：**命令行 (CLI)** 和 **MCP 服务器**（供 Claude Code 等客户端调用）。
给定一个 era 项目（含 `csv/` 与 `erb/`），运行加载+解析管线，返回结构化诊断（消息 / 文件 / 行号 / 等级）。

> 已在真实大型项目（eratohoK，2308 ERB）上验证：Linux 下零误报、零崩溃。

---

## 1. 前置条件

- **.NET 10 SDK**（本机已装于 `~/.dotnet`，版本 10.0.301）。确保在 PATH 中：
  ```bash
  export PATH="$HOME/.dotnet:$PATH"
  ```
- Linux / Windows 均可。
  - Linux 只构建 `Emuera.Core` / `Emuera.Mcp` / `Emuera.Analyzer.Cli`（分析所需）。
  - `Emuera`（原 WinForms App，`net10.0-windows`）**仅 Windows 可构建**，分析功能不依赖它。

## 2. 构建 / 部署

开发期直接构建：
```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build Emuera.Mcp/Emuera.Mcp.csproj -c Release
```

部署期发布到独立目录（推荐，便于在客户端中引用固定路径）：
```bash
dotnet publish Emuera.Mcp/Emuera.Mcp.csproj -c Release -o ~/era-mcp
# 入口: ~/era-mcp/era-mcp.dll
```

## 3. 命令行 (CLI) 用法

```bash
# 整项目分析
dotnet run --project Emuera.Analyzer.Cli -c Release -- <项目根目录>

# 单文件 / 子目录（仍以项目根加载 ERH/CSV 上下文）
dotnet run --project Emuera.Analyzer.Cli -c Release -- <项目根目录> <目标文件或文件夹>
```

示例：
```bash
dotnet run --project Emuera.Analyzer.Cli -c Release -- ./testgame
dotnet run --project Emuera.Analyzer.Cli -c Release -- ./testgame ./testgame/erb/bad.ERB
```

输出（诊断行格式 `[等级] 文件:行  消息`）：
```
projectRoot : .../testgame
files       : 2
success     : True
diagnostics : 2
  [warning] bad.ERB:2  対応する"ENDIF"の無い"IF"文です
```

- **项目根**：包含 `csv/`（或 `CSV/`）与 `erb/`（或 `ERB/`）的目录。大小写自动识别。
- **success**：解析完成且无致命错误（level 3）。

## 4. MCP 服务器用法

服务器走 **stdio**，暴露工具 `analyze_era_project`：

| 参数 | 说明 |
|---|---|
| `projectRoot` | 项目根的绝对路径（含 csv/erb） |
| `target`（可选） | 单个 `.ERB` 文件或子目录；留空 = 整个 erb 目录 |

返回 JSON：
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

`level` 含义：`0` 提示 / `1` 信息 / `2` 警告 / `3` 致命；`severity` 为其可读映射。

### 接入 Claude Code（或其它 MCP 客户端）

在 MCP 配置中加入（用 publish 后的固定路径）：
```json
{
  "mcpServers": {
    "era-analyzer": {
      "command": "dotnet",
      "args": ["/home/island/era-mcp/era-mcp.dll"]
    }
  }
}
```

> 注意：服务器**所有日志走 stderr**，stdout 仅承载 JSON-RPC，请勿向 stdout 打印其它内容。

### 手动冒烟测试（不接客户端）

```bash
dotnet ~/era-mcp/era-mcp.dll   # 启动后通过 stdin 发送 JSON-RPC initialize / tools/call
```

## 5. 注意事项

- **单文件检查仍需项目根**：未定义变量/函数/宏、参数个数等跨文件语义依赖项目的 ERH/CSV。
- **只解析、不执行**：分析跑加载+解析（不运行游戏逻辑、不写存档、不写 `emuera.config`），对项目目录无副作用。
- **跨平台大小写**：Linux 区分大小写的文件名（如 `Abl.csv` vs `ABL.CSV`）已自动解析。
- 重复分析（MCP 长驻进程）每次调用前会复位全部静态状态，互不污染。

更多设计细节见 [README.analyzer.md](README.analyzer.md)。
