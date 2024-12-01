
using Spectre.Console;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators
{
	internal class CMS
	{
		public string URL { get; set; }
		public string OutFile { get; set; }
		public bool Debug { get; set; }

		public CMS(CMSCommand.Settings settings)
		{
			URL = Functions.RemoveTrailingSlash(settings.URL);
			OutFile = settings.OutFile;
			Debug = settings.Debug;
		}

		public async Task<List<(float score, string val)>> Enumerate()
		{
			List<string> found = new();
			HttpClient client = new();

			if (Debug) Console.WriteLine("Starting CMS detection...");

			(float wordpressScore, string wordpress) = await CMSFuncs.Wordpress(URL, client, Debug);
			(float joomlaScore, string joomla) = await CMSFuncs.Joomla(URL, client, Debug);
			(float drupalScore, string drupal) = await CMSFuncs.Drupal(URL, client, Debug);

			var scores = new List<(float score, string val)>
			{
				(wordpressScore, wordpress),
				(joomlaScore, joomla),
				(drupalScore, drupal)
			};

			scores.Sort((x, y) => y.score.CompareTo(x.score));

			return scores;
		}
	}
}