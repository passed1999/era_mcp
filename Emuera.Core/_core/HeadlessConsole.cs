using MinorShift.Emuera.Analysis;
using MinorShift.Emuera.Runtime;
using MinorShift.Emuera.Runtime.Utils;
using MinorShift.Emuera.UI.Game;
using System.Collections.Generic;

namespace MinorShift.Emuera.GameView;

/// <summary>
/// IConsoleOutput 的无头实现，用于语法分析。
/// 关键：诊断在 <see cref="PrintWarning"/> 处采集——因为 ParserMediator.FlushWarningList()
/// 会在加载/解析管线中多次把每条警告推给 console.PrintWarning 后清空内部列表，
/// 事后读 warningList 取不到。故采集点必须在此推送处。其余输出一律 no-op。
/// </summary>
internal sealed class HeadlessConsole : IConsoleOutput
{
	private readonly List<Diagnostic> diagnostics = new();
	public IReadOnlyList<Diagnostic> Diagnostics => diagnostics;
	public void ClearDiagnostics() => diagnostics.Clear();

	// ---- 诊断采集点 ----
	public void PrintWarning(string str, ScriptPosition? position, int level)
		=> diagnostics.Add(Diagnostic.From(str, position, level));

	public void PrintErrorButton(string str, ScriptPosition? pos, int level = 0)
		=> diagnostics.Add(Diagnostic.From(str, pos, level));

	// ---- 状态 / 标志 ----
	public bool RunERBFromMemory { get; set; }
	public bool IsRunning => true;
	public bool Enabled => false;
	public bool noOutputLog { get; set; }
	public bool MesSkip { get; set; }
	public bool updatedGeneration { get; set; }
	public void UpdateGeneration() { }
	public bool LastLineIsEmpty => false;
	public bool LastLineIsTemporary => false;
	public DisplayLineAlignment Alignment { get; set; }

	// ---- 输出（分析无需真正渲染，全部 no-op）----
	public void Print(string str, bool lineEnd = true) { }
	public void PrintC(string str, bool alignmentRight) { }
	public void PrintSingleLine(string str) { }
	public void PrintSingleLine(string str, bool temporary) { }
	public void PrintError(string str) { }
	public void PrintSystemLine(string str) { }
	public void PrintFlush(bool force) { }
	public void PrintTemporaryLine(string str) { }
	public void PrintBar() { }
	public void NewLine() { }
	public void deleteLine(int argNum) { }
	public void ClearText() { }

	// ---- 样式 / 窗口 ----
	public void ResetStyle() { }
	public void RefreshStrings(bool force_Paint) { }
	public void ReloadErbFinished() { }
	public void setStBar(string barStr) { }
	public void SetWindowTitle(string str) { }

	// ---- 错误 / 流程 ----
	public void ThrowError(bool playSound) { }
	public void ThrowTitleError(bool error) { }
	public void WaitInput(InputRequest req) { }
	public void ReadAnyKey(bool anykey = false, bool stopMesskip = false) { }

	// ---- 日志 ----
	public bool OutputLog(string filename, bool hideInfo) => true;
	public bool OutputSystemLog(string filename) => true;

	// ---- 调试 ----
	public void DebugAddTraceLog(string str) { }
	public void DebugRemoveTraceLog() { }
	public void DebugClearTraceLog() { }

	public void Dispose() { }
}
