using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogInsight.Web.Models;

namespace LogInsight.Web.Controllers
{
    public class LogviewController : Controller
    {
        // GET: Logview
        public ActionResult Index(LogData _logData)
        {
            if(_logData == null) _logData = new LogData();
            if(_logData.logList == null) _logData.logList = new List<LogData>();
           
            var logs = Event.ReadFromFile(true);
            if (logs != null)
            {
                foreach (var log in logs)
                {
                    _logData.logList.Add(new LogData
                    {
                        DateTime = log.DateTime,
                        Level = log.LogEntryType,
                        AppName = log.AppName,
                        Source = log.Source,
                        Message = log.Message
                    });
                }
            }

            return View(_logData);
        }
    }
}