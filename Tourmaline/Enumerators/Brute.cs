
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators 
{
	internal class Brute
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public string OutFile { get; set; }
		public string[] WordList { get; set; }
		public bool Debug { get; set; }
		public int Depth { get; set; }

		public Brute(BruteCommand.Settings settings, string[] wordlist)
		{
			URL = Functions.RemoveTrailingSlash(settings.URL);
			Threads = settings.Threads;
			OutFile = settings.OutFile;
			WordList = wordlist;
			Debug = settings.Debug;
			Depth = settings.Depth;
		}

		public async Task<List<string>> Enumerate(Action<string, HttpStatusCode, int> action)
		{
			List<string> found = new();
			Task[] tasks = new Task[Threads];
			HttpClient client = new();
			Queue<(string, int)> queue = new(WordList.Select(word => (word, 1)));

			object queueLock = new();

			Func<Task> thread = async () =>
			{
				while (true)
				{
					string url; 
					int depth;

					lock (queueLock)
					{
						if (queue.Count == 0) break;
						(url, depth) = queue.Dequeue();
					}

					HttpResponseMessage res = await client.GetAsync(Functions.ResolveURL(URL, url));

					if (res.IsSuccessStatusCode)
					{
						found.Add(url);
						action(Functions.ResolveURL(URL, url), res.StatusCode, queue.Count);

						if (depth < Depth)
						{
							lock (queueLock)
							{
								foreach (var word in WordList)
								{
									queue.Enqueue((Functions.ResolveURL(url, word), depth + 1));
								}
							}
						}
					}
				}
			};

			if (Debug) Console.WriteLine("Starting threads...");

			for (int i = 0; i < Threads; i++)
			{
				if (Debug) Console.WriteLine($"Starting thread {i}...");
				tasks[i] = thread();
			}

			await Task.WhenAll(tasks);

			client.Dispose();
			return found;
		}
	}
}