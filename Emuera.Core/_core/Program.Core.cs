using System.Collections.Generic;
using System.IO;

namespace MinorShift.Emuera;

// 无头核心自带的 Program 数据静态量。
// 原 App 的 Program.cs 把这些路径/标志和 WinForms 入口耦合在一起，无法进 Core；
// 这里只保留 Runtime 层引用到的数据成员（不含任何 UI/Main 逻辑）。
// 注意：未来在 Windows 上构建 App 时，App 的 Program.cs 需改为复用此处而非各自声明。
static partial class Program
{
	public static string ExeDir { get; set; }
	public static string CsvDir { get; set; }
	public static string ErbDir { get; set; }
	public static string DebugDir { get; set; }
	public static string DatDir { get; set; }
	public static string ContentDir { get; set; }
	public static string SoundDir { get; set; }
	public static string FontDir { get; set; }
	public static string ExeName { get; set; }

	public static bool rebootFlag;
	public static bool AnalysisMode;
	public static List<string> AnalysisFiles;
	public static bool DebugMode { get; set; }

	/// <summary>
	/// 依据项目根目录设置各子目录路径（与 App.Program.SetDirPaths 保持一致）。
	/// </summary>
	public static void SetDirPaths(string exeDir)
	{
		ExeDir = Path.GetFullPath(new DirectoryInfo(exeDir).FullName + Path.DirectorySeparatorChar);
		CsvDir = Path.Combine(ExeDir, "csv") + Path.DirectorySeparatorChar;
		ErbDir = Path.Combine(ExeDir, "erb") + Path.DirectorySeparatorChar;
		DebugDir = Path.Combine(ExeDir, "debug") + Path.DirectorySeparatorChar;
		DatDir = Path.Combine(ExeDir, "dat") + Path.DirectorySeparatorChar;
		ContentDir = Path.Combine(ExeDir, "resources") + Path.DirectorySeparatorChar;
		SoundDir = Path.Combine(ExeDir, "sound") + Path.DirectorySeparatorChar;
		FontDir = Path.Combine(ExeDir, "font") + Path.DirectorySeparatorChar;
	}
}
