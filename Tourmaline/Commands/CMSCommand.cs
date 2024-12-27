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
	internal class CMSCommand : AsyncCommand<CMSCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandOption("-o|--outfiles <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("--outfile-bare")]
			public bool OutFileBare { get; set; } = false;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", Functions.VERSION);
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "CMS Detection");
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
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

			CMS cms = new(settings);
			var scores = await cms.Enumerate();

			AnsiConsole.MarkupLine("[green]CMS detection complete![/]");
			foreach (var (score, val) in scores)
			{
				AnsiConsole.MarkupLine(val);
			}

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
