using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInsight.CLI.Utility
{
    public struct CmdType
    {
        public const string Help = "help";
        public const string Exit = "exit";
        public const string Clear = "clear";
        public const string Log = "log";
        public const string Read = "read";
    }

    public struct ReadOption
    {
        public const string Event = "-event";
        public const string Local = "-local";
    }

    internal class Command
    {

    }
}
   
