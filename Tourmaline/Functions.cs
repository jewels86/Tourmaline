using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline
{
	internal static class Functions
	{
		internal const string VERSION = "v2.3.0";
		
		internal static string ResolveURL(string baseUrl, params string[] parts)
		{
			StringBuilder sb;
			if (!(baseUrl.StartsWith("http://") || baseUrl.StartsWith("https://")))
			{
				baseUrl = "http://" + baseUrl;
			}
			sb = new StringBuilder(baseUrl);

			foreach (string part in parts)
			{
				if (part.StartsWith("/"))
				{
					sb.Append(part);
				}
				else
				{
					sb.Append("/" + part);
				}
			}

			return sb.ToString();
		}
		internal static string[] ResolveURLs(string baseUrl, string[] strings)
		{
			List<string> list = new();

			foreach (string str in strings)
			{
				list.Add(ResolveURL(baseUrl, str));
			}

			return list.ToArray();
		}

		internal static string TruncateURL(string url)
		{
			if (url.StartsWith("http://"))
			{
				url = url.Substring(7);
			}
			else if (url.StartsWith("https://"))
			{
				url = url.Substring(8);
			}
			int slashIndex = url.IndexOf('/');
			if (slashIndex != -1)
			{
				url = url.Substring(0, slashIndex);
			}
			return url;
		}
		internal static string RemoveTrailingSlash(string url)
		{
			if (url.EndsWith("/"))
			{
				url = url.Substring(0, url.Length - 1);
			}
			return url;
		}
		internal static string[] ReadFileAsLines(string path)
		{
			return System.IO.File.ReadAllLines(path);
		}

		internal static string[] RemoveFromArray(string[] array, string item)
		{
			List<string> list = new(array);
			list.Remove(item);
			return list.ToArray();
		}
		internal static string Escape(string str)
		{
			return str.Replace("[", "[[").Replace("]", "]]");
		}

		internal static Dictionary<string, int> ParseCMSFile(string[] lines)
		{
			Dictionary<string, int> output = new();

			foreach (string line in lines)
			{
				string[] parts = line.Split(": ");
				output.Add(parts[0], int.Parse(parts[1]));
			}

			return output;
		}

		internal async static Task<float> ScorePaths(string url, string name, HttpClient client, bool verbose = false)
		{
			string basePath = OperatingSystem.IsLinux() ? "/usr/local/share/tourmaline" : AppContext.BaseDirectory;
			string[] file = await File.ReadAllLinesAsync(Path.Combine(basePath, "wordlists", "cms-fuzzing", $"{name}.txt"));
			Dictionary<string, int> files = ParseCMSFile(file);

			int pathsScore = 0;

			foreach (var kvp in files)
			{
				string p = kvp.Key;
				int s = kvp.Value;

				HttpResponseMessage res = await client.GetAsync(ResolveURL(url, p));
				if (res.IsSuccessStatusCode)
				{
					if (verbose) Console.WriteLine($"[{name}] Found {p} with score {s}/10");
					pathsScore += s;
				}
			}

			return 100 * (pathsScore / files.Count);
		}
		internal async static Task<float> AnalyzeHTML(string name, HttpResponseMessage res, bool verbose = false)
		{
			string basePath = OperatingSystem.IsLinux() ? "/usr/local/share/tourmaline" : AppContext.BaseDirectory;
			string[] tags = ReadFileAsLines(Path.Combine(basePath, "wordlists", "html-analysis", $"{name}.txt"));
			string html = await res.Content.ReadAsStringAsync();
			int htmlScore = 0;

			foreach (string tag in tags)
			{
				if (html.Contains(tag))
				{
					htmlScore += 1;
					if (verbose) Console.WriteLine($"[{name}] Found HTML tag: '{tag}' for a total score of {htmlScore}");
				}
			}

			return htmlScore / tags.Length;
		}
		internal static string ResolveVersionsToRange(List<string> versions)
		{
			versions = versions.OrderBy(v => new Version(v)).ToList();
			Version min = new(versions.First());
			Version max = new(versions.Last());
			return $"{min}-{max}";
		}
	}
}
