using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tourmaline.Scripts.Functions;

namespace Tourmaline.Scripts
{
	internal class CMSAgent
	{
		internal string URL { get; set; }

		internal CMSAgent(string url)
		{
			URL = url;
		}

		internal async Task<CMS> Identify()
		{
			if (!await VerifySite(URL)) return CMS.Unknown;

			URL = ProcessURL(URL);

			HttpClient client = new();
			HttpResponseMessage tmpmsg;
			List<string> wordlist = [];

			// WordPress 
			wordlist = new(File.ReadAllLines("wordlists/CMS/WordPress.txt"));
			foreach (string word in wordlist)
			{
				tmpmsg = await client.GetAsync($"{URL}/{word}");
				if (tmpmsg.IsSuccessStatusCode)
				{
					if ((await tmpmsg.Content.ReadAsStringAsync()).Contains("Word Press"))
					{
						client.Dispose();
						tmpmsg.Dispose();
						return CMS.WordPress;
					}
				}
			}

			// Google Sites
			tmpmsg = await client.GetAsync(URL);
			if ((await tmpmsg.Content.ReadAsStringAsync()).Contains("Google Sites")) 
			{
				client.Dispose();
				tmpmsg.Dispose();
				return CMS.GoogleSites; 
			}

			client.Dispose();
			tmpmsg.Dispose();
			return CMS.Unknown;
		}

		internal async Task<List<Path>> Exploit(CMS cms)
		{
			HttpClient client = new();
			HttpResponseMessage msg;
			Path path;
			string tmp;
			List<Path> output = [];
			foreach (string str in File.ReadAllLines($"wordlists/CMS/{cms}.txt"))
			{
				tmp = ProcessURL(str, URL);
				msg = await client.GetAsync(tmp);
				path = new()
				{
					Status = (int)msg.StatusCode,
					Type = msg.Content.Headers.ContentType?.MediaType ?? "unknown",
					URL = tmp
				};

				msg.Dispose();

				if (msg.IsSuccessStatusCode)
				{
					continue;
				}

				output.Add(path);
			}
			client.Dispose();
			return output;
		}
	}
}
