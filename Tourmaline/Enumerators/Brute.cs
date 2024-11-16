
using System.ComponentModel.DataAnnotations;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators 
{
    internal class Brute 
    {
        public string URL { get; set; }
        public int Threads { get; set;}
        public int Depth { get; set;}
        public string OutFile { get; set;}
        public string[] WordList { get; set; }

        public Brute(BruteCommand.Settings settings, string[] wordlist)
        {
            URL = Functions.RemoveTrailingSlash(settings.URL);
            Threads = settings.Threads;
            Depth = settings.Depth;
            OutFile = settings.OutFile;
            WordList = wordlist;
		}

        public async Task Enumerate()
        {
            List<string> found = new();
            List<Task> tasks = new();
			HttpClient client = new();
            Queue<string> queue = new(WordList);

			object queueLock = new();

            Func<Task> thread = async () =>
            {
                for (int i = 0; i <= Depth; i++)
                {
                    while (queue.Count > 0)
                    {
                        string url;
                        lock (queueLock) url = queue.Dequeue();

                        url = Functions.ResolveURL(URL, url);

                        HttpResponseMessage res = await client.GetAsync(url);

                        if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            found.Add(url);
                        }
					}
                }
            };
		}
    }
}