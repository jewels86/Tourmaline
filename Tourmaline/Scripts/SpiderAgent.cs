using System.Text;
using System.Text.RegularExpressions;

namespace Tourmaline.Scripts 
{
    public class SpiderAgent(string url)
    {
        public string URL { get; set; } = url;
        public ushort? StrayValue { get; set; }
        public uint? RateLimit { get; set; }
        public ulong? MaxPaths { get; set; }
        public bool DevMode { get; set; } = false;
        public string? OutfilePath { get; set; }
        public bool BareOutfile { get; set; } = false;
        public Regex? Regex { get; set; }
        public Regex? IgnoreRegex { get; set; }
        public ushort Threads { get; set; } = 4;

        public async Task<List<Path>> Start(Action<Path>? next = null)
        {
            Regex jsPathFinder = new(@"['""]([a-zA-Z0-9\\\/\.?!#,=:;&% ]+[\\\/\.][a-zA-Z0-9\\\/\.?!#,=:;&% ]+)['""]");
            Regex htmlPathFinder = new(@"(src|href|action)=""([a-zA-Z0-9\\\/\.?!#,=:;&% ]+)""");

            List<Path> output = [];
            List<string> strpaths = [];
            HttpClient client = new();
            Queue<string> queue = new();

            HttpResponseMessage[] responses = new HttpResponseMessage[Threads];
            Path[] paths = new Path[Threads];
            string[] addresses = new string[Threads];
            if (StrayValue is not null) { Path[] parents = new Path[Threads]; }

            object pathsLock = new();
            object strpathsLock = new();
            object queueLock = new();
            object iLock = new();
            object tmpLock = new();

            StringBuilder tmp;

            ulong i = 0;
            bool waitFM = false; // Wait for more/me

            tmp = new(URL);
            ProcessURL(ref tmp);
            URL = tmp.ToString();
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
                        lock (iLock) if ((MaxPaths is not null || i <= MaxPaths) == true) 
                                return;
                        if (queue.Count == 0 && !waitFM)
                            return;
                        else if (waitFM) while (waitFM && queue.Count == 0)
                                await Task.Delay(50);
                        lock (queueLock)
                        {
                            addresses[tn] = queue.Dequeue();
                            if (queue.Count == 0) waitFM = true;
                        }

                        lock (tmpLock)
                        {
                            tmp = new(addresses[tn]);
                            ProcessURL(ref tmp);
                            addresses[tn] = tmp.ToString();
                        }
                        

                        if (strpaths.Contains(addresses[tn]) || !addresses[tn].Contains(CutURLToDomain(URL))) 
                            continue;

                        lock (strpathsLock)
                        {
                            strpaths.Add(addresses[tn].ToString());
                        }

                        responses[tn] = await client.GetAsync(addresses[tn]);

                        paths[tn] = new()
                        {
                            URL = addresses[tn],
                            Status = (int)responses[tn].StatusCode,
                            Type = responses[tn].Content.Headers.ContentType?.MediaType ?? "unknown"
                        };

                        if (paths[tn].Status > 400)
                            continue;

                        if (paths[tn].Type.Contains("html"))
                        {
                            foreach(Match match in htmlPathFinder.Matches(await responses[tn].Content.ReadAsStringAsync()))
                            {
                                lock (queueLock) queue.Enqueue(match.Groups[2].ToString());
                                waitFM = false;
                            }
                            
                        }
                        else if (paths[tn].Type.Contains("js"))
                        {
                            foreach (Match match in jsPathFinder.Matches(await responses[tn].Content.ReadAsStringAsync()))
                            {
                                lock (queueLock) queue.Enqueue(match.Groups[0].ToString());
                                waitFM = false;
                            }
                        }

                        if ((Regex?.IsMatch(addresses[tn]) ?? true) == true && (IgnoreRegex?.IsMatch(addresses[tn]) ?? false) == false)
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
                Path[] array = [.. output];
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

        internal void ProcessURL(ref StringBuilder url, bool isBaseURL = false)
        {
            if (url[0] == '/' && !isBaseURL)
            {
                url.Insert(0, URL);
            }
            if (url[url.Length - 1] == '/')
            {
                url.Remove(url.Length - 1, 1);
            }

            url.Replace("http://", "");
            url.Replace("https://", "");
            url.Replace("www.", "");

            url.Insert(0, "http://");

            string[] parts = url.ToString().Split('#', '?');
            url = new(parts[0]);
            
        }
        internal string CutURLToDomain(string url)
        {
            StringBuilder output = new(url);
            ProcessURL(ref output);
            return output.ToString().Split("/")[2];
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