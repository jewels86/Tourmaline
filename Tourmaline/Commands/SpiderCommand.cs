﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using static Tourmaline.Functions;
using Tourmaline.Enumerators;
using System.Net;

namespace Tourmaline.Commands
{
	internal class SpiderCommand : AsyncCommand<SpiderCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 12;

			[CommandOption("-o|--outfile <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;

			[CommandOption("-d|--depth <DEPTH>")]
			public int Depth { get; set; } = -1;

			[CommandOption("-r|--regex <REGEX>")]
			public string Regex { get; set; } = @".*";

			[CommandOption("-i|--ignore-regex <IGNORE_REGEX>")]
			public string IgnoreRegex { get; set; } = "(?!)";

			[CommandOption("--force-regex")]
			public bool ForceRegex { get; set; } = false;

			[CommandOption("--force-ignore")]
			public bool ForceIgnore { get; set; } = false;

			[CommandOption("-k|--known <KNOWN>")]
			public string _Known { get; set; } = string.Empty;

			[CommandOption("--known-file <KNOWN_FILE>")]
			public string KnownFile { get; set; } = string.Empty;

			[CommandOption("-l|--limit <LIMIT>")]
			public int Limit { get; set; } = -1;

			[CommandOption("--force-limit")]
			public bool ForceLimit { get; set; } = false;

			public string[] Known { get; set; } = [];
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = ResolveURL(settings.URL);
			
			if (settings.KnownFile != string.Empty)
			{
				settings.Known = ReadFileAsLines(settings.KnownFile);
			}
			else if (settings._Known != string.Empty)
			{
				settings.Known = settings._Known.Split(',');
			}
			else if (settings.Known.Length == 0)
			{
				settings.Known = ["sitemap.xml", "robots.txt"];
			}

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", Functions.VERSION);
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "Spider");
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddEmptyRow();

			table.AddRow("Depth", settings.Depth != -1 ? settings.Depth.ToString() : "No depth specified.");
			table.AddRow("Regex", settings.Regex == string.Empty ? "No regex specified." :  Escape(settings.Regex));
			table.AddRow("Ignore Regex", settings.IgnoreRegex == string.Empty ? "No ignore regex specified." : Escape(settings.IgnoreRegex));
			table.AddRow("Known Paths", settings.Known.Length == 0 ? "No known paths specified." : string.Join(", ", settings.Known));
			table.AddEmptyRow();

			table.AddRow("License", "GPL-3.0");
			table.AddRow("Author", "Jewels");

			AnsiConsole.Write(table);

			Status status = AnsiConsole.Status();
			status.Spinner = Spinner.Known.Dots;
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

			Spider spider = new(settings);
			Action<string, HttpStatusCode, int> action = (a, b, c) => AnsiConsole.MarkupLine($"[bold green]{a}[/] - {(int)b} {b.ToString()} ([bold]{c}[/] left)");
			await spider.Enumerate(action);

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
					AnsiConsole.MarkupLine($"[bold]{s.URL}[/] didn't return a successful status code.\n[green]Tip[/]: run tourmaline with the [bold]-f[/] flag to run anyway.");
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
