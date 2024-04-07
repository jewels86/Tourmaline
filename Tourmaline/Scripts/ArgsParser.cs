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

        internal static Dictionary<string, string> Parse(string[] args)
        {
            Dictionary<string, string> output = new();

            bool nextIsValue = false;
            string currentFlag = "";
            foreach (string arg in args)
            {
                if (arg.StartsWith('-') || arg.Length >= 2)
                {
                    // is a flag
                    currentFlag = $"{arg[1]}";

                    if (nextIsValue) throw new Exception("Invalid arguments");
                    if (!Args.ContainsKey(currentFlag)) throw new Exception("Invalid arguments");

                    if (Args[currentFlag] == true) nextIsValue = true;
                    else nextIsValue = false;
                } 
                else
                {
                    // not a flag
                    if (!nextIsValue) throw new Exception("Invalid arguments");

                    output[currentFlag] = arg;
                    nextIsValue = false;
                }
            }
            return output;

        }
    }
}