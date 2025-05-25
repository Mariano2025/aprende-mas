using System;
using System.IO;

// Define el espacio de nombres para las utilidades del servicio de notificaciones
namespace AprendeMasNotificationService.Utilities
{
    // Clase que gestiona el registro de logs en archivos
    public class Logger
    {
        // Ruta del archivo de log
        private readonly string _logFilePath;
        // Objeto para sincronización en operaciones de escritura
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor de la clase Logger. Inicializa la ruta del archivo de log.
        /// </summary>
        /// <param name="componentName">Nombre del componente que genera los logs.</param>
        /// <param name="baseDirectory">Directorio base donde se almacenarán los logs.</param>
        public Logger(string componentName, string baseDirectory)
        {
            // Combina la ruta base con la carpeta "Logs" para crear el directorio de logs
            string logDirectory = Path.Combine(baseDirectory, "Logs");
            // Crea el directorio de logs si no existe
            Directory.CreateDirectory(logDirectory);

            // Define la ruta del archivo de log con el nombre del componente
            _logFilePath = Path.Combine(logDirectory, $"{componentName}.log");
        }

        /// <summary>
        /// Registra un mensaje de nivel INFO en el archivo de log.
        /// </summary>
        /// <param name="message">El mensaje a registrar.</param>
        /// <param name="source">El origen del mensaje (opcional).</param>
        public void Info(string message, string source = null)
        {
            // Llama al método Log con el nivel "INFO"
            Log("INFO", message, source);
        }

        /// <summary>
        /// Registra un mensaje de nivel WARNING en el archivo de log.
        /// </summary>
        /// <param name="message">El mensaje a registrar.</param>
        /// <param name="source">El origen del mensaje (opcional).</param>
        public void Warning(string message, string source = null)
        {
            // Llama al método Log con el nivel "WARNING"
            Log("WARNING", message, source);
        }

        /// <summary>
        /// Registra un mensaje de nivel ERROR en el archivo de log, con una excepción opcional.
        /// </summary>
        /// <param name="message">El mensaje a registrar.</param>
        /// <param name="ex">La excepción asociada al error (opcional).</param>
        /// <param name="source">El origen del mensaje (opcional).</param>
        public void Error(string message, Exception ex = null, string source = null)
        {
            // Incluye el mensaje de la excepción y su stack trace si se proporciona
            string fullMessage = ex != null ? $"{message}: {ex.Message}\n{ex.StackTrace}" : message;
            // Llama al método Log con el nivel "ERROR"
            Log("ERROR", fullMessage, source);
        }

        /// <summary>
        /// Escribe un mensaje en el archivo de log con el formato especificado.
        /// </summary>
        /// <param name="level">El nivel del log (INFO, WARNING, ERROR).</param>
        /// <param name="message">El mensaje a registrar.</param>
        /// <param name="source">El origen del mensaje (opcional).</param>
        private void Log(string level, string message, string source)
        {
            // Obtiene la marca de tiempo actual en formato específico
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            // Formatea la entrada de log, incluyendo el origen si se proporciona
            string logEntry = source != null
                ? $"[{timestamp}] {level} [{source}] {message}"
                : $"[{timestamp}] {level} {message}";

            // Usa un bloqueo para garantizar escritura segura en el archivo
            lock (_lock)
            {
                try
                {
                    // Escribe la entrada de log en el archivo, agregándola al final
                    File.AppendAllText(_logFilePath, $"{logEntry}{Environment.NewLine}");
                }
                catch
                {
                    // Ignora cualquier error de escritura para evitar interrumpir la aplicación
                }
            }
        }
    }
}