using MinorShift.Emuera.Analysis;
using System;
using System.Linq;

namespace MinorShift.Emuera.AnalyzerCli;

internal static class Program
{
	private static int Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.Error.WriteLine("usage: era-analyze <projectRoot> [targetFileOrDir]");
			return 2;
		}
		string root = args[0];
		string target = args.Length > 1 ? args[1] : null;

		AnalysisResult r = EmueraAnalyzer.AnalyzeProject(root, target);

		Console.WriteLine($"projectRoot : {r.ProjectRoot}");
		Console.WriteLine($"target      : {r.Target ?? "(full project)"}");
		Console.WriteLine($"files       : {r.FileCount}");
		Console.WriteLine($"success     : {r.Success}");
		Console.WriteLine($"elapsedMs   : {r.ElapsedMs}");
		Console.WriteLine($"diagnostics : {r.Diagnostics.Count}");
		foreach (var d in r.Diagnostics.OrderByDescending(d => d.Level))
			Console.WriteLine($"  [{d.Severity,-7}] {System.IO.Path.GetFileName(d.File)}:{d.Line}  {d.Message}");
		return r.Success ? 0 : 1;
	}
}
