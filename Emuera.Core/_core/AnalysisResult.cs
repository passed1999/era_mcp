using System.Collections.Generic;

namespace MinorShift.Emuera.Analysis;

/// <summary>一次分析的结果。</summary>
public sealed class AnalysisResult
{
	public string ProjectRoot { get; init; }
	public string Target { get; init; }
	/// <summary>是否成功完成分析且无致命错误（level&gt;=3）。</summary>
	public bool Success { get; init; }
	public long ElapsedMs { get; init; }
	public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = new List<Diagnostic>();
	public int FileCount { get; init; }
}
