
using System.ComponentModel.DataAnnotations;
using Tourmaline.Commands;

namespace Tourmaline.Enumerators 
{
    internal class Brute 
    {
        public string URL { get; set; }
        public int Threads { get; set;}
        public int Depth { get; set;}
        public string OutFile { get; set;}

        public Brute(BruteCommand.Settings settings)
        {
            URL = settings.URL;
            Threads = settings.Threads;
            Depth = settings.Depth;
            OutFile = settings.OutFile;
        }
    }
}