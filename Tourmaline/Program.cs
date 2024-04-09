using System;
using Spectre.Console.Cli;
using Tourmaline;
using Tourmaline.Scripts;

namespace Tourmaline
{
    public static class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Tourmaline!");
            CommandApp app = new();
            app.Configure(config =>
            {
                config.AddCommand<SpiderCommand>("spider");
                config.AddCommand<BruteCommand>("brute");
            });
            app.Run(args);
        }
    }
}