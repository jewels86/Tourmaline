using System;
using System.Net.Http;
using System.Runtime.Loader;

namespace Tourmaline.Scripts 
{
    public class Client 
    {
        private string url;
        private int rateLimit;
        private int maxPaths;

        public string URL { get { return url; } }
        public int RateLimit { get { return rateLimit; } }
        public int MaxPaths { get { return maxPaths; } }

        public Client(string url, int rateLimit = 60, int maxPaths = int.MaxValue) 
        {
            this.url = url;
            this.rateLimit = rateLimit;
            this.maxPaths = maxPaths;
        }

        public async Task<List<Path>> Start()
        {
            List<Path> paths = new();
            List<string> strpaths = new();
            HttpClient client = new();
            HttpResponseMessage response;
            Queue<string> queue = new();

            ProcessURL();
            if (await VerifySite() == false) 
                throw new Exception("The base page of the site either returned a non success status code or is not of type 'text/html'");

            queue.Enqueue("");

            int i = 0;
            while (queue.Count > 0 && i < maxPaths)
            {
                string adr = queue.Dequeue();
                if (strpaths.Contains(adr)) continue;

                response = await client.GetAsync(url + adr);

            }

            return paths;
        }

        private void ProcessURL()
        {
            url = url.EndsWith("/") ? url : url + "/";
        }
        private async Task<bool> VerifySite()
        {
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType?.MediaType != "text/html")
            {
                return false;
            }

            return true;
        }
    }
}