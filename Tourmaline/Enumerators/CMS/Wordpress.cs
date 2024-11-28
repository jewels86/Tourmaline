using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

		string version = "No version found";
		string content = await res.Content.ReadAsStringAsync();

		Regex regex1 = new Regex(@"<meta name=[""']generator[""'] content=[""']WordPress (\d+\.\d+(\.\d+)?)[""']>", RegexOptions.IgnoreCase);
		Match match1 = regex1.Match(content);

		if (match1.Success)
		{
			version = match1.Groups[1].Value;
			Console.WriteLine($"Version: {version}");
		}

		if (!match1.Success)
		{
			HttpResponseMessage rssFeed = await client.GetAsync(Functions.ResolveURL(url, "feed"));
			string rssContent = await rssFeed.Content.ReadAsStringAsync();

			Regex regex2 = new Regex(@"<generator>https://wordpress.org/\?v=(\d+\.\d+(\.\d+)?)</generator>", RegexOptions.IgnoreCase);
			Match match2 = regex2.Match(rssContent);

			if (match2.Success)
			{
				version = match2.Groups[1].Value;
				Console.WriteLine($"Version: {version}");
			}
		}

		return $"{score}% ({version})";
	}
}