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

        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            return 0;
        }
    }
}
