using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
    internal static async Task<float> Wordpress(string url) 
    {
        float score = 0;

        HttpClient client = new();
        HttpResponseMessage fileRes = await client.GetAsync("https://gold-team.tech/static/tourmaline/wordpress.txt");
		string file = await fileRes.Content.ReadAsStringAsync();
        Dictionary<string, int> files = Functions.ParseCMSFile(file);

        foreach (var kvp in files)
        {
            string p = kvp.Key;
            int s = kvp.Value;

			HttpResponseMessage res = await client.GetAsync(Functions.ResolveURL(url, p));
            if (res.IsSuccessStatusCode)
			{
				score += s;
			}
		}

        return score;
	}
}