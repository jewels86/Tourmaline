using System.ComponentModel;
using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Tourmaline.Scripts
{
	public sealed class SpiderCommand : AsyncCommand<SpiderCommand.Settings>
	{
		public sealed class Settings : CommandSettings
		{
			[Description("URL to use the spider on")]
			[CommandArgument(0, "<url>")]
			public required string URL { get; init; }

			[Description("The max amount of paths to search")]
			[CommandOption("-m|--max-paths")]
			public ulong? MaxPaths { get; init; }

			[Description("Regex all paths must pass.")]
			[CommandOption("-r")]
			public string? Regex { get; set; }

			[Description("Regex for ignoring paths.")]
			[CommandOption("-i")]
			public string? IgnoreRegex { get; set; }

			[Description("Outfile.")]
			[CommandOption("-o")]
			public string? OutfilePath { get; init; }

			[Description("Initiates dev mode")]
			[CommandOption("-d|--dev-mode")]
			[DefaultValue(false)]
			public bool DevMode { get; init; }

			[CommandOption("-t")]
			[DefaultValue((ushort)4)]
			public ushort Threads { get; init; }

			[CommandOption("-s")]
			public ushort? StrayValue { get; init; }

			[Description("Makes the outfile bare so it only contains paths.")]
			[CommandOption("--outfile-bare")]
			[DefaultValue(false)]
			public bool? OutfileBare { get; init; }

			[Description("Makes the output bare so it only shows paths.")]
			[CommandOption("--output-bare")]
			[DefaultValue(false)]
			public bool OutputBare { get; init; }
		}

		public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			if (!settings.OutputBare)
			{
				AnsiConsole.WriteLine("Welcome to");
				FigletText figlet = new FigletText("Tourmaline!")
					.Color(Color.Blue);
				AnsiConsole.Write(figlet);
			}
			Table table = new();
			table.AddColumns("[Blue]Tourmaline[/]", "");
			table.AddRow("License", "GPL v3")
				.AddRow("Creator(s)", "Jewels")
				.AddRow("Mode", "Spider")
				.AddRow("URL", settings.URL)
				.AddRow("Threads", settings.Threads.ToString())
				.AddRow("Dev mode", settings.DevMode ? "Enabled":"Disabled");
			if (settings.MaxPaths is not null) table.AddRow("Max Paths", ((ulong)settings.MaxPaths).ToString());
			if (settings.Regex is not null) table.AddRow("Regex", settings.Regex.Replace("[", "[[").Replace("]", "]]"));
			if (settings.IgnoreRegex is not null) table.AddRow("Ignore Regex", settings.IgnoreRegex.Replace("[", "[[").Replace("]", "]]"));
			if (settings.OutputBare != false) table.AddRow("Bare Output", "True");
			if (settings.OutfileBare != false) table.AddRow("Bare Outfile", "True");
			if (settings.OutfilePath is not null) table.AddRow("Outfile", settings.OutfilePath);
			table.Width(100);
			if (!settings.OutputBare) AnsiConsole.Write(table);

			Status status = AnsiConsole.Status();

			GUI gui = new();
			SpiderAgent agent = new(settings.URL);


			agent.MaxPaths = settings.MaxPaths ?? null;
			agent.DevMode = settings.DevMode;
			agent.BareOutfile = settings.OutfileBare ?? false;
			agent.Regex = settings.Regex is not null ? new(settings.Regex) : null;
			agent.IgnoreRegex = settings.IgnoreRegex is not null ? new(settings.IgnoreRegex) : null;
			agent.Threads = settings.Threads;
			agent.StrayValue = settings.StrayValue ?? null;

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
				await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()));
			}

			gui.TaskCompletionSource.TrySetResult();

			return 0;
		}
	}
}
