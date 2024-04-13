using System.ComponentModel;
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
            public int? MaxPaths { get; init; }

            [Description("Initates dev mode")]
            [CommandOption("-d|--dev-mode")]
            public bool? DevMode { get; init; }

            [CommandOption("--outfile-bare")]
            public bool? OutfileBare { get; init; }
            [CommandOption("--output-bare")]
            public bool? OutputBare { get; init; }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            Table table = new();
            table.AddColumns("[Blue]Tourmaline[/]", "");
            table.AddRow("Lincense", "GPL v3")
                .AddRow("Creator(s)", "Jewels")
                .AddRow("Mode", "Spider")
                .AddRow("URL", settings.URL)
                .AddRow("Max paths?", settings.MaxPaths?.ToString() ?? "false")
                .AddRow("Dev mode?", settings.DevMode?.ToString() ?? false.ToString());
            table.Width(100);
            AnsiConsole.Write(table);

            Status status = AnsiConsole.Status();

            GUI gui = new();
            SpiderAgent agent = new(settings.URL);

            await status.StartAsync("Starting...", async ctx =>
            {
                await Task.Delay(200);

                ctx.Status = "Configuring agent...";
                if (settings.MaxPaths != null) agent.MaxPaths = (int)settings.MaxPaths;
                if (settings.DevMode != null && settings.DevMode == true) agent.DevMode = (bool)settings.DevMode;
                if (settings.OutfileBare != null && settings.OutfileBare == true) agent.BareOutfile = (bool)settings.OutfileBare;
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
            }
            else
            {
                gui.Start();
                await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()));
            }

            gui.TaskCompletionSource.TrySetResult();

            return 0;
        }
    }
}
