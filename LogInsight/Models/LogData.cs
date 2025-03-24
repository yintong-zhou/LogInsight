using System;
using System.Collections.Generic;

namespace LogInsight.Models
{
    public class LogData
    {
        public DateTime DateTime { get; set; }
        public string LogEntryType { get; set; }
        public string Source { get; set; }
        public string AppName { get; set; }
        public string Context { get; set; }
        public string Message { get; set; }
        public List<LogData> logList { get; set; }

    }
}
