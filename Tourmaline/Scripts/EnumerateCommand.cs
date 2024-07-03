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
			internal Action SetIndeterminate { get; set; } = () => { };

			internal void Start(TaskCompletionSource tcs)
			{
				Progress progress = AnsiConsole.Progress()
					.Columns(
						new TaskDescriptionColumn(),
						new ProgressBarColumn(),
						new PercentageColumn(),
						new SpinnerColumn(Spinner.Known.Dots));
				progress.StartAsync(async ctx =>
				{
					Stack<ProgressTask> tasks = new();
					NextStage = stage => 
					{
						if (tasks.Count != 0) 
						{
							tasks.Peek().Increment(100 - tasks.Peek().Percentage);
							tasks.Peek().StopTask(); 
						}
						tasks.Push(ctx.AddTask(stage, new())); 
					};
					IncrementProgress = percent => tasks.Peek().Increment(percent);
					SetIndeterminate = () => tasks.Peek().IsIndeterminate = true;

					await tcs.Task;
				});
			}
		}
		public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			Table table = new();
			table.Width = 100;
			table.AddColumns("[Blue][bold]Tourmaline[/][/]", "v1");
			table.AddRow("License", "GPL v3")
				.AddRow("Creator(s)", "Jewels")
				.AddRow("Mode", "Enumerate")
				.AddRow("URL", settings.URL)
				.AddRow("Threads", settings.Threads.ToString())
				.AddRow("Dev mode", settings.DevMode ? "Enabled" : "Disabled");
			AnsiConsole.Write(table);

			EnumerateAgent agent = new(settings.URL, settings.WordlistPath);
			GUI gui = new();
			TaskCompletionSource tcs = new();

			gui.Start(tcs);
			await agent.Start((progress) => { gui.IncrementProgress(progress); }, (stage) => { gui.NextStage(stage); }, gui.SetIndeterminate);

			return 0;
		}
	}
}
