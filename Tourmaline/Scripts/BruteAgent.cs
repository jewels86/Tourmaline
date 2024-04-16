namespace Tourmaline.Scripts
{
    public class BruteAgent
    {
        public string URL { get; set; }
        public string WordlistPath { get; set; }
        public string? OutfilePath { get; set; }
        public bool DevMode { get; set; } = false;
        public bool BareOutfile { get; set; } = false;
        public short Threads { get; set; } = 1;

        internal BruteAgent(string wordlistPath, string url)
        {
            WordlistPath = wordlistPath;
            URL = url ;
        }

        internal async Task<List<Path>> Start(Action<Path>? next = null)
        {
            List<Path> output = new();
            HttpClient client = new();
            Queue<string> queue;

            object outputLock = new();
            object clientLock = new();
            object queueLock = new();

            string _tmp = URL;
            ProcessURL(ref _tmp, true);
            URL = _tmp;

            var file = await File.ReadAllLinesAsync(WordlistPath);
            queue = new(file);

            async Task thread(ThreadCompletionSource tcs)
            {
                string address;
                lock (queueLock)
                {
                    if (queue.Count == 0) return;
                    address = queue.Dequeue()!;
                }

                ProcessURL(ref address);
                HttpClient _client;

                lock (clientLock)
                {
                    _client = client;
                }

                HttpResponseMessage response = await _client.GetAsync(address);
                if ((short)response.StatusCode > 400) return;

                Path path = new();
                path.URL = address;
                path.Status = (int)response.StatusCode;
                path.Type = response.Content.Headers.ContentType?.MediaType ?? "unknown";

                lock (outputLock)
                {
                    output.Add(path);
                }

                tcs.Finish();
                next?.Invoke(path);
            };

            short openThreads = 0;
            List<ThreadCompletionSource> tcsl = new();
            while (queue.Count - 1 > 0)
            {
                if (openThreads >= Threads)
                {
                    while (openThreads >= Threads) await Task.Delay(50);
                }
                ThreadCompletionSource tcs = new();
                thread(tcs);
                tcsl.Add(tcs);
            }
            foreach (ThreadCompletionSource tcs in tcsl)
            {
                if (!tcs.IsFinished()) await tcs.Wait();
            }

            if (OutfilePath != null)
            {
                if (!File.Exists(OutfilePath)) { 
                    FileStream stream = File.Create(OutfilePath); 
                    stream.Close();
                }
                
                Path[] array = output.ToArray();
                string[] realArray = new string[array.Length];

                int i = 0;
                if (!BareOutfile)
                {
                    foreach (var path in array)
                    {
                        realArray[i] = path.ToString();
                        i++;
                    }
                } else
                {
                    foreach (var path in array)
                    {
                        realArray[i] = path.URL;
                        i++;
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
    }
}
