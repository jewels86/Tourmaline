namespace Tourmaline.Scripts
{
    public class BruteAgent
    {
        public string URL { get; set; }
        public string WordlistPath { get; set; }
        public string? OutfilePath { get; set; }
        public bool DevMode { get; set; } = false;
        public bool BareOutfile { get; set; } = false;

        internal BruteAgent(string wordlistPath, string url)
        {
            WordlistPath = wordlistPath;
            URL = url ;
        }

        internal async Task<List<Path>> Start(Action<Path>? next = null)
        {
            List<Path> output = new();
            HttpClient client = new();
            List<string> paths;

            string _tmp = URL;
            ProcessURL(ref _tmp, true);
            URL = _tmp;

            var file = await File.ReadAllLinesAsync(WordlistPath);
            paths = new(file);

            foreach (var path in paths)
            {
                try
                {
                    HttpResponseMessage res = await client.GetAsync($"{URL}/{path}");
                    if (!((int)res.StatusCode < 400)) continue;

                    Path pathOutput = new();
                    pathOutput.URL = $"{URL}/{path}";
                    pathOutput.Type = res.Content.Headers.ContentType?.MediaType ?? "unknown";
                    pathOutput.Status = (int)res.StatusCode;

                    output.Add(pathOutput);

                    next?.Invoke(pathOutput);
                } catch
                {
                    if (DevMode) throw;
                    continue;
                }
                
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

            url = url.StartsWith("http://") || url.StartsWith("https://") ? url : "http://" + url;

            string[] parts = url.Split('#', '?');
            url = parts[0];

        }
    }
}
