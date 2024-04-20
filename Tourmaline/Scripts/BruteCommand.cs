using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;

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
            [CommandArgument(1, "<wordlistPath>")]
            public required string WordlistPath { get; init; }

            [Description("Initiates dev mode.")]
            [CommandOption("-d")]
            public bool? DevMode { get; init; }

            [CommandOption("-t")]
            public short? Threads { get; init; }

            [Description("Path to the outfile.")]
            [CommandOption("-o|--outfile-path")]
            public string? OutfilePath { get; init; }

            [Description("Makes the outfile bare so it only contains paths.")]
            [CommandOption("--outfile-bare")]
            public bool? OutfileBare { get; init; }

            [Description("Makes the output bare so it only shows paths.")]
            [CommandOption("--output-bare")]
            public bool? OutputBare { get; init; }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            Table table = new();
            table.AddColumns("[Blue]Tourmaline[/]", "");
            table.AddRow("License", "GPL v3")
                .AddRow("Creator(s)", "Jewels")
                .AddRow("Mode", "Brute")
                .AddRow("URL", settings.URL)
                .AddRow("Wordlist", settings.WordlistPath.Split("/")[^1])
                .AddRow("Threads", settings.Threads?.ToString() ?? "4")
                .AddRow("Dev mode?", settings.DevMode?.ToString() ?? false.ToString())
                .AddRow("Outfile?", settings.OutfilePath ?? false.ToString());
            table.Width(100);
            AnsiConsole.Write(table);

            Status status = AnsiConsole.Status();

            BruteAgent agent = new(settings.WordlistPath, settings.URL);
            GUI gui = new();

            await status.StartAsync("Starting...", async ctx =>
            {
                await Task.Delay(200);

                ctx.Status = "Configuring agent...";
                if (settings.DevMode != null && settings.DevMode == true) agent.DevMode = true;
                if (settings.OutfilePath != null) agent.OutfilePath = settings.OutfilePath;
                if (settings.OutfileBare != null && settings.OutfileBare == true) agent.BareOutfile = true;
                if (settings.Threads is not null) agent.Threads = (short)settings.Threads;
                await Task.Delay(1000);

                ctx.Status = "Finished";
                await Task.Delay(200);
            });

            string start = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Ready?")
                    .AddChoices("Yes", "No")
            );

            if (start == "No") return -1;
            if (settings.OutputBare != null && settings.OutputBare == true)
            {
                await agent.Start((path) => Console.WriteLine(path.URL));
            } else
            {
                gui.Start();
                await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()));
            }
            

            gui.TaskCompletionSource.TrySetResult();

            return 0;
        }
    }
}
