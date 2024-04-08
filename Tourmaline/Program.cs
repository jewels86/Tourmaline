using System;
using Spectre.Console;
using Tourmaline;
using Tourmaline.Scripts;

namespace Tourmaline
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            TourmaineAgent agent = new("httpstat.us");
            
            /*var parsedArgs = ArgsParser.Parse(args);
            Console.WriteLine($"{parsedArgs["u"]}");*/

            Console.WriteLine("Tourmaline (Owned by the Gold Team)");

            GUI gui = new();
            gui.Start();

            _ = await agent.Start((path) => 
            {
                gui.AddRow(path.URL, path.Type, path.Status.ToString());
            });

            gui.TaskCompletionSource.TrySetResult();
        }
    }
}