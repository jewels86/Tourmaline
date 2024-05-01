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
			public ushort Threads { get; set; }

			[CommandOption("-d")]
			[DefaultValue(false)]
			public bool DevMode { get; set; }

			[CommandOption("-w")]
			[DefaultValue("Wordlists/wordlist.txt")]
			public string? WordlistPath { get; set; }
		}
		public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			Table table = new();
			table.AddColumns("[Blue][bold]Tourmaline[/][/]", "");
			table.AddRow("License", "GPL v3")
				.AddRow("Creator(s)", "Jewels")
				.AddRow("Mode", "Enumerate")
				.AddRow("URL", settings.URL)
				.AddRow("Threads", settings.Threads.ToString())
				.AddRow("Dev mode", settings.DevMode ? "Enabled" : "Disabled");

			return 0;
		}
	}
}
