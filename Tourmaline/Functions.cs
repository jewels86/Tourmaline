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
		internal static string[] ReadFileAsLines(string path)
		{
			return System.IO.File.ReadAllLines(path);
		}
	}
}
