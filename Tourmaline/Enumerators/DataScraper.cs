using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourmaline.Commands;
using System.Net;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Tourmaline.Enumerators
{
	internal class DataScraper
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public string OutFile { get; set; }
		public bool Debug { get; set; }
		public string[] Paths { get; set; }
		public Regex Regex { get; set; }

		public DataScraper(DataScraperCommand.Settings settings)
		{
			URL = settings.URL;
			Threads = settings.Threads;
			OutFile = settings.OutFile;
			Debug = settings.Debug;
			Paths = Functions.ReadFileAsLines(settings.Paths);
			Regex = new(settings.Regex);
		}

		public async Task<List<string>> Enumerate(Action<string, HttpStatusCode, int> action)
		{
			Queue<string> queue = new(Functions.ResolveURLs(URL, Paths));
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

						if (Debug) Console.WriteLine($"Downloading {url}");

						try { res = await client.GetAsync(url); }
						catch { continue; }

						if (!res.IsSuccessStatusCode) continue;

						string content = await res.Content.ReadAsStringAsync();
						string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (string line in lines)
						{
							if (Regex.IsMatch(line))
							{
								found.Add(line);
							}
						}

						action(url, res.StatusCode, queue.Count);

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

			if (!string.IsNullOrEmpty(OutFile))
			{
				await File.WriteAllLinesAsync(OutFile, found);
			}

			client.Dispose();
			return found;
		}
	}
}
