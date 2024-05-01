using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline.Scripts
{
	internal class EnumerateAgent
	{
		internal string URL { get; set; }
		internal string WordlistPath { get; set; }

		internal EnumerateAgent(string url, string wordlistPath) 
		{
			WordlistPath = wordlistPath;
			URL = url;
		}

		internal async Task Start(Action<Path>? next = null) 
		{
			SpiderAgent spider = new(URL);
			BruteAgent brute = new(URL, WordlistPath);

			List<Path> paths = [];
		}

		internal enum Stage
		{
			Brute, CMS, Spider
		}
	}
}
