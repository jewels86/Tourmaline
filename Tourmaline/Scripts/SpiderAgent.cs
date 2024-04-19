using System.Text.RegularExpressions;
using Spectre.Console;

namespace Tourmaline.Scripts 
{
    public class SpiderAgent
    {
        public string URL { get; set; }
        public int? RateLimit { get; set; }
        public ulong? MaxPaths { get; set; }
        public bool DevMode { get; set; } = false;
        public string? OutfilePath { get; set; }
        public bool BareOutfile { get; set; } = false;
        public Regex? Regex { get; set; }
        public Regex? IgnoreRegex { get; set; }
        public short Threads { get; set; } = 4;

        public SpiderAgent(string url) 
        {
            URL = url;
        }

        public async Task<List<Path>> Start(Action<Path>? next = null)
        {
            List<Path> output = new();
            List<string> strpaths = new();
            HttpClient client = new();
            Queue<string> queue = new();

            HttpResponseMessage[] responses = new HttpResponseMessage[Threads];
            Path[] paths = new Path[Threads];

            object pathsLock = new();
            object strpathsLock = new();
            object queueLock = new();
            object iLock = new();

            ulong i = 0;
            bool waitFM = false; // Wait for more/me

            string _tmp = URL;
            ProcessURL(ref _tmp);
            URL = _tmp;
            if (await VerifySite() == false) 
                throw new Exception("The base page of the site either returned a non success status code or is not of type 'text/html'");

            queue.Enqueue(URL);

            async void thread(int tn) 
            {
                // tn = Thread Number
                while (true)
                {
                    try
                    {
                        lock (iLock) if ((MaxPaths is not null ? true : i <= MaxPaths) == true) 
                                return;
                        string address;
                        if (queue.Count == 0 && !waitFM)
                            return;
                        else if (waitFM) while (waitFM && queue.Count == 0)
                                await Task.Delay(50);
                        lock (queueLock)
                        {
                            address = queue.Dequeue();
                            if (queue.Count == 0)waitFM = true;
                        }

                        ProcessURL(ref address);

                        if (strpaths.Contains(address) || !address.Contains(CutURLToDomain(URL))) 
                            continue;

                        lock (strpathsLock)
                        {
                            strpaths.Add(address);
                        }

                        responses[tn] = await client.GetAsync(address);

                        paths[tn] = new()
                        {
                            URL = address,
                            Status = (int)responses[tn].StatusCode,
                            Type = responses[tn].Content.Headers.ContentType?.MediaType ?? "unknown"
                        };

                        if (paths[tn].Status > 400)
                            continue;

                        if (paths[tn].Type.Contains("html"))
                        {
                            string html = await responses[tn].Content.ReadAsStringAsync();
                            Regex regex = new(@"(src|href|action)=""([a-zA-Z0-9\\\/\.?!#,=:;&% ]+)""");
                            MatchCollection matches = regex.Matches(html);
                            foreach(Match match in matches)
                            {
                                lock (queueLock) queue.Enqueue(match.Groups[2].ToString());
                            }
                            
                        }
                        else if (paths[tn].Type.Contains("text"))
                        {
                            string text = await responses[tn].Content.ReadAsStringAsync();
                            Regex regex = new(@"['""]([a-zA-Z0-9\\\/\.?!#,=:;&% ]+[\\\/\.][a-zA-Z0-9\\\/\.?!#,=:;&% ]+)['""]");
                            MatchCollection matches = regex.Matches(@"['""]([a-zA-Z0-9\\\/\.?!#,=:;&% ]+[\\\/\.][a-zA-Z0-9\\\/\.?!#,=:;&% ]+)['""]");
                            foreach (Match match in matches)
                            {
                                lock (queueLock) queue.Enqueue(match.Groups[0].ToString());
                                await Task.Delay(10);
                            }
                        }

                        if ((Regex?.IsMatch(address) ?? true) == true && (IgnoreRegex?.IsMatch(address) ?? false) == false)
                        {
                            next?.Invoke(paths[tn]); output.Add(paths[tn]);
                        }
                        lock (iLock) i++;
                        responses[tn].Dispose();
                    } 
                    catch
                    {
                        if (DevMode) throw;
                    }
                    waitFM = false;
                }
            }

            Task[] tasks = new Task[Threads];
            for (int j = 0; j < Threads; j++)
            {
                await Task.Delay(10);
                tasks[j] = new Task(() => thread(j - 1));
                tasks[j].Start();
            }
            while (queue.Count > 0 || waitFM) 
                await Task.Delay(50);
            foreach (Task task in tasks)
            {
                task.Wait();
            }

            if (OutfilePath != null)
            {
                File.Create(OutfilePath);
                Path[] array = output.ToArray();
                string[] realArray = [];

                int k = 0;
                if (!BareOutfile)
                {
                    foreach (var path in array)
                    {
                        realArray[k] = path.ToString();
                        k++;
                    }
                } else
                {
                    foreach (var path in array)
                    {
                        realArray[k] = path.URL;
                        k++;
                    }
                }

                File.WriteAllLines(OutfilePath, realArray);
            }

            client.Dispose();
            return output;
        }

        internal void ProcessURL(ref string url, bool isBaseURL = false)
        {
            if (url.StartsWith("/") && !isBaseURL)
            {
                url = URL + url;
            }

            url = url.StartsWith("http://") ? url.Substring(7) : url;
            url = url.StartsWith("https://") ? url.Substring(8) : url;
            url = url.StartsWith("www.") ? url.Substring(4) : url;
            url = $"http://{url}";
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