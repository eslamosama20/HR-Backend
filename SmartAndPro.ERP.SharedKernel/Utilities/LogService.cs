using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

/*
/ Provides logging functionality for the application.
/ 
/ 
/ This class is responsible for logging error and information messages. It supports two modes of logging:
/ - On Windows platforms (Win32NT), it utilizes the Windows Event Log for logging.
/ - On non-Windows platforms, it writes log messages to the console.
/ The choice of logging mechanism is determined based on the operating system platform at runtime.
*/



namespace SmartAndPro.ERP.SharedKernel.Utilities
{
    public static class LogService
    {
        private static string logName = "SmartAndPro.ERP";
        //private static string logFilePath = "logs.json";
        private static string logDirectory = "/root/SmartAndPro.ERP.Inventory/Logs";
        private static string logFilePath = Path.Combine(logDirectory, "logs.json");
        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(logDirectory);
                }
                catch (UnauthorizedAccessException)
                {
                    // Silently fail if we don't have permission to create the log directory.
                }
            }
        }

        private static EventLog eventLog;

        public static void LogError(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerPath = "", [CallerLineNumber] int? lineNumber = 0, params object[] parameters)
        {
            var logMessage = new LogMessage(message, callerName, callerPath, lineNumber, parameters);
            WriteLog(logMessage, "Error");

            //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            if (false)
            {
                InitializeLog();
                using (var eventLog = new EventLog(logName))
                {
                    eventLog.Source = logName;
                    eventLog.WriteEntry(GenerateLogMessage(logMessage), EventLogEntryType.Error);
                }
            }
            else
            {
                Console.WriteLine($"Error occurred in method: {callerName}");
                Console.WriteLine($"File path: {callerPath}, Line: {lineNumber}");
                Console.WriteLine($"Message: {message}");

                if (parameters.Length > 0)
                {
                    Console.WriteLine("Additional parameters:");
                    foreach (var parameter in parameters)
                    {
                        Console.WriteLine(parameter);
                    }
                }
            }
        }

        public static void LogInformation(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerPath = "", [CallerLineNumber] int? lineNumber = 0, params object[] parameters)
        {
            var logMessage = new LogMessage(message, callerName, callerPath, lineNumber, parameters);
            WriteLog(logMessage, "Information");

            if (false)
            {
                using (var eventLog = new EventLog(logName))
                {
                    eventLog.Source = logName;
                    eventLog.WriteEntry(GenerateLogMessage(logMessage), EventLogEntryType.Information);
                }
            }
            else
            {
                Console.WriteLine($"Information logged from method: {callerName}");
                Console.WriteLine($"File path: {callerPath}, Line: {lineNumber}");
                Console.WriteLine($"Message: {message}");

                if (parameters.Length > 0)
                {
                    Console.WriteLine("Additional parameters:");
                    foreach (var parameter in parameters)
                    {
                        Console.WriteLine(parameter);
                    }
                }
            }
        }

        public static void LogError(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string callerPath = "", [CallerLineNumber] int? lineNumber = 0)
        {
            var logMessage = new LogMessage(exception.ToString(), callerName, callerPath, lineNumber);
            WriteLog(logMessage, "Error");

            if (false)
            {
                InitializeLog();
                using (var eventLog = new EventLog(logName))
                {
                    eventLog.Source = logName;
                    eventLog.WriteEntry(GenerateLogMessage(logMessage), EventLogEntryType.Error);
                }
            }
            else
            {
                Console.WriteLine($"Error occurred in method: {callerName}");
                Console.WriteLine($"File path: {callerPath}, Line: {lineNumber}");
                Console.WriteLine($"Exception details: {exception}");

                LogInnerExceptions(exception);
            }
        }

        private static void LogInnerExceptions(Exception exception, int depth = 0)
        {
            if (false)
            {
                Console.WriteLine($"Inner Exception {depth + 1}: {exception.InnerException}");
                LogInnerExceptions(exception.InnerException, depth + 1);
            }
        }

        private static void InitializeLog()
        {
            try
            {
                if (!EventLog.SourceExists(logName))
                {
                    EventLog.CreateEventSource(logName, logName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GenerateLogMessage(LogMessage logMessage)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine($"Caller: {logMessage.callerName}");
            logBuilder.AppendLine($"Caller Path: {logMessage.callerPath}");
            logBuilder.AppendLine($"Caller Line: {logMessage.lineNumber}");
            logBuilder.AppendLine($"Log Message: {logMessage.message}");

            return logBuilder.ToString();
        }

        private static void WriteLog(LogMessage logMessage, string logType)
        {
            try
            {
                EnsureLogDirectoryExists();
                var logEntry = new
                {
                    LogType = logType,
                    logMessage.message,
                    logMessage.callerName,
                    logMessage.callerPath,
                    logMessage.lineNumber,
                    logMessage.parameters,
                    Timestamp = DateTime.UtcNow
                };

                string logJson = JsonSerializer.Serialize(logEntry);
                AppendToJsonFile(logJson);
            }
            catch (Exception)
            {
                // Silently swallow all errors — LogService must never throw.
            }
        }

        private static void AppendToJsonFile(string logJson)
        {
            try
            {
                // نتأكد أن المجلد موجود
                if (!Directory.Exists(logDirectory))
                {
                    try { Directory.CreateDirectory(logDirectory); }
                    catch (UnauthorizedAccessException) { /* silently fail if no write permission */ }
                    catch (IOException) { /* silently fail on other IO errors */ }
                }

                // لو الملف مش موجود، نكتبه مباشرة كـ مصفوفة فيها العنصر
                if (!File.Exists(logFilePath))
                {
                    File.WriteAllText(logFilePath, "[\n" + logJson + "\n]");
                    return;
                }

                // الملف موجود، نضيف عليه العنصر الجديد
                var existingContent = File.ReadAllText(logFilePath).Trim();

                // نحذف القوس النهائي "]" ونضيف العنصر الجديد
                if (existingContent.EndsWith("]"))
                {
                    existingContent = existingContent.Substring(0, existingContent.Length - 1).TrimEnd();

                    // إذا كان الملف يحتوي على عناصر بالفعل، نضيف فاصلة
                    if (existingContent.Length > 1)
                        existingContent += ",\n";

                    existingContent += logJson + "\n]";
                }
                else
                {
                    // حالة غير متوقعة: الملف موجود لكن مش منسق، نعيد كتابة مصفوفة جديدة
                    existingContent = "[\n" + logJson + "\n]";
                }

                File.WriteAllText(logFilePath, existingContent);
            }
            catch (Exception)
            {
                // Silently swallow all errors — LogService must never throw.
            }
        }

        //    private static void AppendToJsonFile(string logJson)
        //    {
        //        if (!File.Exists(logFilePath))
        //        {
        //            using (var stream = File.CreateText(logFilePath))
        //            {
        //                stream.WriteLine("[");
        //            }
        //        }

        //        var existingContent = File.ReadAllText(logFilePath).TrimEnd();
        //        if (existingContent.EndsWith("]"))
        //        {
        //            existingContent = existingContent.Remove(existingContent.Length - 1);
        //            existingContent += ",";
        //        }

        //        File.WriteAllText(logFilePath, existingContent);
        //        File.AppendAllText(logFilePath, logJson + Environment.NewLine + "]");
        //    }
        //}

        public class LogMessage
        {
            public LogMessage(string message, string callerName = "", string callerPath = "", int? lineNumber = 0, object[] parameters = null)
            {
                this.message = message;
                this.callerName = callerName;
                this.callerPath = callerPath;
                this.lineNumber = lineNumber;
                this.parameters = parameters;
            }

            public string message { get; set; }
            public string callerName { get; set; }
            public string callerPath { get; set; }
            public int? lineNumber { get; set; }
            public object[] parameters { get; set; }
        }
    }
}
