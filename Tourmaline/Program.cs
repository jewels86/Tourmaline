using System;
using Tourmaline;
using Tourmaline.Scripts;

namespace Tourmaline
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            /*TourmaineAgent agent = new();
            _ = await agent.Start((path) => { Console.WriteLine($"{path.URL} ({path.Status} - {path.Type})"); });*/
            var parsedArgs = ArgsParser.Parse(args);
            Console.WriteLine($"{parsedArgs["u"]}");
        }
    }
}