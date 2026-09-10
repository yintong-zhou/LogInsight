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
        public static string WinLogName = GetAppSetting("WinLogName", "Application");

        /// <summary>
        /// Application name from config file
        /// </summary>
        public static string AppName = GetAppSetting("AppName", "LogInsight");

        /// <summary>
        /// Local log file name
        /// </summary>
        public static string FallbackLogFile = GetAppSetting("LogDirectory", "Logs.txt");

        private static string GetAppSetting(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string GetLogFilePath()
        {
            return Path.IsPathRooted(FallbackLogFile)
                ? FallbackLogFile
                : Path.Combine(Environment.CurrentDirectory, FallbackLogFile);
        }

        private static bool IsEventViewerEnabled()
        {
            bool enabled;
            return bool.TryParse(ConfigurationManager.AppSettings["EnableEventViewer"], out enabled) && enabled;
        }

        private static void TryLogToFile(string message, EventLogEntryType type, string appName, string source, string context = null)
        {
            try
            {
                LogToFile(message, type, appName, source, context);
            }
            catch (Exception logException)
            {
                Console.WriteLine($"Unable to write fallback log: {logException.Message}");
            }
        }

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
                    TryLogToFile($"Event Source created.", EventLogEntryType.Information, AppName, Environment.MachineName);
                    return;
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Need permission to write on Event Viewer.");
                TryLogToFile($"SecurityException: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Access negated. Run as Administrator");
                TryLogToFile($"UnauthorizedAccessException: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging error.");
                TryLogToFile($"Exception: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
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

            bool inEventViewer = IsEventViewerEnabled();
            try
            {

                if(inEventViewer)
                {
                    using (EventLog eventLog = new EventLog(WinLogName))
                    {
                        eventLog.Source = AppName;
                        eventLog.WriteEntry(message, type);
                        LogToFile(message, type, AppName, Environment.MachineName, context);
                    }
                }
                else
                {
                    LogToFile(message, type, AppName, Environment.MachineName, context);
                }
               
            }
            catch (Exception ex)
            {
                TryLogToFile($"Impossible to write in Event Viewer. Original message: {message}. {ex.Message}", type, AppName, Environment.MachineName, context);
            }
        }

        internal static void CheckLogFile()
        {
            string fullPath = GetLogFilePath();

            try
            {
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(fullPath))
                {
                    File.AppendAllText(fullPath, $"DATETIME|TYPE|SOURCE|APP|CONTEXT|MESSAGE{Environment.NewLine}");
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
                CheckLogFile();
                File.AppendAllText(GetLogFilePath(), $"{DateTime.Now}|{type}|{source}|{appName}|{context}|{message}{Environment.NewLine}");
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

            return new List<LogData>();
        }

        public static List<LogData> ReadFromFile(bool ignoreDefaultPath = false)
        {
            try
            {
                List<LogData> data = new List<LogData>();
                string[] lines = File.ReadAllLines(GetLogFilePath());

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("DATETIME|", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string[] parts = line.Split(new[] { '|' }, 6);
                    DateTime logDateTime;
                    if (parts.Length != 6 || !DateTime.TryParse(parts[0], out logDateTime))
                    {
                        continue;
                    }

                    data.Add(new LogData
                    {
                        DateTime = logDateTime,
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
                TryLogToFile($"Impossible to read local log file: {ex.Message}", EventLogEntryType.Error, AppName, Environment.MachineName);
            }

            return new List<LogData>();
        }
    }
}
