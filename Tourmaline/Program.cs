using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Tourmaline.Commands;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		CommandApp app = new CommandApp();

		app.Configure(config =>
		{
			config.AddCommand<SpiderCommand>("spider");
			config.AddCommand<BruteCommand>("brute");
			config.AddCommand<CMSCommand>("cms");
			config.AddCommand<ScanCommand>("scan");
			config.AddBranch("scrape", (scraper) =>
			{
				scraper.AddCommand<FileScraperCommand>("files");

			});
		});

		return await app.RunAsync(args);
	}
}