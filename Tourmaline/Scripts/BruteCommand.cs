using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace Tourmaline.Scripts
{
    public sealed class BruteCommand : AsyncCommand<BruteCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<url>")]
            public string URL { get; set; }
            [CommandArgument(1, "<wordlistPath>")]
            public string WordlistPath { get; set; }

            [CommandOption("-d")]
            public bool? DevMode { get; set; }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            BruteAgent agent = new(settings.WordlistPath, settings.URL);
            //if (settings.MaxPaths != null) agent.MaxPaths = (int)settings.MaxPaths;
            if (settings.DevMode != null && settings.DevMode == true) agent.DevMode = (bool)settings.DevMode;

            GUI gui = new();

            gui.Start();
            await agent.Start((path) => gui.AddRow(path.URL, path.Type, path.Status.ToString()));

            gui.TaskCompletionSource.TrySetResult();

            return 0;
        }
    }
}
