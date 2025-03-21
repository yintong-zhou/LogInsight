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
        public DateTime DateTime { get; set; }
        public string Level { get; set; }
        public string AppName { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public List<LogData> logList { get; set; }
    }
}