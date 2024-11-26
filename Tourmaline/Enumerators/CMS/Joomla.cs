using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
	internal static async Task<string> Joomla(string url, HttpClient client, bool debug)
	{
		float score = 0;
		HttpResponseMessage res = await client.GetAsync(url);

		score += await Functions.ScorePaths(url, "joomla", client);
		score += await Functions.AnalyzeHTML("joomla", res);

		score = score / 4;

		return $"{score}% accuracy (No other notes)";
	}
}