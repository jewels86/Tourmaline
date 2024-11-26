using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline.Enumerators;

namespace Tourmaline.Commands
{
	internal class BruteCommand : AsyncCommand<BruteCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandArgument(1, "[WORDLIST]")]
			public string Wordlist { get; set; } = string.Empty;

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 12;

			[CommandOption("-o|--outfiles <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("-d|--depth <DEPTH>")]
			public int Depth { get; set; } = 1;

			[CommandOption("--outfile-bare")]
			public bool OutFileBare { get; set; } = false;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);
			settings.Wordlist = settings.Wordlist == string.Empty ? "https://raw.githubusercontent.com/3ndG4me/KaliLists/refs/heads/master/dirb/common.txt" : settings.Wordlist;

			string[] paths;

			if (settings.Wordlist.StartsWith("http://") || settings.Wordlist.StartsWith("https://"))
			{
				HttpClient client = new();
				HttpResponseMessage res = await client.GetAsync(settings.Wordlist);

				if (res.IsSuccessStatusCode == false)
				{
					client.Dispose();
					AnsiConsole.MarkupLine($"Wordlist [bold]{settings.Wordlist}[/] didn't return a successful status code.");
					return -1;
				}
				string content = await res.Content.ReadAsStringAsync();
				paths = content.Split("\n");
				client.Dispose();
			}
			else
			{
				paths = Functions.ReadFileAsLines(settings.Wordlist);
			}

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", "v2.0");
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "Brute Force");
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddEmptyRow();

			table.AddRow("Depth", settings.Depth.ToString());
			table.AddRow("Wordlist", settings.Wordlist);
			table.AddEmptyRow();

			table.AddRow("License", "GPL-3.0");
			table.AddRow("Author", "Gold Team");

			AnsiConsole.Write(table);

			Status status = AnsiConsole.Status();
			status.Spinner = Spinner.Known.Dots;
			try
			{
				await status.Start("Setting up...", async action =>
				{
					if (settings.Debug) Console.WriteLine("Preparing...");
					await Task.Delay(5000);
					if (!await Prepare(settings, action))
					{
						AnsiConsole.MarkupLine("[green]Tourmaline[/] is exiting (Error in preparation).");
						return;
					}
				});
			}
			catch
			{
				AnsiConsole.MarkupLine("[green]Tourmaline[/] is exiting (Error in preparation).");
				return -1;
			}

			if (settings.Debug) Console.WriteLine("Preparation complete.");

			Brute brute = new(settings, paths);
			await brute.Enumerate();

			return 0;
		}

		public async Task<bool> Prepare(Settings s, StatusContext ctx)
		{
			ctx.Status("Checking site accessiblity...");
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
