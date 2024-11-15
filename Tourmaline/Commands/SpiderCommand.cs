using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline;

namespace Tourmaline.Commands
{
	internal class SpiderCommand : AsyncCommand<SpiderCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 4;

			[CommandOption("-o|--outfiles <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("--outfile-bare")]
			public bool OutFileBare { get; set; } = false;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;

			[CommandOption("-f|--force")]
			public bool Force { get; set; } = false;

			[CommandOption("-d|--depth <DEPTH>")]
			public int Depth { get; set; } = -1;

			[CommandOption("-r| --regex <REGEX>")]
			public string Regex { get; set; } = string.Empty;

			[CommandOption("-i| --ignore-regex <IGNORE-REGEX>")]
			public string IgnoreRegex { get; set; } = string.Empty;

			[CommandOption("-k| --known <KNOWN>")]
			public string _Known { get; set; } = string.Empty;

			[CommandOption("--known-file <KNOWN-FILE>")]
			public string KnownFile { get; set; } = string.Empty;

			public string[] Known { get; set; } = [];
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);
			
			if (settings.KnownFile != string.Empty)
			{
				settings.Known = Functions.ReadFileAsLines(settings.KnownFile);
			}
			else if (settings._Known != string.Empty)
			{
				settings.Known = settings._Known.Split(',');
			}

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", "v2.0");
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddRow("Force", settings.Force.ToString());
			table.AddEmptyRow();

			table.AddRow("Depth", settings.Depth != -1 ? settings.Depth.ToString() : "No depth specified.");
			table.AddRow("Regex", settings.Regex == string.Empty ? "No regex specified." : settings.Regex);
			table.AddRow("Ignore Regex", settings.IgnoreRegex == string.Empty ? "No ignore regex specified." : settings.IgnoreRegex);
			table.AddRow("Known Paths", settings.Known.Length == 0 ? "No known paths specified." : string.Join(", ", settings.Known));
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
				if (settings.Debug) Console.WriteLine("Error");
				return -1; 
			}

			if (settings.Debug) Console.WriteLine("Preparation complete.");

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

			foreach (string known in s.Known)
			{
				string k = Functions.ResolveURL(s.URL, known);
				HttpResponseMessage res = await client.GetAsync(k);

			}

			client.Dispose();

			return true;
		}
	}
}
