using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spectre.Console;

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

		internal async Task Start(Action<double>? progress = null, Action<string>? stage = null, Action? setIndeterminate = null) 
		{
			stage?.Invoke(Stages[0]);
			SpiderAgent spider = new(URL);
			BruteAgent brute = new(WordlistPath, URL);
			CMSAgent cms = new(URL);
			HttpClient client = new();

			List<string> queued = [];
			List<Path> paths = [];
			List<Path> tmplist = [];

			URL = Functions.ProcessURL(URL);
			progress?.Invoke(100.00);

			await Task.Delay(1000);

			// Brute - Works
			stage?.Invoke(Stages[1]);
			await Task.Delay(1000);

			brute.DevMode = true;

			int length = (await File.ReadAllLinesAsync(WordlistPath)).Length;
			await brute.Start(paths.Add, null, (inc) => progress?.Invoke(Math.Round((float)(100 / length), 2)));

			// CMS
			stage?.Invoke(Stages[2]);
			await Task.Delay(1000);

			CMS type = await cms.Identify();
			progress?.Invoke(20.00);

			if (type != CMS.Unknown)
			{
				stage?.Invoke(Stages[3]);
				tmplist = await cms.Exploit(type);

				foreach (Path path in tmplist) { paths.Add(path); progress?.Invoke(Math.Round((float)(80 / tmplist.Count), 2)); }
			} else
			{
				progress?.Invoke(80.00);
			}

			await Task.Delay(500);

			// Sitemap and robots.txt
			stage?.Invoke(Stages[4]);
			HttpResponseMessage msg = await client.GetAsync($"{URL}/sitemap.xml");
			progress?.Invoke(20.00);

			if (msg.IsSuccessStatusCode)
			{
				paths.Add(new()
				{
					Status = (int)msg.StatusCode,
					Type = msg.Content.Headers.ContentType?.MediaType ?? "unknown",
					URL = $"{URL}/sitemap.xml"
				});
				MatchCollection matches = Regex.Matches(await msg.Content.ReadAsStringAsync(), @"<loc>(.+)</loc>");
				foreach (Match match in matches)
				{ 
					queued.Add(URL = match.ToString().Replace("<loc>", "").Replace("</loc>", ""));
					progress?.Invoke(80/matches.Count);
				}
			} else { progress?.Invoke(100.00); }

			msg.Dispose();

			stage?.Invoke(Stages[5]);
			msg = await client.GetAsync($"{URL}/robot.txt");
			progress?.Invoke(20.00);

			if (msg.IsSuccessStatusCode)
			{
				paths.Add(new()
				{
					Status = (int)msg.StatusCode,
					Type = msg.Content.Headers.ContentType?.MediaType ?? "unknown",
					URL = $"{URL}/robots.txt"
				});
				string[] tmp = (await msg.Content.ReadAsStringAsync()).Split('\n');
				foreach (string path in tmp)
				{ 
					queued.Add(path);
					progress?.Invoke(0.8 / tmp.Length);
				}
			}

			stage?.Invoke(Stages[7]);
			spider.IgnoreRegex = new(@"(.+\.(js|css)|(\/.+\.))");
			spider.Found = paths.Select(path => path.URL).ToArray();
			setIndeterminate?.Invoke();

			await spider.Start(paths.Add);

			stage?.Invoke(Stages[8]);
			spider.IgnoreRegex = null;
			await spider.Start(paths.Add);

			foreach (Path path in paths)
			{
				AnsiConsole.WriteLine(path.ToString());
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
