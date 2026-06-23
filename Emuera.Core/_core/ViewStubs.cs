// 无头核心需要的 App/视图侧类型的极小桩：仅满足执行/渲染期代码的编译，分析路径不会调用其逻辑。
using System.Drawing;

namespace MinorShift.Emuera.UI.Game
{
	// 原 UI/Game/HotkeyState.cs（WinForms）已排除；引擎按签名引用。
	internal sealed class HotkeyState
	{
		public void HotkeyStateInit(nint size) { }
		public void HotkeyStateSet(nint index, nint value) { }
	}
}

namespace MinorShift.Emuera.Forms
{
	// 原 App 主窗口（WinForms Form）的桩。执行期命令（输入/CBG/鼠标）会访问其成员，
	// 分析不会执行这些路径。返回 null/缺省即可编译。
	// 原 UI/Framework/Forms/RikaiDialog.cs（WinForms 弹窗）的桩；Rikaichan 会 new 它。
	internal delegate void RikaiSendIndex(byte[] edictind);
	internal sealed class RikaiDialog
	{
		public RikaiDialog(byte[] edict, RikaiSendIndex rikaiSendIndex) { }
		public void Show() { }
	}

	internal sealed class MainWindow
	{
		public UI.Game.HotkeyState hotkeyState = new();
		public System.Windows.Forms.PictureBox MainPicBox => null;
		public System.Windows.Forms.RichTextBox TextBox => null;
		public string Text { get; set; }
		public void ApplyTextBoxChanges() { }
		public void ChangeTextBox(string str) { }
		public void ResetTextBoxPos() { }
		public void SetTextBoxPos(int xOffset, int yOffset, int width) { }
	}
}

namespace MinorShift.Emuera.UI
{
	// 原 UI/Dialog.cs（WinForms 弹窗）的桩。
	internal static class Dialog
	{
		public static void Show(string text) { }
		public static void Show(string title, string text) { }
		public static bool ShowPrompt(string title, string text) => false;
	}
}
