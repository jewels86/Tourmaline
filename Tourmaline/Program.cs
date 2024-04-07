using System;
using Tourmaline;
using Tourmaline.Scripts;

namespace Tourmaline
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            TourmaineAgent agent = new("google.com");
            _ = await agent.Start((path) => { Console.WriteLine($"{path.URL} ({path.Status} - {path.Type})"); });

            /*

            string url = "http://google.com";
            agent.ProcessURL(ref url, true);
            string url2 = "http://google.com/hello?h=h#div";
            agent.ProcessURL(ref url2);
            string url3 = "/h";
            agent.ProcessURL(ref url3);
            Console.WriteLine(url);
            Console.WriteLine(url2);
            Console.WriteLine(url3);

            Console.ReadKey();*/
        }
    }
}