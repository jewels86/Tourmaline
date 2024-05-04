namespace Tourmaline.Scripts
{
	public class BruteAgent
	{
		public string URL { get; set; }
		public string WordlistPath { get; set; }
		public string? OutfilePath { get; set; }
		public bool DevMode { get; set; } = false;
		public bool BareOutfile { get; set; } = false;
		public ushort Threads { get; set; } = 4;

		internal BruteAgent(string wordlistPath, string url)
		{
			WordlistPath = wordlistPath;
			URL = url ;
		}

		internal async Task<List<Path>> Start(Action<Path>? found = null, Action? next = null)
		{
			List<Path> output = [];
			HttpClient client = new();
			Queue<string> queue;
			int length; 

			object outputLock = new();
			object queueLock = new();
			object nextLock = new();

			URL = Functions.ProcessURL(URL);

			queue = new(await File.ReadAllLinesAsync(WordlistPath));
			length = queue.Count;

			ThreadCompletionSource[] tcss = new ThreadCompletionSource[Threads];

			async Task thread(int tn)
			{
				try
				{
					string address;
					lock (queueLock)
					{
						if (queue.Count == 0) { tcss[tn].Finish(); return; }
						address = $"{URL}/{queue.Dequeue()!}";
					}

					address = Functions.ProcessURL(address, URL);

					HttpResponseMessage response = await client.GetAsync(address);
					if ((int)response.StatusCode > 400) 
					{
						tcss[tn].Finish();
						response.Dispose();
						next?.Invoke();
						return;
					}

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

					lock (nextLock) found?.Invoke(path);
					next?.Invoke();
					response.Dispose();
					tcss[tn].Finish();
					return;
				}
				catch
				{
					if (DevMode == true) throw;
					next?.Invoke();
					tcss[tn].Finish();
					return;
				}

			}

			short openThreads = 0;
			Task[] tasks = new Task[Threads];
			while (queue.Count > 0)
			{
				while (openThreads >= Threads) 
					await Task.Delay(50);

				tcss[openThreads] = new();
				tcss[openThreads].Finished += () => { openThreads -= 1; };
				tasks[openThreads] = new(async () => await thread(openThreads - 1));
				tasks[openThreads].Start();

				openThreads++;
			}

			while (openThreads != 0) await Task.Delay(50);

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

		private int NonNullInArray(object?[] array)
		{
			int output = 0;
			foreach (object? obj in array) if (obj is null) output += 1;
			return output;
		} 
	}
}
