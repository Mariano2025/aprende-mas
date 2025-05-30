using AprendeMasNotificationService.Communication;
using AprendeMasNotificationService.Utilities;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Define el espacio de nombres para el servicio de notificaciones
namespace AprendeMasNotificationService
{
    // Clase que gestiona el servicio de notificaciones en la bandeja del sistema
    internal class NotificationService
    {
        // Icono de notificación en la bandeja del sistema
        private static NotifyIcon notifyIcon;
        // Indica si el servicio está escuchando mensajes
        private static bool isListening;
        // Ruta del archivo de configuración
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        // Objeto para registrar logs de las operaciones
        private static readonly Logger logger = new Logger("NotificationService", @"C:\Program Files (x86)\Aprende Mas\AprendeMasNotificationService");
        // Servidor de pipe para recibir mensajes
        private static PipeServer pipeServer;

        /// <summary>
        /// Punto de entrada principal del servicio de notificaciones.
        /// Configura y ejecuta la aplicación de notificación.
        /// </summary>
        /// <param name="args">Argumentos de la línea de comandos (no utilizados).</param>
        [STAThread]
        private static void Main(string[] args)
        {
            // Habilita estilos visuales para la aplicación
            Application.EnableVisualStyles();
            // Configura el modo de renderizado de texto para compatibilidad
            Application.SetCompatibleTextRenderingDefault(false);

            // Carga la configuración inicial desde el archivo
            isListening = LoadConfig();
            // Registra en el log el estado inicial del cliente
            logger.Info($"Cliente iniciado. Escuchando mensajes: {isListening}", nameof(Main));

            // Inicializa el icono de la bandeja del sistema
            notifyIcon = new NotifyIcon
            {
                // Asigna un ícono predeterminado de información
                Icon = SystemIcons.Information,
                // Hace visible el ícono en la bandeja
                Visible = true,
                // Configura el título inicial del mensaje emergente
                BalloonTipTitle = "Servicio Activo",
                // Configura el texto inicial según el estado de escucha
                BalloonTipText = isListening ? "Escuchando notificaciones..." : "Notificador activo, pero en pausa."
            };
            // Muestra el mensaje emergente durante 2 segundos
            notifyIcon.ShowBalloonTip(2000);

            // Inicializa el servidor de pipe
            pipeServer = new PipeServer("CanalNotificaciones", logger, HandleMessage);

            // Crea un token de cancelación para controlar la escucha de mensajes
            var cts = new CancellationTokenSource();
            // Inicia la tarea asíncrona para escuchar mensajes
            var task = pipeServer.ListenAsync(cts.Token);

            // Ejecuta la aplicación de Windows Forms
            Application.Run();

            // Cancela la escucha de mensajes al cerrar la aplicación
            cts.Cancel();
            // Espera a que la tarea de escucha termine
            task.Wait();
            // Oculta el ícono de la bandeja
            notifyIcon.Visible = false;
            // Libera los recursos del ícono
            notifyIcon.Dispose();
        }

        /// <summary>
        /// Procesa los mensajes recibidos y actualiza el estado del servicio o muestra notificaciones.
        /// </summary>
        /// <param name="mensaje">El mensaje recibido a través del pipe.</param>
        private static void HandleMessage(string mensaje)
        {
            // Verifica si el mensaje es un comando "START" (ignorando mayúsculas)
            if (mensaje.Equals("START", StringComparison.OrdinalIgnoreCase))
            {
                // Activa la escucha de mensajes
                isListening = true;
                // Guarda el nuevo estado en el archivo de configuración
                SaveConfig();
                // Actualiza el título del mensaje emergente
                notifyIcon.BalloonTipTitle = "Servicio Activo";
                // Actualiza el texto del mensaje emergente
                notifyIcon.BalloonTipText = "Escuchando notificaciones...";
                // Muestra el mensaje emergente durante 1 segundos
                notifyIcon.ShowBalloonTip(1000);
                // Registra en el log el cambio de estado
                logger.Info("Estado cambiado a START.", nameof(HandleMessage));
            }
            // Verifica si el mensaje es un comando "STOP" (ignorando mayúsculas)
            else if (mensaje.Equals("STOP", StringComparison.OrdinalIgnoreCase))
            {
                // Desactiva la escucha de mensajes
                isListening = false;
                // Guarda el nuevo estado en el archivo de configuración
                SaveConfig();
                // Actualiza el título del mensaje emergente
                notifyIcon.BalloonTipTitle = "Servicio Pausado";
                // Actualiza el texto del mensaje emergente
                notifyIcon.BalloonTipText = "Notificador activo, pero en pausa.";
                // Muestra el mensaje emergente durante 2 segundos
                notifyIcon.ShowBalloonTip(2000);
                // Registra en el log el cambio de estado
                logger.Info("Estado cambiado a STOP.", nameof(HandleMessage));
            }
            // Procesa el mensaje como una notificación si está en modo escucha
            else if (isListening)
            {
                // Configura el título del mensaje emergente como notificación
                notifyIcon.BalloonTipTitle = "Notificación";
                // Usa el mensaje recibido como texto del mensaje emergente
                notifyIcon.BalloonTipText = mensaje;
                // Muestra el mensaje emergente durante 5 segundos
                notifyIcon.ShowBalloonTip(5000);
            }
        }

        /// <summary>
        /// Carga la configuración desde el archivo config.json.
        /// </summary>
        /// <returns>True si el servicio está configurado para escuchar, false en caso contrario.</returns>
        private static bool LoadConfig()
        {
            try
            {
                // Verifica si el archivo de configuración existe
                if (File.Exists(ConfigFilePath))
                {
                    // Lee el contenido del archivo de configuración
                    string json = File.ReadAllText(ConfigFilePath);
                    // Deserializa el JSON en un objeto Config
                    var config = JsonSerializer.Deserialize<Config>(json);
                    // Retorna el estado de escucha (o false si config es nulo)
                    return config?.IsListening ?? false;
                }
            }
            catch (Exception ex)
            {
                // Registra cualquier error al cargar la configuración
                logger.Error($"Error al cargar config.", ex, nameof(LoadConfig));
            }
            // Retorna false si no se pudo cargar la configuración
            return false;
        }

        /// <summary>
        /// Guarda la configuración actual en el archivo config.json.
        /// </summary>
        private static void SaveConfig()
        {
            try
            {
                // Crea un objeto Config con el estado actual de escucha
                var config = new Config { IsListening = isListening };
                // Serializa el objeto a JSON con formato indentado
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                // Escribe el JSON en el archivo de configuración
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                // Registra cualquier error al guardar la configuración
                logger.Error($"Error al guardar config.", ex, nameof(SaveConfig));
            }
        }
    }

    // Clase que representa la estructura del archivo de configuración
    internal class Config
    {
        // Propiedad que indica si el servicio está escuchando mensajes
        public bool IsListening { get; set; }
    }
}