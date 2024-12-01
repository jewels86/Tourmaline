﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Tourmaline.Commands
{
	public class ScrapeCommand : AsyncCommand<ScrapeCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandArgument(1, "<REGEX>")]
			public required string Regex { get; set; }

			[CommandOption("-o|--outfiles <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("--outfile-bare")]
			public bool OutFileBare { get; set; } = false;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 12;

			[CommandOption("-f|--files")]
			public bool Files { get; set; } = false;

			[CommandOption("-p|--pages <PAGES-FILE>")]
			public string Pages { get; set; } = string.Empty;

			[CommandOption("--outfiles-dir <OUTFILES-DIR>")]
			public string OutFilesDir { get; set; } = string.Empty;

			public string[] Paths = []; 
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", "v2.0");
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "Scrape");
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddEmptyRow();

			table.AddRow("Files", settings.Files.ToString());
			table.AddRow("Pages", settings.Pages == string.Empty ? "No pages file specified." : settings.Pages);
			table.AddRow("Regex", Functions.Escape(settings.Regex));
			table.AddEmptyRow();

			table.AddRow("License", "GPL-3.0");
			table.AddRow("Author", "Gold Team");

			try
			{
				if (settings.Debug) Console.WriteLine("Preparing...");
				await Task.Delay(5000);
				if (!await Prepare(settings))
				{
					AnsiConsole.MarkupLine("[green]Tourmaline[/] is exiting (Error in preparation).");
					return -1;
				}
			}
			catch (Exception e)
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] {e.Message}");
				return 1;
			}



			return 0;
		}

		private async Task<bool> Prepare(Settings settings)
		{
			if (settings.URL == string.Empty)
			{
				AnsiConsole.MarkupLine("[red]Error:[/] No URL specified.");
				return false;
			}

			try
			{
				using HttpClient client = new();
				HttpResponseMessage response = await client.GetAsync(settings.URL);
				if (!response.IsSuccessStatusCode)
				{
					AnsiConsole.MarkupLine("[red]Error:[/] Failed to reach the URL.");
					return false;
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
				return false;
			}

			return true;
		}
	}
}