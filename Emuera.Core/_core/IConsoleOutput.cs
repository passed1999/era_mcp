using MinorShift.Emuera.Runtime;
using MinorShift.Emuera.Runtime.Utils;
using MinorShift.Emuera.UI.Game;
using System;

namespace MinorShift.Emuera.GameView;

/// <summary>
/// 引擎对"控制台/视图"的抽象。EmueraConsole（WinForms）与 HeadlessConsole（无头分析）均实现它，
/// 使 Process / ParserMediator / 加载器 / 解析器不再硬依赖 WinForms。
/// 成员集合 = Runtime 层实际调用的 console 表面；按编译器报错增量补全。
/// </summary>
internal interface IConsoleOutput : IDisposable
{
	// ---- 状态 / 标志 ----
	bool RunERBFromMemory { get; set; }
	bool IsRunning { get; }
	bool Enabled { get; }
	bool noOutputLog { get; set; }
	bool MesSkip { get; set; }
	bool updatedGeneration { get; set; }
	void UpdateGeneration();
	bool LastLineIsEmpty { get; }
	bool LastLineIsTemporary { get; }
	DisplayLineAlignment Alignment { get; set; }

	// ---- 输出 ----
	void Print(string str, bool lineEnd = true);
	void PrintC(string str, bool alignmentRight);
	void PrintSingleLine(string str);
	void PrintSingleLine(string str, bool temporary);
	void PrintError(string str);
	void PrintErrorButton(string str, ScriptPosition? pos, int level = 0);
	void PrintSystemLine(string str);
	void PrintWarning(string str, ScriptPosition? position, int level);
	void PrintFlush(bool force);
	void PrintTemporaryLine(string str);
	void PrintBar();
	void NewLine();
	void deleteLine(int argNum);
	void ClearText();

	// ---- 样式 / 窗口 ----
	void ResetStyle();
	void RefreshStrings(bool force_Paint);
	void ReloadErbFinished();
	void setStBar(string barStr);
	void SetWindowTitle(string str);

	// ---- 错误 / 流程 ----
	void ThrowError(bool playSound);
	void ThrowTitleError(bool error);
	void WaitInput(InputRequest req);
	void ReadAnyKey(bool anykey = false, bool stopMesskip = false);

	// ---- 日志 ----
	bool OutputLog(string filename, bool hideInfo);
	bool OutputSystemLog(string filename);

	// ---- 调试 ----
	void DebugAddTraceLog(string str);
	void DebugRemoveTraceLog();
	void DebugClearTraceLog();
}
