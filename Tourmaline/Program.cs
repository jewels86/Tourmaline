using System;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline;
using Tourmaline.Scripts;

namespace Tourmaline
{
	public static class Program
	{

		public static void Main(string[] args)
		{
			CommandApp app = new();
			app.Configure(config =>
			{
				config.AddCommand<SpiderCommand>("spider");
				config.AddCommand<BruteCommand>("brute");
				config.AddCommand<BuilderCommand>("build");
				config.AddCommand<EnumerateCommand>("enumerate");
			});
			app.Run(args);
		}
	}
}