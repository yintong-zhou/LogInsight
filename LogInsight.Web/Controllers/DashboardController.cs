using LogInsight.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogInsight.Web.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public ActionResult Index(LogData _logData)
        {
            var logs = Event.ReadFromFile(true);
            
            foreach (var log in logs) {
                _logData.logList.Add(new LogData
                {
                    DateTime = log.DateTime,
                    Level = log.LogEntryType,
                    AppName = log.AppName,
                    Source = log.Source,
                    Context = log.Context,
                    Message = log.Message
                });
            }

            var items = _logData.logList
                .GroupBy(l => l.Level)
                .Select(g => new 
                {
                    Level = g.Key, 
                    Count = g.Count() 
                });

            foreach (var item in items)
            {
                _logData.countList.Add(new LogCount
                {
                    Level = item.Level,
                    Count = item.Count
                });
            }

            return View(_logData);
        }
    }
}