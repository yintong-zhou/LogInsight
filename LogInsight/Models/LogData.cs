using System;

namespace LogInsight.Models
{
    public class LogData
    {
        public DateTime DateTime { get; set; }
        public string LogEntryType { get; set; }
        public string Source { get; set; }
        public string AppName { get; set; }
        public string Message { get; set; }
    }
}
