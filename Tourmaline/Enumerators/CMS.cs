
using Spectre.Console;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators
{
	internal class CMS
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public string OutFile { get; set; }
		public bool Debug { get; set; }

		public Dictionary<string, string> CMSToList { get; set; } = new()
		{
			{ "joolma", Path.Combine(AppContext.BaseDirectory, "wordlists", "joolma.txt") }
		};

		public CMS(CMSCommand.Settings settings, string[] wordlist)
		{
			URL = Functions.RemoveTrailingSlash(settings.URL);
			Threads = settings.Threads;
			OutFile = settings.OutFile;
			Debug = settings.Debug;
		}

		public async Task Enumerate()
		{
			List<string> found = new();
			Task[] tasks = new Task[Threads];
			HttpClient client = new();
			ConcurrentQueue<KeyValuePair<string, string>> queue = new();


		}
	}
}