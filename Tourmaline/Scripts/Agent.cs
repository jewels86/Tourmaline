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

            ProcessURL(ref url);
            if (await VerifySite() == false) 
                throw new Exception("The base page of the site either returned a non success status code or is not of type 'text/html'");

            queue.Enqueue(url);

            int i = 0;
            while (queue.Count > 0 && i < maxPaths)
            {
                Path path = new();
                string adr = queue.Dequeue();
                ProcessURL(ref adr);

                if (strpaths.Contains(adr)) continue;
                if (!adr.Contains(CutURLToDomain(url))) continue;

                response = await client.GetAsync(adr);
                string type = response.Content.Headers.ContentType?.MediaType ?? "unknown";

                if (400 <= (int)response.StatusCode && (int)response.StatusCode < 500) continue;

                path.Status = (int)response.StatusCode;
                path.URL = adr;
                path.Type = type;

                strpaths.Add(adr);

                if (type == "text/html")
                {
                    if (!response.IsSuccessStatusCode) continue;
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

        internal void ProcessURL(ref string url, bool isBaseURL = false)
        {
            if (url.StartsWith("/")  && !isBaseURL)
            {
                url = this.url + url;
            }

            url = url.StartsWith("http://") || url.StartsWith("https://") ? url : "http://" + url;

            string[] parts = url.Split('#', '?');
            url = parts[0];
            
        }
        internal string CutURLToDomain(string url)
        {
            string output = url;
            ProcessURL(ref output);
            string[] parts = output.Split("/");
            return parts[2];
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