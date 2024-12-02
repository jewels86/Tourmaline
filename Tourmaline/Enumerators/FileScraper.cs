using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourmaline.Commands;
using System.Net;

namespace Tourmaline.Enumerators
{
	internal class FileScraper
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public string OutFileDir { get; set; }
		public bool Debug { get; set; }
		public string[] Paths { get; set; }

		public FileScraper(FileScraperCommand.Settings settings)
		{
			URL = settings.URL;
			Threads = settings.Threads;
			OutFileDir = settings.OutFileDir;
			Debug = settings.Debug;
			Paths = Functions.ReadFileAsLines(settings.Paths);
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

						try { res = await client.GetAsync(url); }
						catch { continue; }

						if (!res.IsSuccessStatusCode) continue;

						string fileName = Path.GetFileName(url);
						string filePath = Path.Combine(OutFileDir, fileName);

						using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
						{
							await res.Content.CopyToAsync(fileStream);
						}

						action(url, res.StatusCode, queue.Count);
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
			return found;
		}
	}
}
