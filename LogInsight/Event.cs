using System;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LogInsight.Models;

namespace LogInsight
{
    public class Event
    {
        /// <summary>
        /// Event Viewer log name
        /// </summary>
        public static string WinLogName = "Application";

        /// <summary>
        /// Application name from config file
        /// </summary>
        public static string AppName = ConfigurationManager.AppSettings["AppName"];

        /// <summary>
        /// Local log file name
        /// </summary>
        public static string FallbackLogFile = ConfigurationManager.AppSettings["LogDirectory"];

        /// <summary>
        /// Create Event Source if not exists
        /// </summary>
        public static void UseSource()
        {
            try
            {
                if (!EventLog.SourceExists(AppName))
                {
                    EventLog.CreateEventSource(AppName, WinLogName);
                    LogToFile($"Event Source created.", EventLogEntryType.Information, AppName, Environment.MachineName);
                    return;
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Need permission to write on Event Viewer.");
                LogToFile($"SecurityException: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Access negated. Run as Administrator");
                LogToFile($"UnauthorizedAccessException: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging error.");
                LogToFile($"Exception: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        // WRITING LOGS
        //--------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Write in Event Viewer and file
        /// </summary>
        public static void WriteLog(string message, EventLogEntryType type, string context = null)
        {

            bool inEventViewer = bool.Parse(ConfigurationManager.AppSettings["EnableEventViewer"]);
            try
            {

                if(inEventViewer)
                {
                    using (EventLog eventLog = new EventLog(WinLogName))
                    {
                        eventLog.Source = AppName;
                        eventLog.WriteEntry(message, type);
                        LogToFile(message, type, AppName, Environment.MachineName);
                    }
                }
                else
                {
                    LogToFile(message, type, AppName, Environment.MachineName, context);
                }
               
            }
            catch (Exception ex)
            {
                LogToFile($"Impossible to write in Event Viewer. Logs saved on file. {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
        }

        internal static void CheckLogFile()
        {
            string fullDir = $"{Environment.CurrentDirectory}\\{FallbackLogFile}";

            try
            {
                if (!File.Exists(fullDir))
                {
                    //DATETIME|TYPE|SOURCE|APPNAME|MESSAGE
                    File.AppendAllText($"{Environment.CurrentDirectory}\\{FallbackLogFile}", $"DATETIME|TYPE|SOURCE|APP|CONTEXT|MESSAGE{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{EventLogEntryType.Error}|Check Directory failed. {ex.Message}");
            }
        }

        internal static void LogToFile(string message, EventLogEntryType type, string appName = null, string source = null, string context = null)
        {
            try
            {
                //CheckLogFile();
                File.AppendAllText($"{Environment.CurrentDirectory}\\{FallbackLogFile}", $"{DateTime.Now}|{type}|{source}|{appName}|{context}|{message}{Environment.NewLine}"); 
            }
            catch (Exception ex)
            {
                throw new Exception($"{EventLogEntryType.Error}|Impossible to write on local file. {ex.Message}");
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        // READING LOGS
        //--------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Read logs from Event Viewer
        /// </summary>
        public static List<LogData> ReadFromEventViewer()
        {
            try
            {
                List<LogData> data = new List<LogData>();
                using (EventLog eventLog = new EventLog(WinLogName))
                {
                    var entries = eventLog.Entries
                        .Cast<EventLogEntry>()
                        .Where(x => x.Source == AppName)
                        .OrderByDescending(x => x.TimeGenerated)
                        .ToList();

                    foreach (var entry in entries)
                    {
                        data.Add(new LogData
                        {
                            DateTime = entry.TimeGenerated,
                            LogEntryType = entry.EntryType.ToString(),
                            Message = entry.Message
                        });
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, EventLogEntryType.Error);
            }

            return null;
        }

        public static List<LogData> ReadFromFile(bool ignoreDefaultPath = false)
        {
            try
            {
                List<LogData> data = new List<LogData>();
                string[] lines = { };
                if (ignoreDefaultPath)
                    lines = File.ReadAllLines($"{ConfigurationManager.AppSettings["LogDirectory"]}");
                else lines = File.ReadAllLines($"{Environment.CurrentDirectory}\\{FallbackLogFile}");

                foreach (var line in lines)
                {
                    string[] parts = line.Split('|');
                    data.Add(new LogData
                    {
                        DateTime = Convert.ToDateTime(parts[0]),
                        LogEntryType = parts[1],
                        Source = parts[2],
                        AppName = parts[3],
                        Context = parts[4],
                        Message = parts[5],
                    });
                }

                return data;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, EventLogEntryType.Error);
            }

            return null;
        }
    }
}
