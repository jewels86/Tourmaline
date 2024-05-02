using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Tourmaline.Scripts
{
	internal sealed class EnumerateCommand : AsyncCommand<EnumerateCommand.Settings>
	{
		internal class Settings : CommandSettings
		{
			[CommandArgument(0, "<url>")]
			public required string URL { get; set; }

			[CommandOption("-t")]
			[DefaultValue((ushort)4)]
			public required ushort Threads { get; set; }

			[CommandOption("-d")]
			[DefaultValue(false)]
			public required bool DevMode { get; set; }

			[CommandOption("-w")]
			[DefaultValue("Wordlists/wordlist.txt")]
			public required string WordlistPath { get; set; }

			[CommandOption("--output-bare")]
			[DefaultValue(false)]
			public required bool OutputBare { get; set; }
		}
		internal class GUI
		{
			internal Action<string> NextStage { get; set; } = (stage) => { };
			internal Action<double> IncrementProgress { get; set; } = (percent) => { };

			internal void Start(TaskCompletionSource tcs)
			{
				Progress progress = AnsiConsole.Progress();
				progress.StartAsync(async ctx =>
				{
					Stack<ProgressTask> tasks = new();
					NextStage = stage => { tasks.Push(ctx.AddTask(stage, new())); };
					IncrementProgress = percent => tasks.Peek().Increment(percent);

					await tcs.Task;
				});
			}
		}
		public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			Table table = new();
			table.AddColumns("[Blue][bold]Tourmaline[/][/]", "v1.xx");
			table.AddRow("License", "GPL v3")
				.AddRow("Creator(s)", "Jewels")
				.AddRow("Mode", "Enumerate")
				.AddRow("URL", settings.URL)
				.AddRow("Threads", settings.Threads.ToString())
				.AddRow("Dev mode", settings.DevMode ? "Enabled" : "Disabled");

			EnumerateAgent agent = new(settings.URL, settings.WordlistPath);
			GUI gui = new();
			TaskCompletionSource tcs = new();

			gui.Start(tcs);
			await agent.Start((path, progress) => { gui.IncrementProgress(progress); }, (stage) => { gui.NextStage(stage); });

			return 0;
		}
	}
}
