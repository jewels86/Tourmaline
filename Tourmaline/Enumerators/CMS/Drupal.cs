using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
	internal static async Task<(float, string)> Drupal(string url, HttpClient client, bool verbose)
	{
		float score = 0;
		HttpResponseMessage res = await client.GetAsync(url);

		score += await Functions.ScorePaths(url, "drupal", client, verbose);
		score += await Functions.AnalyzeHTML("drupal", res, verbose);

		// header analysis
		string[] headers = ["X-Powered-By: Drupal"];
		int headerScore = 0;

		foreach (string header in headers)
		{
			if (res.Headers.Contains(header.Split(": ")[0]))
			{
				if (res.Headers.GetValues(header.Split(": ")[0]).Contains(header.Split(": ")[1])) headerScore += 1;
			}
		}

		score += headerScore / headers.Length;
		score = score / 3;

		return (score, $"[green]Drupal[/]: {score}% certainty (No other notes - coming soon)");
	}
}