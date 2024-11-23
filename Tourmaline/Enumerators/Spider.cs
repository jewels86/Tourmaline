using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spectre.Console;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators
{
	internal class Spider
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public int Depth { get; set; }
		public string OutFile { get; set; }
		public Regex Regex { get; set; }
		public Regex IgnoreRegex { get; set; }
		public bool Debug { get; set; }
		public string[] Known { get; set; }

		public Regex JSPathFinder = new(@"['""]([a-zA-Z0-9\\\/\.?!#,=:;&% ]+[\\\/\.][a-zA-Z0-9\\\/\.?!#,=:;&% ]+)['""]");
		public Regex HTMLPathFinder = new(@"(src|href|action)=""([a-zA-Z0-9\\\/\.?!#,=:;&% ]+)""");
		public Regex OtherPathFinder = new(@"(?:\/[a-zA-Z0-9\-_]+|[a-zA-Z0-9\-_]+\.[a-zA-Z0-9\-_]+)");

		public Spider(SpiderCommand.Settings settings)
		{
			URL = settings.URL;
			Threads = settings.Threads;
			Depth = settings.Depth;
			OutFile = settings.OutFile;
			Regex = new(settings.Regex);
			IgnoreRegex = new(settings.IgnoreRegex);
			Debug = settings.Debug;
			Known = Functions.ResolveURLs(URL, settings.Known);
		}

		public async Task Enumerate()
		{
			Queue<string> queue = new(Known.Append(URL));
			List<string> found = new();
			Task[] tasks = new Task[Threads];
			HttpClient client = new();
			bool stop = false;

			object queueLock = new();

			Func<Task> thread = async () =>
			{
				while (!stop)
				{
					if (queue.Count == 0)
					{
						await Task.Delay(100);
						continue;
					}

					try
					{
						string url;
						lock (queueLock) url = queue.Dequeue();
						HttpResponseMessage res;

						try { res = await client.GetAsync(url); }
						catch { continue; }

						if (!res.IsSuccessStatusCode) continue;

						string content = await res.Content.ReadAsStringAsync();

						MatchCollection htmlMatches = HTMLPathFinder.Matches(content);
						MatchCollection jsMatches = JSPathFinder.Matches(content);
						MatchCollection otherMatches = OtherPathFinder.Matches(content);

						foreach (Match match in otherMatches.Concat(htmlMatches.Concat(jsMatches)))
						{
							if (res.IsSuccessStatusCode == false)
								continue;
							string u = ProcessURL(match.Groups[1].Value);
							if (found.Contains(u) || queue.Contains(u) || !u.Contains(Functions.TruncateURL(URL)) || u.Contains(' ')) continue;
							if (Regex.IsMatch(u) && !IgnoreRegex.IsMatch(u) && CheckDepth(u))
							{
								queue.Enqueue(u);
							}
						}

						AnsiConsole.MarkupLine($"[bold green]{url}[/] - {(int)res.StatusCode} {res.StatusCode.ToString()} ([bold]{queue.Count}[/] left)");
						found.Add(url);
						lock (queueLock) if (queue.Count == 0) stop = true;
					}
					catch (Exception e)
					{
						if (Debug) Console.WriteLine(e.Message);
						continue;
					}
				}
			};

			if (Debug) Console.WriteLine("Starting threads...");

			for (int i = 0; i < Threads; i++)
			{
				tasks[i] = thread();
			}

			await Task.WhenAll(tasks);

			client.Dispose();
		}

		public string ProcessURL(string url)
		{
			if (url.StartsWith("/") || !(url.StartsWith("https://") || url.StartsWith("http://")))
				url = Functions.ResolveURL(URL, url);

			int index = url.IndexOfAny(['#', '?']);
			if (index != -1)
				url = url.Substring(0, index);

			return url;
		}

		public bool CheckDepth(string path)
		{
			if (Depth == -1) return true;

			string[] parts = path.Split("/");
			return parts.Length <= Depth;
		}
	}
}
