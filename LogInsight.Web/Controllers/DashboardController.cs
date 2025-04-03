using LogInsight.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace LogInsight.Web.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public ActionResult Index(LogData model) 
        {
            var logs = Event.ReadFromFile(true);
            model.FileSize = (new FileInfo(Event.FallbackLogFile).Length / 1024); // bytes

            model.Logs.AddRange(logs.Select(log => new LogData
            {
                DateTime = log.DateTime,
                Level = log.LogEntryType,
                AppName = log.AppName,
                Source = log.Source,
                Context = log.Context,
                Message = log.Message
            }));

            var items = model.Logs
                .GroupBy(l => l.Level)
                .Select(g => new 
                {
                    Level = g.Key, 
                    Count = g.Count() 
                });

            model.Counts.AddRange(items.Select(item => new LogCount
            {
                Level = item.Level,
                Count = item.Count
            }));

            return View(model);
        }
    }
}