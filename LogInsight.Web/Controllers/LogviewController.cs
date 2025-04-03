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
        public ActionResult Index(LogData model)
        {
            var logs = Event.ReadFromFile(true);

            model.Logs.AddRange(logs.Select(log => new LogData
            {
                DateTime = log.DateTime,
                Level = log.LogEntryType,
                AppName = log.AppName,
                Source = log.Source,
                Context = log.Context,
                Message = log.Message
            }));

            return View(model);
        }
    }
}