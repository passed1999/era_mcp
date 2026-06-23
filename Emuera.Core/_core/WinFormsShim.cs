// 无头核心运行在 net10.0（无 -windows），System.Windows.Forms / System.Media 不可用。
// 这里提供引擎仅在执行期/错误提示时触达的极小桩类型，使源码无需改动即可在 Linux 编译。
// 分析路径不会真正调用到有副作用的逻辑（弹窗/剪贴板/UI 泵 都是 no-op）。
#pragma warning disable CA1050 // 故意置于 System.* 命名空间以匹配原有 using

namespace System.Windows.Forms
{
	internal enum DialogResult { None = 0, OK = 1, Cancel = 2, Yes = 6, No = 7 }

	internal static class MessageBox
	{
		public static DialogResult Show(string text) => DialogResult.OK;
		public static DialogResult Show(string text, string caption) => DialogResult.OK;
		public static DialogResult Show(string text, string caption, object buttons) => DialogResult.OK;
	}

	internal static class Application
	{
		// WinForms 消息泵；无头环境下无意义，置空。
		public static void DoEvents() { }
	}

	internal static class Clipboard
	{
		public static void SetDataObject(object data) { }
		public static void SetDataObject(object data, bool copy) { }
		public static string GetText() => string.Empty;
		public static bool ContainsText() => false;
	}

	[System.Flags]
	internal enum TextFormatFlags
	{
		Default = 0,
		NoPadding = 1 << 0,
		NoPrefix = 1 << 1,
		Left = 1 << 2,
		Right = 1 << 3,
		HorizontalCenter = 1 << 4,
		VerticalCenter = 1 << 5,
		WordBreak = 1 << 6,
		SingleLine = 1 << 7,
		NoClipping = 1 << 8,
	}

	// GDI 文本测量/绘制：仅在执行期渲染路径触达；分析路径不会调用。返回缺省值即可编译。
	internal static class TextRenderer
	{
		public static System.Drawing.Size MeasureText(System.Drawing.Graphics dc, System.ReadOnlySpan<char> text, System.Drawing.Font font, System.Drawing.Size proposedSize, TextFormatFlags flags) => System.Drawing.Size.Empty;
		public static System.Drawing.Size MeasureText(System.Drawing.Graphics dc, string text, System.Drawing.Font font, System.Drawing.Size proposedSize, TextFormatFlags flags) => System.Drawing.Size.Empty;
		public static System.Drawing.Size MeasureText(string text, System.Drawing.Font font) => System.Drawing.Size.Empty;

		public static void DrawText(System.Drawing.Graphics dc, System.ReadOnlySpan<char> text, System.Drawing.Font font, System.Drawing.Point pt, System.Drawing.Color foreColor, TextFormatFlags flags) { }
		public static void DrawText(System.Drawing.Graphics dc, string text, System.Drawing.Font font, System.Drawing.Point pt, System.Drawing.Color foreColor, TextFormatFlags flags) { }
	}
}

namespace System.Media
{
	internal sealed class SystemSound
	{
		public void Play() { }
	}

	internal static class SystemSounds
	{
		public static SystemSound Hand { get; } = new SystemSound();
		public static SystemSound Asterisk { get; } = new SystemSound();
		public static SystemSound Beep { get; } = new SystemSound();
		public static SystemSound Exclamation { get; } = new SystemSound();
		public static SystemSound Question { get; } = new SystemSound();
	}
}
