using System;
using System.Net.Http;
using System.Runtime.Loader;
using System.Windows.Markup;
using HtmlAgilityPack;

namespace Tourmaline.Scripts 
{
    public class SpiderAgent
    {
        public string URL { get; set; }
        public int RateLimit { get; set; }
        public int MaxPaths { get; set; }
        public bool DevMode { get; set; } = false;
        public string? OutfilePath { get; set; }

        public SpiderAgent(string url, int rateLimit = 60, int maxPaths = int.MaxValue) 
        {
            this.URL = url;
            this.RateLimit = rateLimit;
            this.MaxPaths = maxPaths;
        }

        public async Task<List<Path>> Start(Action<Path>? next = null)
        {
            List<Path> paths = new();
            List<string> strpaths = new();
            HttpClient client = new();
            HttpResponseMessage response;
            Queue<string> queue = new();

            string _tmp = URL;
            ProcessURL(ref _tmp);
            URL = _tmp;
            if (await VerifySite() == false) 
                throw new Exception("The base page of the site either returned a non success status code or is not of type 'text/html'");

            queue.Enqueue(URL);

            int i = 0;
            while (queue.Count > 0 && i < MaxPaths)
            {
                try
                {
                    Path path = new();
                    string adr = queue.Dequeue();
                    ProcessURL(ref adr);

                    if (strpaths.Contains(adr)) continue;
                    if (!adr.Contains(CutURLToDomain(URL))) continue;

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
                } catch
                {
                    if (DevMode) throw;
                    continue;
                }
                
            }

            if (OutfilePath != null)
            {
                File.Create(OutfilePath);
                Path[] array = paths.ToArray();
                string[] realArray = [];

                int k = 0;
                foreach (var path in array)
                {
                    realArray[k] = path.ToString();
                    k++;
                }

                File.WriteAllLines(OutfilePath, realArray);
            }

            client.Dispose();
            return paths;
        }

        internal void ProcessURL(ref string url, bool isBaseURL = false)
        {
            if (url.StartsWith("/") && !isBaseURL)
            {
                url = URL + url;
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
            HttpResponseMessage response = await client.GetAsync(URL);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType?.MediaType != "text/html")
            {
                return false;
            }

            return true;
        }
    }
}