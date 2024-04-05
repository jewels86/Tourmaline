using System;
using System.Net.Http;
using System.Runtime.Loader;
using HtmlAgilityPack;

namespace Tourmaline.Scripts 
{
    public class TourmaineAgent
    {
        private string url;
        private int rateLimit;
        private int maxPaths;

        public string URL { get { return url; } }
        public int RateLimit { get { return rateLimit; } }
        public int MaxPaths { get { return maxPaths; } }

        public TourmaineAgent(string url, int rateLimit = 60, int maxPaths = int.MaxValue) 
        {
            this.url = url;
            this.rateLimit = rateLimit;
            this.maxPaths = maxPaths;
        }

        public async Task<List<Path>> Start(Action<Path>? next = null)
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
                Path path = new();
                string adr = queue.Dequeue();
                if (strpaths.Contains(adr)) continue;

                response = await client.GetAsync(url + adr);
                string type = response.Content.Headers.ContentType?.MediaType ?? "unknown";

                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                path.Address = adr;
                path.Status = (int)response.StatusCode;
                path.URL = url + adr;
                path.Type = type;

                strpaths.Add(adr);

                if (type == "text/html")
                {
                    string html = await response.Content.ReadAsStringAsync();

                    HtmlDocument doc = new();
                    doc.LoadHtml(html);
                    IEnumerable<HtmlNode> nodes = doc.DocumentNode.SelectNodes("//img | //script | //link | //a");
                    if (nodes != null)
                    {
                        foreach (HtmlNode node in nodes)
                        {
                            string src = node.GetAttributeValue("src", "");
                            string href = node.GetAttributeValue("href", "");

                            if (!string.IsNullOrEmpty(src)) queue.Enqueue(src);
                            if (!string.IsNullOrEmpty(href)) queue.Enqueue(href);
                        }
                    }
                }

                paths.Add(path);
                next?.Invoke(path);
                i++;
            }

            client.Dispose();
            return paths;
        }

        private void ProcessURL()
        {
            url = url.EndsWith("/") ? url : url + "/";
            url = url.StartsWith("http://") ? url : "http://" + url;
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