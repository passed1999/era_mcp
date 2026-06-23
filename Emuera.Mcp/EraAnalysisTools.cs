using MinorShift.Emuera.Analysis;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MinorShift.Emuera.Mcp;

[McpServerToolType]
public sealed class EraAnalysisTools
{
	private static readonly JsonSerializerOptions JsonOpts = new()
	{
		WriteIndented = true,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 保留日文等原文
	};

	[McpServerTool(Name = "analyze_era_project")]
	[Description("对一个 era 项目运行 Emuera 的加载+语法解析，返回结构化诊断（JSON）。" +
		"会加载项目根下的 csv/ 与 erb/（ERH 头文件）作为上下文；可只解析指定文件/文件夹。" +
		"不执行游戏逻辑，仅做语法/结构/标识符/参数等静态检查。")]
	public static string AnalyzeEraProject(
		[Description("项目根目录的绝对路径，需包含 csv/ 与 erb/ 子目录。")] string projectRoot,
		[Description("可选：要解析的单个 .ERB 文件或子文件夹（绝对路径）。留空则解析整个 erb/ 目录。")] string? target = null)
	{
		// EmueraAnalyzer 内部已用全局锁串行化并复位静态状态，故并发调用安全（依次执行）。
		AnalysisResult r = EmueraAnalyzer.AnalyzeProject(projectRoot, target);

		var payload = new
		{
			projectRoot = r.ProjectRoot,
			target = r.Target,
			success = r.Success,
			fileCount = r.FileCount,
			elapsedMs = r.ElapsedMs,
			diagnosticCount = r.Diagnostics.Count,
			diagnostics = r.Diagnostics
				.OrderByDescending(d => d.Level)
				.Select(d => new
				{
					severity = d.Severity,
					level = d.Level,
					file = d.File,
					line = d.Line,
					message = d.Message,
				}),
		};
		return JsonSerializer.Serialize(payload, JsonOpts);
	}
}
