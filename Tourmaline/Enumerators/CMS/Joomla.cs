﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tourmaline;

internal static partial class CMSFuncs
{
	internal static async Task<(float, string)> Joomla(string url, HttpClient client, bool debug)
	{
		float score = 0;
		HttpResponseMessage res = await client.GetAsync(url);

		float pathScore = await Functions.ScorePaths(url, "joomla", client);
		if (debug) Console.WriteLine($"Path Score: {pathScore}");
		score += pathScore;

		float htmlScore = await Functions.AnalyzeHTML("joomla", res);
		if (debug) Console.WriteLine($"HTML Score: {htmlScore}");
		score += htmlScore;

		// header analysis 
		string[] headers = ["X-Powered-By: Joomla!"];
		int headerScore = 0;

		foreach (string header in headers)
		{
			if (res.Headers.Contains(header.Split(": ")[0]))
			{
				if (res.Headers.GetValues(header.Split(": ")[0]).Contains(header.Split(": ")[1])) headerScore += 1;
			}
		}
		float headerScoreNormalized = (float)headerScore / headers.Length;
		if (debug) Console.WriteLine($"Header Score: {headerScoreNormalized}");
		score += headerScoreNormalized;

		score = score / 3;

		if (debug) Console.WriteLine($"Final Score: {score}");

		string version = "No version found";

		Regex regex = new Regex(@"\?version=(\d+\.\d+(\.\d+)?)", RegexOptions.IgnoreCase);
		Match match = regex.Match(url);

		if (match.Success)
		{
			version = match.Groups[1].Value;
		}
		else
		{

		}

		return (score, $"[green]Joomla[/]: {score}% accuracy ({version})");
	}
}