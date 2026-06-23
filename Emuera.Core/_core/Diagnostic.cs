using MinorShift.Emuera.Runtime.Utils;

namespace MinorShift.Emuera.Analysis;

/// <summary>
/// 一条结构化诊断（语法/解析告警或错误）。来源于引擎 ParserMediator 的 ParserWarning，
/// 经 HeadlessConsole.PrintWarning 采集。
/// </summary>
public sealed class Diagnostic
{
	public string Message { get; init; }
	public string File { get; init; }
	public int Line { get; init; }
	/// <summary>警告级别 0~3：0 轻微 / 1 可忽略行 / 2 不执行则无害 / 3 致命。</summary>
	public int Level { get; init; }

	/// <summary>level 到可读 severity 的映射。</summary>
	public string Severity => Level switch
	{
		>= 3 => "error",
		2 => "warning",
		1 => "info",
		_ => "hint",
	};

	internal static Diagnostic From(string message, ScriptPosition? pos, int level) => new()
	{
		Message = message,
		File = pos?.Filename,
		Line = pos?.LineNo ?? 0,
		Level = level,
	};
}
