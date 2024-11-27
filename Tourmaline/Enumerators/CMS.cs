
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
		public string OutFile { get; set; }
		public bool Debug { get; set; }

		public CMS(CMSCommand.Settings settings)
		{
			URL = Functions.RemoveTrailingSlash(settings.URL);
			OutFile = settings.OutFile;
			Debug = settings.Debug;
		}

		public async Task Enumerate()
		{
			List<string> found = new();
			HttpClient client = new();
			
			if (Debug) Console.WriteLine("Starting CMS detection...");

			string wordpressScore = await CMSFuncs.Wordpress(URL, client, Debug);
			string joomlaScore = await CMSFuncs.Joomla(URL, client, Debug);
			string drupalScore = await CMSFuncs.Drupal(URL, client, Debug);

			AnsiConsole.MarkupLine("[green]CMS detection complete![/]");
			AnsiConsole.MarkupLine($"[green]Wordpress[/]: {wordpressScore}");
			AnsiConsole.MarkupLine($"[green]Joomla[/]: {joomlaScore}");
			AnsiConsole.MarkupLine($"[green]Drupal[/]: {drupalScore}");
		}
	}
}