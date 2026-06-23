using MinorShift.Emuera.Analysis;
using MinorShift.Emuera.Forms;
using MinorShift.Emuera.Runtime;
using MinorShift.Emuera.Runtime.Utils;
using static MinorShift.Emuera.Runtime.Utils.EvilMask.Utils;
using MinorShift.Emuera.UI.Game;
using MinorShift.Emuera.UI.Game.Image;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MinorShift.Emuera.GameView;

/// <summary>
/// IConsoleOutput 的无头实现，用于语法分析。
/// 关键：诊断在 <see cref="PrintWarning"/> 处采集——ParserMediator.FlushWarningList() 会在
/// 加载/解析管线中多次把每条警告推给 console.PrintWarning 后清空内部列表，事后读取不到，
/// 故采集点必须在此推送处。其余成员皆为执行/渲染期接口，分析路径不触达，统一 no-op。
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
	public bool IsActive => false;
	public bool IsTimeOut => false;
	public bool Enabled => false;
	public bool EmptyLine => true;
	public bool noOutputLog { get; set; }
	public bool MesSkip { get; set; }
	public bool AlwaysRefresh { get; set; }
	public bool UseSetColorStyle { get; set; }
	public bool UseUserStyle { get; set; }
	public bool updatedGeneration { get; set; }
	public bool LastLineIsEmpty => false;
	public bool LastLineIsTemporary => false;
	public int LastButtonGeneration => 0;
	public int NewButtonGeneration => 0;
	public int GetLineNo => 0;
	public long LineCount => 0;
	public int ClientWidth => 0;
	public int ClientHeight => 0;
	public Color bgColor { get; set; }
	public DisplayLineAlignment Alignment { get; set; }
	public ConsoleRedraw Redraw => ConsoleRedraw.None;

	// ---- 视图模型访问器 ----
	public StringStyle StringStyle => default;
	public StringMeasure StrMeasure => null;
	public PrintStringBuffer PrintBuffer => null;
	public List<ConsoleDisplayLine> DisplayLineList => new();
	public Dictionary<int, List<AConsoleDisplayNode>> EscapedParts => new();
	public ConsoleButtonString PointingSring => null;
	public ConsoleDisplayLine[] GetDisplayLines(long lineNo) => Array.Empty<ConsoleDisplayLine>();
	public ConsoleDisplayLine[] PopDisplayingLines() => Array.Empty<ConsoleDisplayLine>();
	public int GetLinePointY(int lineNo) => 0;
	public MainWindow Window => null;

	// ---- 位图缓存 ----
	public ConsoleButtonString[] bitmapCacheArray => Array.Empty<ConsoleButtonString>();
	public nint bitmapCacheArrayIndex { get; set; }
	public bool bitmapCacheEnabledForNextLine { get; set; }

	// ---- 世代 / 重绘 ----
	public void UpdateGeneration() { }
	public void forceUpdateGeneration() { }
	public void SetRedraw(long i) { }
	public void setRedrawTimer(int tickcount) { }
	public void RefreshStrings(bool force_Paint) { }
	public void ReloadErbFinished() { }

	// ---- 输出 ----
	public void Print(string str, bool lineEnd = true) { }
	public void PrintC(string str, bool alignmentRight) { }
	public void PrintSingleLine(string str) { }
	public void PrintSingleLine(string str, bool temporary) { }
	public void PrintPlain(string str) { }
	public void PrintPlainWithSingleLineFix(string str) { }
	public void PrintError(string str) { }
	public void PrintSystemLine(string str) { }
	public void PrintFlush(bool force) { }
	public void PrintTemporaryLine(string str) { }
	public void PrintBar() { }
	public void printCustomBar(string barStr, bool isConst) { }
	public void PrintButton(string str, string p) { }
	public void PrintButton(string str, long p) { }
	public void PrintButtonC(string str, string p, bool isRight) { }
	public void PrintButtonC(string str, long p, bool isRight) { }
	public void PrintHtml(string str, bool toPrintBuffer) { }
	public void PrintHTMLIsland(string html) { }
	public void ClearHTMLIsland() { }
	public void PrintImg(string name, string nameb, string namem, MixedNum height, MixedNum width, MixedNum ypos) { }
	public void PrintShape(string type, MixedNum[] param) { }
	public void NewLine() { }
	public void deleteLine(int argNum) { }
	public void ClearText() { }
	public void ClearDisplay() { }

	// ---- 样式 / 字体 / 工具提示 ----
	public void ResetStyle() { }
	public void SetStringStyle(FontStyle fs) { }
	public void SetStringStyle(Color color) { }
	public void SetBgColor(Color color) { }
	public void SetFont(string fontname) { }
	public void SetToolTipColor(Color foreColor, Color backColor) { }
	public void SetToolTipDelay(int delay) { }
	public void SetToolTipDuration(int duration) { }
	public void SetToolTipFontName(string fn) { }
	public void SetToolTipFontSize(long fs) { }
	public void SetToolTipFormat(long f) { }
	public void SetToolTipImg(bool b) { }
	public void CustomToolTip(bool b) { }

	// ---- 背景图 ----
	public void AddBackgroundImage(string name, long depth, float opacity) { }
	public void RemoveBackground(string key) { }
	public void ClearBackgroundImage() { }

	// ---- CBG ----
	public void CBG_Clear() { }
	public void CBG_ClearBMap() { }
	public void CBG_ClearButton() { }
	public void CBG_ClearRange(int zmin, int zmax) { }
	public bool CBG_SetButtonImage(int buttonValue, ASprite imageN, ASprite imageB, int x, int y, int zdepth, string tooltip = null) => false;
	public bool CBG_SetButtonMap(GraphicsImage gra) => false;
	public bool CBG_SetGraphics(GraphicsImage gra, int x, int y, int zdepth) => false;
	public bool CBG_SetImage(ASprite image, int x, int y, int zdepth) => false;

	// ---- 按钮 / 鼠标 ----
	public bool ButtonIsPointing(ConsoleButtonString button) => false;
	public bool ButtonIsSelected(ConsoleButtonString button) => false;
	public Point GetMousePosition() => Point.Empty;
	public bool MoveMouse(Point point) => false;

	// ---- 窗口 / 状态栏 ----
	public void setStBar(string barStr) { }
	public string getStBar(string barStr) => string.Empty;
	public string getDefStBar() => string.Empty;
	public void SetWindowTitle(string str) { }
	public string GetWindowTitle() => string.Empty;

	// ---- 错误 / 流程 ----
	public void ThrowError(bool playSound) { }
	public void ThrowTitleError(bool error) { }
	public void WaitInput(InputRequest req) { }
	public void ReadAnyKey(bool anykey = false, bool stopMesskip = false) { }
	public void Await(int time) { }
	public void forceStopTimer() { }
	public void Quit() { }
	public void ForceQuit() { }

	// ---- 日志 ----
	public bool OutputLog(string filename, bool hideInfo) => true;
	public bool OutputSystemLog(string filename) => true;

	// ---- 调试 ----
	public void DebugAddTraceLog(string str) { }
	public void DebugRemoveTraceLog() { }
	public void DebugClearTraceLog() { }
	public void DebugClear() { }
	public void DebugNewLine() { }
	public void DebugPrint(string str) { }

	public void Dispose() { }
}
