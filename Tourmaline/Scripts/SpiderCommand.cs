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

            [Description("Initates dev mode")]
            [CommandOption("-d|--dev-mode")]
            public bool? DevMode { get; init; }

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
            table.AddRow("Lincense", "GPL v3")
                .AddRow("Creator(s)", "Jewels")
                .AddRow("Mode", "Spider")
                .AddRow("URL", settings.URL)
                .AddRow("Outfile?", settings.OutfilePath ?? "null")
                .AddRow("Max paths?", settings.MaxPaths?.ToString() ?? "null")
                .AddRow("Dev mode?", settings.DevMode?.ToString() ?? false.ToString())
                .AddRow("Regex?", settings.Regex is not null ? new StringBuilder(settings.Regex).Replace("[", "[[").Replace("]", "]]").ToString() : "null")
                .AddRow("Ignore Regex?", settings.IgnoreRegex is not null ? new StringBuilder(settings.IgnoreRegex).Replace("[", "[[").Replace("]", "]]").ToString() : "null");
            table.Width(100);
            AnsiConsole.Write(table);

            Status status = AnsiConsole.Status();

            GUI gui = new();
            SpiderAgent agent = new(settings.URL);

            await status.StartAsync("Starting...", async ctx =>
            {
                await Task.Delay(200);

                ctx.Status = "Configuring agent...";
                if (settings.MaxPaths is not null) agent.MaxPaths = settings.MaxPaths;
                if (settings.DevMode is not null && settings.DevMode == true) agent.DevMode = (bool)settings.DevMode;
                if (settings.OutfileBare is not null && settings.OutfileBare == true) agent.BareOutfile = (bool)settings.OutfileBare;
                if (settings.Regex is not null) agent.Regex = new(settings.Regex);
                if (settings.IgnoreRegex is not null) agent.IgnoreRegex = new(settings.IgnoreRegex);
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
            if (settings.OutputBare is not null && settings.OutputBare == true)
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
