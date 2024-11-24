using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline
{
	internal static class Functions
	{
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

		internal static Dictionary<string, int> ParseCMSFile(string str)
		{
			string[] lines = str.Split('\n');
			Dictionary<string, int> output = new();

			foreach (string line in lines)
			{
				string[] parts = line.Split(": ");
				output.Add(parts[0], int.Parse(parts[1]));
			}

			return output;
		}
	}
}
