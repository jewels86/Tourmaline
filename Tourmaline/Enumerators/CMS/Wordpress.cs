using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
    internal static async Task<float> Wordpress(string url, HttpClient client) 
    {
        float score = 0;

        score += await Functions.ScorePaths(url, "wordpress", client);

		// html analysis
        string html = await client.GetStringAsync(url);
		int htmlScore = 0;
        List<string> tags = ["<meta name=\"generator\" content=\"WordPress", "<!-- This website is powered by WordPress -->"];

		foreach (string tag in tags)
		{
			if (html.Contains(tag))
			{
				htmlScore += 1;
			}
		}

		score += htmlScore / tags.Count;

		// header analysis
		int headerScore = 0;
		List<string> headers = ["X-Powered-By: WordPress"];

		foreach (string header in headers)
		{
			if (client.DefaultRequestHeaders.Contains(header))
			{
				headerScore += 1;
			}
		}

		score += headerScore / headers.Count;

		// pattern analysis
		int patternScore = 0;

		string notfoundpage = await client.GetStringAsync(Functions.ResolveURL(url, "probablynotfound"));
		if (notfoundpage.Contains("<title>Page not found - WordPress</title>"))
		{
			patternScore += 1;
		}

		score += patternScore / 1;
		score = (score / 4) * 100;

		return score;
	}
}