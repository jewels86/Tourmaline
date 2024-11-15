using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline
{
	internal static class Functions
	{
		internal static string ResolveURL(string url)
		{
			if (url.StartsWith("http://") || url.StartsWith("https://"))
			{
				return url;
			}
			else
			{
				return "http://" + url;
			}
		}
	}
}
