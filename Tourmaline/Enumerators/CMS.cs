
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
		public bool NoWordPress { get; set; }
		public bool NoJoomla { get; set; }
		public bool NoDrupal { get; set; }

		public CMS(CMSCommand.Settings settings)
		{
			URL = Functions.RemoveTrailingSlash(settings.URL);
			OutFile = settings.OutFile;
			Debug = settings.Debug;
			NoWordPress = settings.NoWordPress;
			NoJoomla = settings.NoJoomla;
			NoDrupal = settings.NoDrupal;
		}

		public async Task<List<(float score, string val)>> Enumerate()
		{
			List<string> found = new();
			HttpClient client = new();
			var scores = new List<(float score, string val)>();

			if (Debug) Console.WriteLine("Starting CMS detection...");

			if (!NoWordPress)
			{
				(float wordpressScore, string wordpress) = await CMSFuncs.Wordpress(URL, client, Debug);
				scores.Add((wordpressScore, wordpress));
			}

			if (!NoJoomla)
			{
				(float joomlaScore, string joomla) = await CMSFuncs.Joomla(URL, client, Debug);
				scores.Add((joomlaScore, joomla));
			}

			if (!NoDrupal)
			{
				(float drupalScore, string drupal) = await CMSFuncs.Drupal(URL, client, Debug);
				scores.Add((drupalScore, drupal));
			}

			scores.Sort((x, y) => y.score.CompareTo(x.score));

			return scores;
		}
	}
}