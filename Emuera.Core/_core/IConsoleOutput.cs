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

// 视图重绘状态（原 EmueraConsole 关联枚举）。
internal enum ConsoleRedraw { None = 0, Normal = 1 }

/// <summary>
/// 引擎对"控制台/视图"的抽象。EmueraConsole（WinForms，App 侧）与 HeadlessConsole（无头分析，Core 侧）
/// 均实现它，使 Process / ParserMediator / 加载器 / 解析器 / 视图模型不再硬依赖具体 WinForms 控制台。
/// 成员集合 = 引擎 + 视图层实际调用的 EmueraConsole 表面。分析路径只会触达其中一小撮（加载/解析），
/// 其余为执行/渲染期成员——在 HeadlessConsole 中一律 no-op，绝不在语法分析中被调用。
/// </summary>
internal interface IConsoleOutput : IDisposable
{
	// ===== 状态 / 标志 =====
	bool RunERBFromMemory { get; set; }
	bool IsRunning { get; }
	bool IsActive { get; }
	bool IsTimeOut { get; }
	bool Enabled { get; }
	bool EmptyLine { get; }
	bool noOutputLog { get; set; }
	bool MesSkip { get; set; }
	bool AlwaysRefresh { get; set; }
	bool UseSetColorStyle { get; set; }
	bool UseUserStyle { get; set; }
	bool updatedGeneration { get; set; }
	bool LastLineIsEmpty { get; }
	bool LastLineIsTemporary { get; }
	int LastButtonGeneration { get; }
	int NewButtonGeneration { get; }
	int GetLineNo { get; }
	long LineCount { get; }
	int ClientWidth { get; }
	int ClientHeight { get; }
	Color bgColor { get; set; }
	DisplayLineAlignment Alignment { get; set; }
	ConsoleRedraw Redraw { get; }

	// ===== 视图模型访问器 =====
	StringStyle StringStyle { get; }
	StringMeasure StrMeasure { get; }
	PrintStringBuffer PrintBuffer { get; }
	List<ConsoleDisplayLine> DisplayLineList { get; }
	Dictionary<int, List<AConsoleDisplayNode>> EscapedParts { get; }
	ConsoleButtonString PointingSring { get; }
	ConsoleDisplayLine[] GetDisplayLines(long lineNo);
	ConsoleDisplayLine[] PopDisplayingLines();
	int GetLinePointY(int lineNo);
	MainWindow Window { get; }

	// ===== 位图缓存（ConsoleButtonString 渲染用）=====
	ConsoleButtonString[] bitmapCacheArray { get; }
	nint bitmapCacheArrayIndex { get; set; }
	bool bitmapCacheEnabledForNextLine { get; set; }

	// ===== 世代 / 重绘 =====
	void UpdateGeneration();
	void forceUpdateGeneration();
	void SetRedraw(long i);
	void setRedrawTimer(int tickcount);
	void RefreshStrings(bool force_Paint);
	void ReloadErbFinished();

	// ===== 输出 =====
	void Print(string str, bool lineEnd = true);
	void PrintC(string str, bool alignmentRight);
	void PrintSingleLine(string str);
	void PrintSingleLine(string str, bool temporary);
	void PrintPlain(string str);
	void PrintPlainWithSingleLineFix(string str);
	void PrintError(string str);
	void PrintErrorButton(string str, ScriptPosition? pos, int level = 0);
	void PrintSystemLine(string str);
	void PrintWarning(string str, ScriptPosition? position, int level);
	void PrintFlush(bool force);
	void PrintTemporaryLine(string str);
	void PrintBar();
	void printCustomBar(string barStr, bool isConst);
	void PrintButton(string str, string p);
	void PrintButton(string str, long p);
	void PrintButtonC(string str, string p, bool isRight);
	void PrintButtonC(string str, long p, bool isRight);
	void PrintHtml(string str, bool toPrintBuffer);
	void PrintHTMLIsland(string html);
	void ClearHTMLIsland();
	void PrintImg(string name, string nameb, string namem, MixedNum height, MixedNum width, MixedNum ypos);
	void PrintShape(string type, MixedNum[] param);
	void NewLine();
	void deleteLine(int argNum);
	void ClearText();
	void ClearDisplay();

	// ===== 样式 / 字体 / 工具提示 =====
	void ResetStyle();
	void SetStringStyle(FontStyle fs);
	void SetStringStyle(Color color);
	void SetBgColor(Color color);
	void SetFont(string fontname);
	void SetToolTipColor(Color foreColor, Color backColor);
	void SetToolTipDelay(int delay);
	void SetToolTipDuration(int duration);
	void SetToolTipFontName(string fn);
	void SetToolTipFontSize(long fs);
	void SetToolTipFormat(long f);
	void SetToolTipImg(bool b);
	void CustomToolTip(bool b);

	// ===== 背景图 =====
	void AddBackgroundImage(string name, long depth, float opacity);
	void RemoveBackground(string key);
	void ClearBackgroundImage();

	// ===== CBG 图形子系统 =====
	void CBG_Clear();
	void CBG_ClearBMap();
	void CBG_ClearButton();
	void CBG_ClearRange(int zmin, int zmax);
	bool CBG_SetButtonImage(int buttonValue, ASprite imageN, ASprite imageB, int x, int y, int zdepth, string tooltip = null);
	bool CBG_SetButtonMap(GraphicsImage gra);
	bool CBG_SetGraphics(GraphicsImage gra, int x, int y, int zdepth);
	bool CBG_SetImage(ASprite image, int x, int y, int zdepth);

	// ===== 按钮 / 鼠标 =====
	bool ButtonIsPointing(ConsoleButtonString button);
	bool ButtonIsSelected(ConsoleButtonString button);
	Point GetMousePosition();
	bool MoveMouse(Point point);

	// ===== 窗口 / 状态栏 =====
	void setStBar(string barStr);
	string getStBar(string barStr);
	string getDefStBar();
	void SetWindowTitle(string str);
	string GetWindowTitle();

	// ===== 错误 / 流程 =====
	void ThrowError(bool playSound);
	void ThrowTitleError(bool error);
	void WaitInput(InputRequest req);
	void ReadAnyKey(bool anykey = false, bool stopMesskip = false);
	void Await(int time);
	void forceStopTimer();
	void Quit();
	void ForceQuit();

	// ===== 日志 =====
	bool OutputLog(string filename, bool hideInfo);
	bool OutputSystemLog(string filename);

	// ===== 调试 =====
	void DebugAddTraceLog(string str);
	void DebugRemoveTraceLog();
	void DebugClearTraceLog();
	void DebugClear();
	void DebugNewLine();
	void DebugPrint(string str);
}
