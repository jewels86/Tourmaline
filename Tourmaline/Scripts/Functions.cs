using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline.Scripts
{
	internal static class Functions
	{
		internal static string ProcessURL(string url, string? baseURL = null)
		{
			StringBuilder output = new(url);
			if (url[0] == '/')
			{
				if (baseURL is not null) output.Insert(0, baseURL);
			}
			if (output[output.Length - 1] == '/')
			{
				output.Remove(url.Length - 1, 1);
			}

			output.Replace("http://", "");
			output.Replace("https://", "");
			output.Replace("www.", "");

			output.Insert(0, "http://");

			string[] parts = output.ToString().Split('#', '?');
			return parts[0];

		}
		internal static string CutURLToDomain(string url)
		{
			return ProcessURL(url).Split("/")[2];
		}
		internal static async Task<bool> VerifySite(string url)
		{
			HttpClient client = new();
			HttpResponseMessage response = await client.GetAsync(url);
			if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType?.MediaType != "text/html")
			{
				return false;
			}

			client.Dispose();
			response.Dispose();

			return true;
		}
	}
}
