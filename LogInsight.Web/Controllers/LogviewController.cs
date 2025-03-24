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
        [HttpGet]
        public ActionResult Index(LogData _logData)
        {
            var logs = Event.ReadFromFile(true);

            foreach (var log in logs)
            {
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

            return View(_logData);
        }
    }
}