using System;
using Spectre.Console;

namespace Tourmaline
{
    internal class GUI
    {
        internal Table table = new();

        internal void Start()
        {
            AnsiConsole.Live(table)
                .Start(ctx =>
                {
                    table.AddColumn("URL");
                    table.AddColumn("Status");
                    table.AddColumn("Type");
                });

        }
    }
}