using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LogInsight.Web.Models
{
    public class LogData
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.MinValue;
        public string Level { get; set; }
        public string AppName { get; set; }
        public string Source { get; set; }
        public string Context { get; set; }
        public string Message { get; set; }
        public long FileSize { get; set; }
        public List<LogData> Logs { get; set; }
        public List<LogCount> Counts { get; set; }

        public LogData()
        {
            Logs = new List<LogData>();
            Counts = new List<LogCount>();
        }
    }

    public class LogCount
    {
        public string Level { get; set; }
        public int Count { get; set; }
    }
}