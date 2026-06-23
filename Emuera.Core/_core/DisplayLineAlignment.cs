namespace MinorShift.Emuera.UI.Game;

// 从 UI/Game/ConsoleDisplayLine.cs 迁出到 Core：解析/执行层（Process、Creator.Method）会引用它，
// 而该枚举本身与 WinForms 无关，故归入跨平台核心。
internal enum DisplayLineAlignment
{
	LEFT = 0,
	CENTER = 1,
	RIGHT = 2,
}
