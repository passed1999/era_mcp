# CLAUDE.md

## Global Protocols

### Language & state
- **User-facing:** Chinese. Respond User in chinese.
- **Tool/model-facing:** English.  
- If any tool returns a **SESSION_ID**, persist it and pass `--SESSION_ID <ID>` for follow-ups.

### Overall
- If the task is simple, multi-model collaboration may be skipped; however, you **must** immediately stop all actions and tell the user **why** collaboration is unnecessary, and **must not** proceed until the user explicitly approves. Example: “This is a simple <task>, so multi-model collaboration is not needed. Do you agree to proceed without any multi-model collaboration for this task? I will wait for your reply and strictly follow this specific collaboration rule.”
- Strictly follow the **Mandatory workflow**. Skipping any phase is considered a **high-risk operation**; you must stop immediately and explain **why** the phase would be skipped. Example: “In the current <phase>, I found <reason>, so the work of the next <phase> has effectively been resolved by <reason>. Do you agree that I skip <phase>? I will wait until you explicitly confirm before continuing to the next phase.”
- Except in rare special cases, **always** collaborate with **Codex and Gemini** by invoking the `Skill` tool directly with `collaborating-with-codex` and `collaborating-with-gemini`. **Do not** look for or run local scripts. **Run in parallel** and **do not** set a timeout.

### Mandatory workflow (do not skip phases)
1. **Phase 1 — Context Retrieval (Auggie)**
   - Call: `mcp__auggie__codebase-retrieval`
   - No assumptions. Retrieve **complete definitions/signatures** (recursive until sufficient).
   - Prefer semantic retrieval; avoid brittle keyword-only approaches.
2. **Phase 2 — Dual-model Planning (Codex + Gemini)**
   - Send **raw requirements** to both models; cross-validate.
   - Produce a **step-by-step plan** (light pseudocode ok).
   - **Hard stop:** end the message with **"Shall I proceed with this plan? (Y/N)"** and do nothing beyond planning until the user says **Y**.
3. **Phase 3 — Prototype Acquisition**
   - **Route A (UI/Styling):** Gemini.
   - **Route B (Backend/Logic):** Codex.
   - Prompts must require: **"OUTPUT: Unified Diff Patch ONLY. Strictly prohibit any actual modifications."**
4. **Phase 4 — Implementation (Claude + Codex + Gemini)**
   - Treat external diffs as **dirty prototypes**: mentally apply → validate → rewrite/refactor into production-quality code.
   - Minimal scope; no redundancy; minimal comments/docs.
   - Do not change externally observable behavior unless explicitly required; if changed, **call it out + explain impact + update/add tests**.
   - Any Error should directly raise a exception, including import error or something else. **Don't hide any error.** 
5. **Phase 5 — Audit & Delivery (Codex + Gemini)**
   - Run **parallel code review** using the produced unified diff + target files.
   - Integrate fixes, then deliver.

### Multi-model execution rules
- Prefer **parallel runs** for Codex/Gemini by calling both `Skill` tools in the same response.
- Always invoke via `Skill` tool — never look up or run local scripts.
- Use **no timeout** for all `Skill` calls.

### Safety & ownership
- External models: **zero filesystem write authority** (diff output only).
- Never hardcode secrets; never commit `.env` or credentials.
- Critical paths must have explicit error handling.
- No blind changes: trace dependencies/impact radius before edits.

### Git
- Before making changes, Checkout to a new dev branch related to the changes topic. Ask user if any changes are not commited
- Do not commit or push unless explicitly requested.
- Do not force-push to `main/master` without approval.

### Web research (no guessing)
- If something is unfamiliar or version-sensitive, search first (priority: official docs → changelog → upstream repo docs → community).

---

## 项目：无头 era 语法分析核心 + MCP 服务器

> 本仓是 `emuera.em` 的独立副本（全新 git），用于在 Emuera 之上做**跨平台无头语法分析**与 **C# MCP 服务器**。原 `emuera.em` 不在此修改。

