using EProcess = MinorShift.Emuera.GameProc.Process;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.Runtime.Config;
using MinorShift.Emuera.Runtime.Config.JSON;
using MinorShift.Emuera.Runtime.Script.Data;
using MinorShift.Emuera.Runtime.Script.Parser;
using MinorShift.Emuera.Runtime.Utils;
using MinorShift.Emuera.Runtime.Utils.EvilMask;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MinorShift.Emuera.Analysis;

/// <summary>
/// 无头 era 语法分析入口。给定项目根（含 csv/ 与 erb/）与可选目标文件/文件夹，
/// 跑引擎的加载+解析管线（不执行游戏循环、不写 Analysis.log），返回结构化诊断。
///
/// 引擎大量使用静态单例，故本方法以全局锁串行化，并在每次调用前彻底复位静态状态。
/// </summary>
public static class EmueraAnalyzer
{
	private static readonly object gate = new();
	private static bool encodingProviderRegistered;

	public static AnalysisResult AnalyzeProject(string projectRoot, string target = null)
	{
		if (string.IsNullOrWhiteSpace(projectRoot))
			throw new ArgumentException("projectRoot is required", nameof(projectRoot));

		lock (gate)
		{
			var sw = Stopwatch.StartNew();

			if (!encodingProviderRegistered)
			{
				// Shift-JIS 等代码页支持（与 App.Program.Main 一致）
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				encodingProviderRegistered = true;
			}

			ResetStatics();

			Program.SetDirPaths(projectRoot);
			if (!Directory.Exists(Program.CsvDir))
				return Failure(projectRoot, target, sw, $"csv 目录不存在: {Program.CsvDir}");
			if (!Directory.Exists(Program.ErbDir))
				return Failure(projectRoot, target, sw, $"erb 目录不存在: {Program.ErbDir}");

			// 配置 / 语言（解析期错误消息需要）
			ConfigData.Instance.LoadConfig();
			JSONConfig.Load();
			Lang.LoadLanguageFiles();
			Lang.SetLanguage();

			// 解析目标：空=整个 erb 目录；否则指定文件/文件夹（仍带项目根 ERH/CSV 上下文）
			Program.AnalysisMode = true;
			List<string> files = ResolveTargets(target);
			Program.AnalysisFiles = files;

			var console = new HeadlessConsole();
			ParserMediator.Initialize(console);

			Preload.Clear();
			Preload.Load(Program.ErbDir).GetAwaiter().GetResult();
			Preload.Load(Program.CsvDir).GetAwaiter().GetResult();

			GlobalStatic.Console = console;
			var process = new EProcess(console);
			GlobalStatic.Process = process;

			bool ok;
			try
			{
				ok = process.Initialize(null).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				sw.Stop();
				var diags = new List<Diagnostic>(console.Diagnostics)
				{
					new() { Message = $"分析过程中抛出异常: {ex.GetType().Name}: {ex.Message}", File = null, Line = 0, Level = 3 }
				};
				return new AnalysisResult
				{
					ProjectRoot = projectRoot,
					Target = target,
					Success = false,
					ElapsedMs = sw.ElapsedMilliseconds,
					Diagnostics = diags,
					FileCount = files.Count,
				};
			}
			finally
			{
				GlobalStatic.Process = null;
			}

			sw.Stop();
			var result = new List<Diagnostic>(console.Diagnostics);
			bool hasFatal = result.Any(d => d.Level >= 3);
			return new AnalysisResult
			{
				ProjectRoot = projectRoot,
				Target = target,
				Success = ok && !hasFatal,
				ElapsedMs = sw.ElapsedMilliseconds,
				Diagnostics = result,
				FileCount = files.Count,
			};
		}
	}

	private static List<string> ResolveTargets(string target)
	{
		string root = string.IsNullOrWhiteSpace(target) ? Program.ErbDir : target;
		if (File.Exists(root))
			return new List<string> { Path.GetFullPath(root) };
		if (Directory.Exists(root))
			return EnumerateErb(root);
		// 不存在则退回整个 erb 目录
		return EnumerateErb(Program.ErbDir);
	}

	private static List<string> EnumerateErb(string dir) =>
		Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
			.Where(f => f.EndsWith(".ERB", StringComparison.OrdinalIgnoreCase))
			.Select(Path.GetFullPath)
			.OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
			.ToList();

	private static void ResetStatics()
	{
		GlobalStatic.Reset();              // 清空 Process/Console/字典/tempDic 等
		ParserMediator.ClearWarningList();
		ParserMediator.RenameDic.Clear();
		LexicalAnalyzer.UseMacro = false;
		Program.AnalysisFiles = null;
		Program.AnalysisMode = false;
	}

	private static AnalysisResult Failure(string root, string target, Stopwatch sw, string message)
	{
		sw.Stop();
		return new AnalysisResult
		{
			ProjectRoot = root,
			Target = target,
			Success = false,
			ElapsedMs = sw.ElapsedMilliseconds,
			Diagnostics = new List<Diagnostic> { new() { Message = message, File = null, Line = 0, Level = 3 } },
			FileCount = 0,
		};
	}
}
