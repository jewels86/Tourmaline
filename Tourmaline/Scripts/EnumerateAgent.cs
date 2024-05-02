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

		internal async Task Start(Action<Path, double>? next = null, Action<string>? stage = null) 
		{
			stage?.Invoke(Stages[0]);
			SpiderAgent spider = new(URL);
			BruteAgent brute = new(URL, WordlistPath);
			CMSAgent cms = new(URL);
			HttpClient client = new();

			List<Path> paths = [];

			// Brute
			stage?.Invoke(Stages[1]);
			foreach (Path path in await brute.Start()) paths.Add(path);

			// CMS
			stage?.Invoke(Stages[2]);
			CMS type = await cms.Identify();
			
			if (type != CMS.Unknown)
			{
				stage?.Invoke(Stages[3]);
				foreach (Path path in await cms.Exploit(type)) paths.Add(path);
			}

			// Sitemap and robots.txt
			stage?.Invoke(Stages[3]);
			HttpResponseMessage msg = await client.GetAsync($"{URL}/sitemap.xml");
			if (msg.IsSuccessStatusCode)
			{

			}
			msg.Dispose();

			stage?.Invoke(Stages[4]);
			msg = await client.GetAsync($"{URL}/sitemap.xml");
			if (msg.IsSuccessStatusCode)
			{

			}
		}

		internal string[] Stages { get; private set; } = [
			"Starting",
			"Brute",
			"CMS - Guessing",
			"CMS - Exploitation",
			"Sitemap", 
			"robots.txt",
			"Google Dorking",
			"Spider - Paths",
			"Spider - Files"
		];
	}
}
