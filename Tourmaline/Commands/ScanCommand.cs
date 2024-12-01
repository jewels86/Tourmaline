using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline.Enumerators;
using static System.Net.WebRequestMethods;

namespace Tourmaline.Commands
{
	internal class ScanCommand : AsyncCommand<ScanCommand.Settings>
	{
		public class Settings : CommandSettings
		{
			[CommandArgument(0, "<URL>")]
			public required string URL { get; set; }

			[CommandOption("-o|--outfiles <OUTFILE>")]
			public string OutFile { get; set; } = string.Empty;

			[CommandOption("-t|--threads <THREADS>")]
			public int Threads { get; set; } = 12;

			[CommandOption("--outfile-bare")]
			public bool OutFileBare { get; set; } = false;

			[CommandOption("--debug")]
			public bool Debug { get; set; } = false;

			[CommandOption("-w|--wordlist <WORDLIST>")]
			public string Wordlist { get; set; } = string.Empty;
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			settings.URL = Functions.ResolveURL(settings.URL);

			Table table = new();
			table.AddColumns("[green]Tourmaline[/]", "v2.0");
			table.Width = 200;

			table.AddRow("URL", settings.URL);
			table.AddRow("Mode", "Scan");
			table.AddRow("Threads", settings.Threads.ToString());
			table.AddRow("Outfile", settings.OutFile == string.Empty ? "No outfile specified." : settings.OutFile);
			table.AddRow("Debug Mode", settings.Debug.ToString());
			table.AddEmptyRow();

			table.AddRow("License", "GPL-3.0");
			table.AddRow("Author", "Gold Team");

			AnsiConsole.Write(table);

			Status status = AnsiConsole.Status();
			status.Spinner = Spinner.Known.Dots;
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

			AnsiConsole.MarkupLine("[green]Scan starting...[/]");

			Status scanStatus = AnsiConsole.Status();
			scanStatus.Spinner = Spinner.Known.Dots;
			List<string> found = new();
			await scanStatus.Start("Starting tasks...", async action =>
			{
				List<(float score, string cmsName)> cmsResults = new();

				List<Task> tasks = new List<Task>();
				CMS cms = new(new CMSCommand.Settings
				{
					URL = settings.URL,
					OutFile = settings.OutFile,
					Debug = settings.Debug
				});
				tasks.Add(Task.Run(async () => cmsResults = await cms.Enumerate()));
				action.Status("CMS detection started...");

				settings.Wordlist = settings.Wordlist == string.Empty ? "https://raw.githubusercontent.com/3ndG4me/KaliLists/refs/heads/master/dirb/common.txt" : settings.Wordlist;
				string[] paths;

				if (settings.Wordlist.StartsWith("http://") || settings.Wordlist.StartsWith("https://"))
				{
					HttpClient client = new();
					HttpResponseMessage res = await client.GetAsync(settings.Wordlist);

					string content = await res.Content.ReadAsStringAsync();
					paths = content.Split("\n");
					client.Dispose();
				}
				else
				{
					paths = Functions.ReadFileAsLines(settings.Wordlist);
				}
				Brute brute = new(new BruteCommand.Settings
				{
					URL = settings.URL,
					OutFile = settings.OutFile,
					Threads = settings.Threads,
					Debug = settings.Debug,
					Wordlist = settings.Wordlist
				}, paths);
				tasks.Add(Task.Run(async () => found.AddRange(await brute.Enumerate((a, b, c) => { }))));
				action.Status("Brute forcing started...");

				await tasks[0];
				string cmsResult = cmsResults[0].cmsName.Split(":")[0].ToLower();
				if (!(cmsResults[0].score == 0))
				{
					Dictionary<string, string> cmsToWordlist = new()
					{

					};

					HttpClient client = new();
					HttpResponseMessage res = await client.GetAsync(cmsToWordlist[cmsResult]);
					string content = await res.Content.ReadAsStringAsync();
					paths = content.Split("\n");

					Brute brute2 = new(new BruteCommand.Settings
					{
						URL = settings.URL,
						OutFile = settings.OutFile,
						Threads = settings.Threads,
						Debug = settings.Debug,
						Wordlist = cmsToWordlist[cmsResult]
					}, paths);	
					tasks.Add(Task.Run(async () => found.AddRange(await brute2.Enumerate((a, b, c) => { }))));
				}

				Task.WaitAll(tasks.ToArray());

				Spider spider = new(new SpiderCommand.Settings
				{
					URL = settings.URL,
					OutFile = settings.OutFile,
					Threads = settings.Threads,
					Debug = settings.Debug,
					Known = found.ToArray()
				});

				action.Status("Spider started...");
				await spider.Enumerate((a, b, c) => { });
			});

			AnsiConsole.MarkupLine("[green]Scan complete![/]");
			foreach (string url in found)
			{
				AnsiConsole.Markup($"[green]{url}[/], ");
			}

			return 0;
		}

		private async Task<bool> Prepare(Settings settings)
		{
			return true;
		}
	}
}
