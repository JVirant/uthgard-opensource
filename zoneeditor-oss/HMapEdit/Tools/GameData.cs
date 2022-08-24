using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Connect.Utils;

namespace HMapEdit.Tools
{
	/// <summary>
	/// Game resource class
	/// </summary>
	static class GameData
	{
		/// <summary>
		/// Path to the game
		/// </summary>
		private static string GamePath
		{
			get { return Path.GetFullPath(Program.Arguments.GameDirectory); }
		}

		private static string ExtractPath
		{
			get { return Path.Combine(Environment.CurrentDirectory, "extracted");  }
		}

		/// <summary>
		/// True if the specified file exists
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static bool Exists(string file)
		{
			Stream fs = null;
			try
			{
				fs = Open(file);
				return fs != null;
			}
			finally
			{
				if (fs != null)
					fs.Close();
			}
		}

		/// <summary>
		/// Opens the specified file readonly
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static Stream Open(string file)
		{
			if (GamePath == null)
				return null;
			if (File.Exists(file))
			{
				if (Program.CONFIG.ExtractLoadedAssets)
				{
					var dest = CombineRelative(ExtractPath, file.ToLower().Replace(GamePath.ToLower(), ""));
					if (!File.Exists(dest))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(dest));
						File.Copy(file, dest);
					}
				}
				return new FileStream(file, FileMode.Open, FileAccess.Read);
			}

			string curPath = GamePath;
			string[] components = file.ToLower().Split('\\');
			string nifComponent = null;

			foreach (var comp in components)
			{
				if (nifComponent != null)
				{
					nifComponent = Path.Combine(nifComponent, comp);
				}
				else
				{
					curPath = Path.Combine(curPath, comp);
					if (comp.EndsWith(".npk") || comp.EndsWith(".mpk"))
						nifComponent = "";
				}
			}

