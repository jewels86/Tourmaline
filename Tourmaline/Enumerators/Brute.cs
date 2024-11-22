
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators 
{
    internal class Brute 
    {
        public string URL { get; set; }
        public int Threads { get; set;}
        public string OutFile { get; set;}
        public string[] WordList { get; set; }
		public bool Debug { get; set; }

		public Brute(BruteCommand.Settings settings, string[] wordlist)
        {
            URL = Functions.RemoveTrailingSlash(settings.URL);
            Threads = settings.Threads;
            OutFile = settings.OutFile;
            WordList = wordlist;
			Debug = settings.Debug;
		}

        public async Task Enumerate()
        {
            List<string> found = new();
            Task[] tasks = new Task[Threads];
			HttpClient client = new();
            Queue<string> queue = new(WordList);

			object queueLock = new();

            Func<Task> thread = async () =>
            {
                while (queue.Count > 0)
                {
                    string url;
                    lock (queueLock) url = Functions.ResolveURL(URL, queue.Dequeue());

                    HttpResponseMessage res = await client.GetAsync(url);

                    if (res.IsSuccessStatusCode)
                    {
                        found.Add(url);
						AnsiConsole.MarkupLine($"[bold green]{url}[/] - {(int)res.StatusCode} {res.StatusCode.ToString()} ([bold]{queue.Count}[/] left)");
					}
                };
            };

			if (Debug) Console.WriteLine("Starting threads...");

			for (int i = 0; i < Threads; i++)
			{
                if (Debug) Console.WriteLine($"Starting thread {i}...");
				tasks[i] = thread();
			}

			await Task.WhenAll(tasks);

			client.Dispose();
		}
    }
}