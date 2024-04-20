namespace Tourmaline.Scripts
{
    public class BruteAgent
    {
        public string URL { get; set; }
        public string WordlistPath { get; set; }
        public string? OutfilePath { get; set; }
        public bool DevMode { get; set; } = false;
        public bool BareOutfile { get; set; } = false;
        public short Threads { get; set; } = 4;

        internal BruteAgent(string wordlistPath, string url)
        {
            WordlistPath = wordlistPath;
            URL = url ;
        }

        internal async Task<List<Path>> Start(Action<Path>? next = null)
        {
            List<Path> output = [];
            HttpClient client = new();
            Queue<string> queue;

            object outputLock = new();
            object queueLock = new();
            object nextLock = new();

            string _tmp = URL;
            ProcessURL(ref _tmp, true);
            URL = _tmp;

            queue = new(await File.ReadAllLinesAsync(WordlistPath));

            async void thread(ThreadCompletionSource tcs)
            {
                try
                {
                    string address;
                    lock (queueLock)
                    {
                        if (queue.Count == 0) { tcs.Finish(); return; }
                        address = $"{URL}/{queue.Dequeue()!}";
                    }

                    ProcessURL(ref address);

                    HttpResponseMessage response = await client.GetAsync(address);
                    if ((int)response.StatusCode > 400) { tcs.Finish(); return; }


                    Path path = new()
                    {
                        URL = address,
                        Status = (int)response.StatusCode,
                        Type = response.Content.Headers.ContentType?.MediaType ?? "unknown"
                    };

                    lock (outputLock)
                    {
                        output.Add(path);
                    }

                    tcs.Finish();
                    lock (nextLock) next?.Invoke(path);
                }
                catch
                {
                    if (DevMode == true) throw;
                    tcs.Finish();
                    return;
                }

            }

            short openThreads = 0;
            Task[] tasks = new Task[Threads];
            while (queue.Count - 1 > 0)
            {
                while (openThreads >= tasks.Length) await Task.Delay(50);

                ThreadCompletionSource tcs = new();
                tcs.Finished += () => { openThreads -= 1; };
                tasks[openThreads] = new(() => thread(tcs));
                tasks[openThreads].Start();

                openThreads++;
            }
            foreach (Task task in tasks)
            {
                task.Wait();
            }

            if (OutfilePath != null)
            {
                if (!File.Exists(OutfilePath)) { 
                    FileStream stream = File.Create(OutfilePath); 
                    stream.Close();
                }
                
                Path[] array = [.. output];
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
            if (url.StartsWith('/') && !isBaseURL)
            {
                url = URL + url;
            }

            url = url.StartsWith("http://") ? url[7..] : url;
            url = url.StartsWith("https://") ? url[8..] : url;
            url = url.StartsWith("www.") ? url[4..] : url;
            url = $"http://{url}";
            string[] parts = url.Split('#', '?');
            url = parts[0];

        }
    }
}
