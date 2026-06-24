using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MinorShift.Emuera.Runtime.Utils;

/// <summary>
/// 大文字小文字を区別するファイルシステム(Linux等)で、ハードコードされた
/// ファイル名(例 "ABL.CSV")が実ファイル(例 "Abl.csv")と一致せず読み込めない問題への対処。
///
/// Windows等の大小無視FSでは <see cref="File.Exists"/>/<see cref="Directory.GetFiles"/> が
/// 即座に当たるため、全メソッドは「まず通常APIを試し、見つかった/Windowsならそのまま返す」
/// fast-path構造になっており、Windowsの挙動は一切変えない。
/// </summary>
internal static class CaseInsensitivePath
{
	/// <summary>
	/// path が存在しない場合に限り、親ディレクトリを大小無視で走査して実在パスへ解決する。
	/// 末尾区切り(ディレクトリパス)も保持する。曖昧(同名で大小のみ異なる複数)な場合は
	/// 旧来の挙動を保つため元の path をそのまま返す。
	/// </summary>
	public static string Resolve(string path)
	{
		if (string.IsNullOrEmpty(path))
			return path;
		if (File.Exists(path) || Directory.Exists(path))
			return path;
		// 大小無視FSでは元々一致しているので、解決処理自体不要。
		if (OperatingSystem.IsWindows())
			return path;

		string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string name = Path.GetFileName(trimmed);
		if (string.IsNullOrEmpty(name))
			return path;
		// 親ディレクトリ自体も大小不一致の可能性があるため再帰的に解決する。
		// 空(相対の裸ファイル名)はカレントディレクトリ扱い。
		string dir = Path.GetDirectoryName(trimmed);
		dir = string.IsNullOrEmpty(dir) ? "." : Resolve(dir);
		if (!Directory.Exists(dir))
			return path;

		string match = null;
		foreach (var entry in Directory.EnumerateFileSystemEntries(dir))
		{
			if (!string.Equals(Path.GetFileName(entry), name, StringComparison.OrdinalIgnoreCase))
				continue;
			if (match != null)
				return path; // 曖昧: 静かに一方を選ばず旧挙動を維持
			match = entry;
		}
		if (match == null)
			return path;
		// 元が末尾区切り付き(ディレクトリ指定)なら区切りを復元
		if (path.Length > trimmed.Length)
			return match + Path.DirectorySeparatorChar;
		return match;
	}

	/// <summary>
	/// <see cref="Directory.GetFiles(string,string,SearchOption)"/> の大小無視版。
	/// まず通常APIを試し、結果があれば(=Windows、または正しい大小のLinux)そのまま返す。
	/// 空かつ非Windowsのときだけ、パターンを大小無視の正規表現に変換して再走査する。
	/// </summary>
	public static string[] GetFiles(string dir, string pattern, SearchOption option)
	{
		// Windowsは元々大小無視。存在しないディレクトリは従来同様に例外を投げさせる。
		if (OperatingSystem.IsWindows() || !Directory.Exists(dir))
			return Directory.GetFiles(dir, pattern, option);

		// 非Windowsでは常に大小無視で列挙する。
		// (native検索が空でないときだけ補完する方式だと、同一ディレクトリに
		//  大小違いの両方が在る場合 "A.ERB" のせいで "b.erb" を取り零す)
		Regex regex = GlobToRegex(pattern);
		return Directory.EnumerateFiles(dir, "*", option)
			.Where(f => regex.IsMatch(Path.GetFileName(f)))
			.ToArray();
	}

	private static Regex GlobToRegex(string pattern)
	{
		var sb = new StringBuilder("^");
		foreach (char c in pattern)
		{
			switch (c)
			{
				case '*': sb.Append(".*"); break;
				case '?': sb.Append('.'); break;
				default: sb.Append(Regex.Escape(c.ToString())); break;
			}
		}
		sb.Append('$');
		return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	}
}