			// Normal file/NIF?
			if (!File.Exists(curPath))
				return null;
			if (nifComponent != null)
			{
				var mpk = TinyMPK.FromFile(curPath);
				var subfile = mpk.GetFile(nifComponent);
				if (subfile == null)
					return null;

				var ms = new MemoryStream(subfile.Data);
				if (Program.CONFIG.ExtractLoadedAssets)
				{
					var root = curPath.EndsWith(".npk") ? Path.GetDirectoryName(curPath) : curPath;
					var dest = CombineRelative(ExtractPath, root.ToLower().Replace(GamePath.ToLower(), ""), nifComponent);
					if (!File.Exists(dest))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(dest));
						File.WriteAllBytes(dest, ms.ToArray());
						ms.Position = 0;
					}
				}
				return ms;
			}
			if (Program.CONFIG.ExtractLoadedAssets)
			{
				var dest = CombineRelative(ExtractPath, curPath.ToLower().Replace(GamePath.ToLower(), ""));
				if (!File.Exists(dest))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(dest));
					File.Copy(curPath, dest);
				}
			}
			return new FileStream(curPath, FileMode.Open, FileAccess.Read);
		}

		/// <summary>
		/// Opens the first match
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		public static (Stream, string) OpenFirst(IEnumerable<string> files)
		{
			foreach (string file in files)
			{
				if (Exists(file))
					return (Open(file), file);
			}
			return (null, null);
		}

		public static string[] GetFiles(string path, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
		{
			Console.WriteLine($"path={path}, pattern={pattern}");
			if (Directory.Exists(path))
				return Directory.GetFiles(path, pattern);

			if (pattern.IndexOf('*') > 0 || pattern.LastIndexOf('*') > 0)
				throw new NotImplementedException("GetFiles are only implemented for patterns starting with *");

			var curPath = path.ToLower().StartsWith(GamePath.ToLower()) ? Uri.UnescapeDataString(new Uri(path).MakeRelativeUri(new Uri(GamePath)).ToString()) : GamePath;
			var components = path.ToLower().Split('\\');
			string nifComponent = null;
			foreach (var comp in components)
			{
				if (nifComponent != null)
					nifComponent = Path.Combine(nifComponent, comp);
				else
				{
					curPath = Path.Combine(curPath, comp);
					if (comp.EndsWith(".npk") || comp.EndsWith(".mpk"))
						nifComponent = "";
				}
			}
			if (!File.Exists(curPath) || nifComponent == null)
				return Array.Empty<string>();
			var mpk = TinyMPK.FromFile(curPath);
			var nifLower = nifComponent.ToLower();
			pattern = pattern.StartsWith("*") ? pattern.Substring(1).ToLower() : pattern.ToLower();
			var files = mpk.Files
				.Where(e =>
				{
					var name = e.Name.ToLower();
					if (!name.StartsWith(nifLower))
						return false;
					if (option == SearchOption.AllDirectories)
						return true;
					return !name.Substring(nifLower.Length).Contains('/') && !name.Substring(nifLower.Length).Contains('\\');
				})
				.Select(e => curPath + "/" + e.Name)
				.Where(file => file.ToLower().EndsWith(pattern))
				.ToArray();
			Console.WriteLine(string.Join(", ", files));
			Console.WriteLine($"All files in MPK {curPath}");
			Console.WriteLine(string.Join("\n", mpk.Files.Select(e => e.Name).ToArray()));
			return files;
		}

		/// <summary>
		/// Opens the specified nif file
		/// </summary>
		/// <param name="nif"></param>
		/// <returns></returns>
		public static (Stream, string) FindNIF(string nif)
		{
			var files = new[] { nif, Path.Combine(nif.ToLower().Replace(".nif", ".npk"), nif) };
			var result = OpenFirst(BuildPathPermutations(GamePaths, NIFFolders, files));

			// what's this naming? It seems to be linked to trees
			if (result.Item1 == null && nif.ToLower().EndsWith("-scl3.nif"))
				result = FindNIF(nif.Substring(0, nif.Length - 9) + ".nif");
			if (result.Item1 == null && nif.ToLower().EndsWith("cl3.nif"))
				result = FindNIF(nif.Substring(0, nif.Length - 7) + ".nif");
			if (result.Item1 == null && nif.ToLower().EndsWith("-scl5.nif"))
				result = FindNIF(nif.Substring(0, nif.Length - 9) + ".nif");
			if (result.Item1 == null && nif.ToLower().EndsWith("cl5.nif"))
				result = FindNIF(nif.Substring(0, nif.Length - 7) + ".nif");
			if (result.Item1 == null && !nif.ToLower().StartsWith("bb"))
				result = FindNIF("B" + nif);

			return result;
		}

		/// <summary>
		/// Finds a specific terraintex
		/// </summary>
		/// <param name="tex"></param>
		/// <returns></returns>
		public static (Stream, string) FindTerrainTex(string tex)
		{
			return OpenFirst(BuildPathPermutations(GamePaths, TerrainTexFolders, new[] { tex }));
		}

		public static (Stream, string) FindTex(string tex, params string[] paths)
		{
			if (paths.Length > 0)
				return OpenFirst(BuildPathPermutations(GamePaths, paths, new[] { tex }));
			return OpenFirst(BuildPathPermutations(GamePaths, NIFFolders, new[] { tex }));
		}

		/// <summary>
		/// Returns all available terraintex *.dds files
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> FindAllTerrainTex()
		{
			return
			  BuildPathPermutations(GamePaths, TerrainTexFolders)
			  .Select(d => Path.Combine(GamePath, d))
			  .Where(Directory.Exists)
			  .SelectMany(x => Directory.GetFiles(x, "*.dds"))
			  .Distinct();
		}

		/// <summary>
		/// Returns all world dirs (zones/, frontiers/, ...)
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<string> GamePaths
		{
			get
			{
				yield return "newtowns";
				yield return "frontiers";
				yield return "phousing";
				yield return "tutorial";
				yield return "";
			}
		}

		/// <summary>
		/// Generates all possible nif paths relative to the game dir
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<string> NIFFolders
		{
			get
			{
				yield return "nifs";
				yield return Path.Combine("zones", "nifs");
			}
		}

		/// <summary>
		/// Generates all possible tex paths relative to the game dir
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<string> TerrainTexFolders
		{
			get
			{
				yield return "terraintex";
				yield return Path.Combine("zones", "terraintex");
			}
		}

		#region Awful permutation methods
		private static IEnumerable<string> BuildPathPermutations(IEnumerable<string> a, IEnumerable<string> b)
		{
			foreach (var elemA in a)
			{
				foreach (var elemB in b)
				{
					yield return Path.Combine(elemA, elemB);
				}
			}
		}

		private static IEnumerable<string> BuildPathPermutations(IEnumerable<string> first, params IEnumerable<string>[] rest)
		{
			if (rest.Length == 0)
			{ /* only one set */
				return first;
			}
			else if (rest.Length == 1)
			{
				return BuildPathPermutations(first, rest[0]);
			}
			else
			{
				var parts = BuildPathPermutations(first, rest[0]);
				var remainder = rest.Skip(1).ToArray();
				return BuildPathPermutations(parts, remainder);
			}
		}
		#endregion

		private static string CombineRelative(string path, params string[] subpaths)
		{
			foreach (var subpath in subpaths.Where(sp => !String.IsNullOrWhiteSpace(sp)))
				path = Path.Combine(path, subpath[0] == '/' || subpath[0] == '\\' ? subpath.Substring(1) : subpath);
			return path;
		}
	}
}
