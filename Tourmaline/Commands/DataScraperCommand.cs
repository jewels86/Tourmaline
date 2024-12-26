﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline.Enumerators;

namespace Tourmaline.Commands
{
	internal class DataScraperCommand : AsyncCommand<DataScraperCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandOption("-o|--outfile")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 12;

			[CommandArgument(1, "<PATHS-FILE>")]
			public required string Paths { get; set; }

			[CommandArgument(1, "<REGEX>")]
			public required string Regex { get; set; }
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", Functions.VERSION);
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "Data Scraper");
			table.AddRow("Outfile Directory", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddEmptyRow();

			table.AddRow("Paths File", settings.Paths);
			table.AddEmptyRow();

			table.AddRow("License", "GPL-3.0");
			table.AddRow("Author", "Jewels");

			AnsiConsole.Write(table);

			try
			{
				if (settings.Debug) Console.WriteLine("Preparing...");
				if (!await Prepare(settings))
				{
					AnsiConsole.MarkupLine("[green]Tourmaline[/] is exiting (Error in preparation).");
					return -1;
				}
			}
			catch
			{
				AnsiConsole.MarkupLine("[green]Tourmaline[/] is exiting (Error in preparation).");
				return -1;
			}

			if (settings.Debug) Console.WriteLine("Preparation complete.");

			DataScraper ds = new(settings);

			await AnsiConsole.Status().StartAsync("Scraping data...", async (ctx) =>
			{
				await ds.Enumerate((url, status, count) =>
				{
					ctx.Status($"Scraping {url} ({status}) - {count} URLs left.");
				});
			});

			AnsiConsole.MarkupLine("[green]Data scraping complete![/]");

			return 0;
		}

		public async Task<bool> Prepare(Settings s)
		{
			HttpClient client = new();

			try
			{
				HttpResponseMessage res = await client.GetAsync(s.URL);

				if (res.IsSuccessStatusCode == false)
				{
					AnsiConsole.MarkupLine($"[bold]{s.URL}[/] didn't return a successful status code.");
					return false;
				}
			}
			catch
			{
				AnsiConsole.MarkupLine($"[bold]{s.URL}[/] is not accessible.");
				return false;
			}

			client.Dispose();

			return true;
		}
	}
}
