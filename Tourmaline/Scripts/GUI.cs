using System;
using System.Reflection.Metadata.Ecma335;
using Spectre.Console;

namespace Tourmaline
{
    internal class GUI
    {
        internal Table Table = new();
        internal Action<string, string, string> AddRow = (a,b,c) => { };
        internal TaskCompletionSource TaskCompletionSource = new();

        internal void Start()
        {
            Table.AddColumns("URL", "Type", "Status")
                .Alignment(Justify.Left)
                .Width = 300;

            AnsiConsole.Live(Table).StartAsync(async (ctx) => 
            {
                AddRow = (a, b, c) => { Table.AddRow($"[blue]{a}[/]", b, c); ctx.UpdateTarget(Table); };
                await TaskCompletionSource.Task;
            });
        }
    }
}