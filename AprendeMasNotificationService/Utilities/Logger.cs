using System;
using System.IO;

namespace AprendeMasNotificationService.Utilities
{
    public class Logger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public Logger(string componentName, string baseDirectory)
        {
            // Crear directorio de logs si no existe
            string logDirectory = Path.Combine(baseDirectory, "Logs");
            Directory.CreateDirectory(logDirectory);

            // Nombre del archivo: [Componente].log
            _logFilePath = Path.Combine(logDirectory, $"{componentName}.log");
        }

        public void Info(string message, string source = null)
        {
            Log("INFO", message, source);
        }

        public void Warning(string message, string source = null)
        {
            Log("WARNING", message, source);
        }

        public void Error(string message, Exception ex = null, string source = null)
        {
            string fullMessage = ex != null ? $"{message}: {ex.Message}\n{ex.StackTrace}" : message;
            Log("ERROR", fullMessage, source);
        }

        private void Log(string level, string message, string source)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = source != null
                ? $"[{timestamp}] {level} [{source}] {message}"
                : $"[{timestamp}] {level} {message}";

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, $"{logEntry}{Environment.NewLine}");
                }
                catch
                {
                    // Ignorar errores de escritura
                }
            }
        }
    }
}