using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spectre.Console;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators
{
	internal class Scraper
	{
		public string URL { get; set; }
		public int Threads { get; set; }
		public int Depth { get; set; }
		public string OutFile { get; set; }
		public Regex Regex { get; set; }
		public bool Files { get; set; }
		public string[] Paths { get; set; }
		public string OutFilesDir { get; set; }

		public Scraper(ScrapeCommand.Settings settings)
		{
			URL = settings.URL;
			Threads = settings.Threads;
			OutFile = settings.OutFile;
			Regex = new(settings.Regex);
			Files = settings.Files;
			Paths = Functions.ResolveURLs(URL, settings.Paths);
			OutFilesDir = settings.OutFilesDir;
		}

		public async Task Enumerate(Action<string> action)
		{
			Task[] tasks = new Task[Threads];
			HttpClient client = new();



			client.Dispose();
			return;
		}
	}
}