### 工程结构（同一目录下多项目，无 .sln，逐项目 build）
| 项目 | TFM | 作用 |
|---|---|---|
| `Emuera` | `net10.0-windows` | 原 WinForms 程序，**仅 Windows 可构建/运行**；本仓仅做与解耦兼容的小改 |
| `Emuera.Core` | `net10.0` | **无头分析核心**，复用引擎全部加载/解析/语句/变量层 + 视图模型/图像，无 WinForms，跨平台 |
| `Emuera.Analyzer.Cli` | `net10.0` | 命令行冒烟测试 `era-analyze <root> [target]` |
| `Emuera.Mcp` | `net10.0` | MCP stdio 服务器，工具 `analyze_era_project`（ModelContextProtocol 1.4.0） |

### 架构铁律（改动前必读，违反会静默出错）
1. **诊断采集点在 `HeadlessConsole.PrintWarning`**：`ParserMediator.FlushWarningList()` 在加载/解析管线中被调用约 14 次，每次都会清空内部 `warningList`。**严禁**事后读 `warningList`，必须在 `console.PrintWarning` 推送点采集。
2. **静态单例必须复位**：引擎重度依赖 `Program.*` / `GlobalStatic` / `ParserMediator` / `Config` 静态状态。`EmueraAnalyzer.AnalyzeProject` 已用全局锁串行化并每次 `ResetStatics()`。MCP 长驻进程多次调用靠此隔离——新增静态状态时务必纳入复位。
3. **只解析、不执行**：分析只跑 `Process.Initialize()`（ERH+ERB 加载+解析+标签/参数/跳转解析），**绝不**调用 `DoScript()`。执行层（指令体/图形/声音/窗口）只需"能编译"，故由 `_core/` 下的 WinForms 垫片与视图桩满足；分析路径永不触达。
4. **Core 与 App 是两套独立程序集、共享 `Runtime` 源码**：App **不**引用 Core（避免类型重复）。给引擎加东西若涉及 console，需同步更新 `IConsoleOutput` + `HeadlessConsole`（App 侧 `EmueraConsole` 已实现全部成员）。
5. **单文件检查仍需项目根**：跨文件语义（未定义变量/函数/宏、参数个数）依赖项目的 ERH/CSV，故 `target` 为单文件时仍以 `projectRoot` 加载上下文。

### 关键文件
- 解耦/无头：`Emuera.Core/_core/`（`IConsoleOutput.cs`、`HeadlessConsole.cs`、`WinFormsShim.cs`、`ViewStubs.cs`、`Program.Core.cs`、`SoundStub.cs`、`DisplayLineAlignment.cs`）
- 入口：`Emuera.Core/_core/EmueraAnalyzer.cs`（`AnalyzeProject`）、`Diagnostic.cs`、`AnalysisResult.cs`
- MCP：`Emuera.Mcp/Program.cs`、`EraAnalysisTools.cs`（日志走 **stderr**，stdout 仅 JSON-RPC）
- 测试夹具：`testgame/`（`csv/GAMEBASE.CSV` + `erb/test.ERB` 正常 + `erb/bad.ERB` 缺 ENDIF）
- 文档：`README.analyzer.md`

### 环境与构建
- .NET 10 SDK 在 `~/.dotnet`；命令前 `export PATH="$HOME/.dotnet:$PATH"`。**App（net10.0-windows）在 Linux 无法构建**（无 Windows Desktop 运行时），只构建 `Emuera.Core` / `Emuera.Mcp` / `Emuera.Analyzer.Cli`。
- 跑分析：`dotnet run --project Emuera.Analyzer.Cli -- ./testgame`
- 跑 MCP：`dotnet Emuera.Mcp/bin/Release/net10.0/era-mcp.dll`（stdio）
- 诊断 `level`：0 提示 / 1 信息 / 2 警告 / 3 致命。

### 现状 / TODO
- 四个阶段均已完成并端到端验证（CLI 与 MCP 均能在正确 file:line 报出缺 ENDIF 等错误）。
- 待办：App 尚未改为引用 Core（Windows 上仍是独立单体）；可按需增加检测规则、缓存（按 csv/erh/config 时间戳）、更多测试夹具。
