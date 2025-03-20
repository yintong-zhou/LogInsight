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
        public static string SourceName = ConfigurationManager.AppSettings["AppName"];

        /// <summary>
        /// Local log file name
        /// </summary>
        public static string FallbackLogFile = ConfigurationManager.AppSettings["LogDirectory"];

        /// <summary>
        /// Create Event Source if not exixts
        /// </summary>
        public static void UseSource()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, WinLogName);
                    LogToFile($"Event Source created.", EventLogEntryType.Information);
                    return;
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Need permission to write on Event Viewer.");
                LogToFile($"SecurityException: {ex.Message}", EventLogEntryType.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Access negated. Run as Administrator");
                LogToFile($"UnauthorizedAccessException: {ex.Message}", EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging error.");
                LogToFile($"Exception: {ex.Message}", EventLogEntryType.Error);
            }
        }

        //-------------------------------------------------------------------
        // WRITING LOGS
        //-------------------------------------------------------------------

        /// <summary>
        /// Write in Event Viewer and file
        /// </summary>
        public static void WriteLog(string message, EventLogEntryType type, bool inEventViewer = false)
        {
            try
            {
                if(inEventViewer)
                {
                    using (EventLog eventLog = new EventLog(WinLogName))
                    {
                        eventLog.Source = SourceName;
                        eventLog.WriteEntry(message, type);
                        LogToFile(message, type);
                    }
                }
                else
                {
                    LogToFile(message, type);
                }
               
            }
            catch (Exception ex)
            {
                LogToFile($"Impossible to write in Event Viewer. Logs saved on file. {ex.Message}", EventLogEntryType.Error);
            }
        }

        internal static void CheckLogDir()
        {
            try
            {
                if (!Directory.Exists($"{FallbackLogFile}"))
                {
                    Directory.CreateDirectory($"{Environment.CurrentDirectory}/{FallbackLogFile}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{EventLogEntryType.Error}|Check Directory failed. {ex.Message}");
            }
        }

        internal static void LogToFile(string message, EventLogEntryType type)
        {
            try
            {
                CheckLogDir();
                File.AppendAllText($"{Environment.CurrentDirectory}/{FallbackLogFile}", $"{DateTime.Now}|{type}|{message}{Environment.NewLine}"); // DATETIME|TYPE|MESSAGE
            }
            catch (Exception ex)
            {
                throw new Exception($"{EventLogEntryType.Error}|Impossible to write on local file. {ex.Message}");
            }
        }

        //-------------------------------------------------------------------
        // READING LOGS
        //-------------------------------------------------------------------

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
                        .Where(x => x.Source == SourceName)
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

        public static List<LogData> ReadFromFile()
        {
            try
            {
                List<LogData> data = new List<LogData>();
                string[] lines = File.ReadAllLines($"{Environment.CurrentDirectory}/{FallbackLogFile}");
                foreach (var line in lines)
                {
                    string[] parts = line.Split('|');
                    data.Add(new LogData
                    {
                        DateTime = Convert.ToDateTime(parts[0]),
                        LogEntryType = parts[1],
                        Message = parts[2]
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
