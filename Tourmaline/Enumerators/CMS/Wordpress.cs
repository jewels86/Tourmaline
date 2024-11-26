using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
    internal static async Task<string> Wordpress(string url, HttpClient client, bool debug) 
    {
        float score = 0;
		HttpResponseMessage res = await client.GetAsync(url);

		score += await Functions.ScorePaths(url, "wordpress", client);
		score += await Functions.AnalyzeHTML("wordpress", res);

		// header analysis
		if (debug) Console.WriteLine("Analyzing headers...");

		int headerScore = 0;
		List<string> headers = ["X-Powered-By: WordPress"];

		foreach (string header in headers)
		{
			if (res.Headers.Contains(header.Split(": ")[0]))
			{
				if (res.Headers.GetValues(header.Split(": ")[0]).Contains(header.Split(": ")[1])) headerScore += 1;
			}
		}

		score += headerScore / headers.Count;

		// pattern analysis
		if (debug) Console.WriteLine("Analyzing patterns...");

		int patternScore = 0;

		HttpResponseMessage notfound = await client.GetAsync(Functions.ResolveURL(url, "probablynotfound"));
		string notfoundpage = await notfound.Content.ReadAsStringAsync();
		if (notfoundpage.Contains("<title>Page not found - WordPress</title>"))
		{
			patternScore += 1;
		}

		score += patternScore / 1;
		score = (score / 4);

		return $"{score}% (No other notes)";
	}
}