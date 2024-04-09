using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Tourmaline;

namespace Tourmaline.Scripts
{
    public sealed class SpiderCommand : AsyncCommand<SpiderCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("URL to use the spider on")]
            [CommandArgument(0, "<url>")]
            public string URL { get; init; }

            [Description("The max amount of paths to search")]
            [CommandOption("-m|--max-paths")]
            public int? MaxPaths { get; init; }

            [Description("Initates dev mode")]
            [CommandOption("-d|--dev-mode")]
            public bool? DevMode { get; init; }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            SpiderAgent agent = new(settings.URL);
            if (settings.MaxPaths != null) agent.MaxPaths = (int)settings.MaxPaths;
            if (settings.DevMode != null && settings.DevMode == true) agent.DevMode = (bool)settings.DevMode;

            GUI gui = new();

            gui.Start();
            await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()));

            gui.TaskCompletionSource.TrySetResult();

            return 0;
        }
    }
}
