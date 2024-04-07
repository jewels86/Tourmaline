using System;

namespace Tourmaline.Scripts
{
    internal static class ArgsParser
    {
        /// <summary>
        /// All the args the program can have.
        /// The string shows the flag and the bool shows if it requires a value.
        /// </summary>
        internal static Dictionary<string, bool> Args = new()
        {
            { "u", true }
        };

        internal static Dictionary<string, string?> Parse(string[] args)
        {
            Dictionary<string, object> output = new();

            bool nextIsValue = false
            foreach (string arg in args)
            {
                if (arg.StartsWith('-') || arg.Length >= 2)
                {
                    // is a flag
                    if (nextIsValue) throw new Exception("Invalid arguments");
                    if (!Args.ContainsKey(arg[1])) throw new Exception("Invalid arguments");

                    if (Args[arg[1]] == true) nextIsValue = true;
                    else nextIsValue = false;
                } 
                else
                {
                    // not a flag
                }
            }

        }
    }
}