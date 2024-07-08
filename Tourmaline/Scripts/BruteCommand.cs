using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using System.Reflection;

namespace Tourmaline.Scripts
{
	public sealed class BruteCommand : AsyncCommand<BruteCommand.Settings>
	{
		public sealed class Settings : CommandSettings
		{
			[Description("The url to operate on.")]
			[CommandArgument(0, "<url>")]
			public required string URL { get; init; }

			[Description("The path to the wordlist.")]
			[CommandArgument(1, "[wordlistPath]")]
			public required string? WordlistPath { get; set; }

			[Description("Initiates dev mode.")]
			[CommandOption("-d")]
			[DefaultValue(false)]
			public bool DevMode { get; init; }

			[CommandOption("-t")]
			[DefaultValue((ushort)4)]
			public ushort Threads { get; init; }

			[Description("Path to the outfile.")]
			[CommandOption("-o|--outfile-path")]
			public string? OutfilePath { get; init; }

			[Description("Makes the outfile bare so it only contains paths.")]
			[CommandOption("--outfile-bare")]
			[DefaultValue(false)]
			public bool OutfileBare { get; init; }

			[Description("Makes the output bare so it only shows paths.")]
			[CommandOption("--output-bare")]
			[DefaultValue(false)]
			public bool OutputBare { get; init; }
		}

		internal class GUI
		{
			internal Table Table = new();
			internal Action<string, string, string> AddRow = (a, b, c) => { };
			internal Action<int> SetProgressCap = (a) => { };
			internal Action<double> IncrementProgress = (a) => { };
			internal TaskCompletionSource TaskCompletionSource = new();

			internal void Start()
			{
				Table.AddColumns("URL", "Type", "Status")
					.Alignment(Justify.Left)
					.Width = 300;

				AnsiConsole.Live(Table).StartAsync(async (ctx) =>
				{
					AddRow = (a, b, c) => { Table.AddRow($"[blue]{a}[/]", b, c); ctx.UpdateTarget(Table); };
					await TaskCompletionSource.Task;
				});

				Progress progress = AnsiConsole.Progress();
				progress.Columns(new ProgressBarColumn(), new PercentageColumn());
				progress.StartAsync(async (ctx) =>
				{
					ProgressTask task = ctx.AddTask("Enumerating...");
					SetProgressCap = (a) => task.MaxValue = a;
					IncrementProgress = (a) => task.Increment(a);
					await TaskCompletionSource.Task;
				});
			}
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			bool wasNull = settings.WordlistPath == null;
			if (settings.WordlistPath == null) settings.WordlistPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? "") + "/Wordlists/wordlist.txt";
			if (settings.WordlistPath == "/Wordlists/wordlist.txt" && wasNull) { AnsiConsole.WriteLine("No wordlist given."); return -1; }

			Table table = new();
			table.AddColumns("[Blue]Tourmaline[/]", "");
			table.AddRow("License", "GPL v3")
				.AddRow("Creator(s)", "Jewels")
				.AddRow("Mode", "Brute")
				.AddRow("URL", settings.URL)
				.AddRow("Wordlist", settings.WordlistPath.Split("/")[^1])
				.AddRow("Threads", settings.Threads.ToString());
			if (settings.DevMode != false) table.AddRow("Dev Mode", "Enabled");
			if (settings.OutfilePath != null) table.AddRow("Outfile", settings.OutfilePath);
			table.Width(100);
			if (!settings.OutputBare) AnsiConsole.Write(table);

			BruteAgent agent = new(settings.WordlistPath, settings.URL);
			BruteCommand.GUI gui = new();

			agent.DevMode = settings.DevMode;
			agent.BareOutfile = settings.OutfileBare;
			agent.Threads = settings.Threads;
			if (settings.OutfilePath != null) agent.OutfilePath = settings.OutfilePath;

			if (settings.OutputBare)
			{
				await agent.Start((path) => Console.WriteLine(path.URL));
			}
			else
			{
				string start = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("Ready?")
						.AddChoices("Yes", "No")
				);

				if (start == "No") return -1;

				gui.Start();
				await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()), (max) => gui.SetProgressCap(max), (inc) => gui.IncrementProgress(inc));
			}
			
			gui.TaskCompletionSource.TrySetResult();

			return 0;
		}
	}
}
