using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using LogInsight.CLI.Utility;

namespace LogInsight.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{Event.SourceName}");

            while (true)
            {
                Console.Write($"{Environment.NewLine}>");
                string cmd = Console.ReadLine();
                if(cmd == CmdType.Help)
                {
                    foreach (var item in typeof(CmdType).GetFields())
                    {
                        Console.WriteLine(item.GetValue(null));

                        if (item.Name.ToLower() == CmdType.Read)
                        {
                            foreach (var subItem in typeof(ReadOption).GetFields())
                            {
                                Console.WriteLine(subItem.GetValue(null));
                            }
                        }
                    }
                }
                else if (cmd == CmdType.Exit)
                {
                    break;
                }
                else if (cmd == CmdType.Clear)
                {
                    Console.Clear();
                }
                else if (cmd == CmdType.Log)
                {
                    Event.UseSource();
                    Console.WriteLine("Logging INFORMATION");
                    Event.WriteLog("Logging INFORMATION", EventLogEntryType.Information);

                    Console.WriteLine("Logging WARNING");
                    Event.WriteLog("Logging WARNING", EventLogEntryType.Warning);

                    Console.WriteLine("Logging ERROR");
                    Event.WriteLog("Logging ERROR", EventLogEntryType.Error);

                    Console.WriteLine($"Done! Check in Event Viewer and here: {Environment.CurrentDirectory}/{Event.WinLogName}");
                }
                else if (cmd.StartsWith(CmdType.Read))
                {
                    if (cmd.EndsWith(ReadOption.Event))
                    {
                        var events = Event.ReadFromEventViewer();
                        foreach(var item in events)
                        {
                            Console.WriteLine($"{item.DateTime} [{item.LogEntryType}] {item.Message}");
                        }
                    }
                    else if (cmd.EndsWith(ReadOption.Local))
                    {
                        var events = Event.ReadFromFile();
                        foreach (var item in events)
                        {
                            Console.WriteLine($"{item.DateTime} [{item.LogEntryType}] {item.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command");
                }
            }
        }
    }
}
