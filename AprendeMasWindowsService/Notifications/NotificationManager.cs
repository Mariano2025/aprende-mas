using AprendeMasWindowsService.Communication;
using AprendeMasWindowsService.Utilities;
using System;
using System.Threading.Tasks;

// Define el espacio de nombres para la gestión de notificaciones del servicio de Windows
namespace AprendeMasWindowsService.Notifications
{
    // Clase que gestiona el envío y control de notificaciones
    public class NotificationManager
    {
        // Objeto para registrar logs de las operaciones
        private readonly Logger logger;
        // Cliente para enviar mensajes al servicio de notificaciones
        private readonly NotifierPipeClient notifierPipeClient;
        // Indica si el administrador de notificaciones está activo
        private bool isRunning;

        /// <summary>
        /// Constructor de la clase NotificationManager. Inicializa el logger y el cliente de pipe.
        /// </summary>
        public NotificationManager()
        {
            // Crea una nueva instancia del logger con el nombre del módulo y la ruta de los logs
            logger = new Logger("NotificationManager", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            // Crea una nueva instancia del cliente de pipe para notificaciones
            notifierPipeClient = new NotifierPipeClient();
            // Establece el estado inicial como no activo
            isRunning = false;
        }

        /// <summary>
        /// Inicia el administrador de notificaciones de forma asíncrona y envía el comando START.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="Exception">Se lanza si ocurre un error al iniciar el administrador.</exception>
        public async Task StartAsync()
        {
            // Verifica si el administrador ya está corriendo
            if (isRunning)
            {
                // Registra una advertencia si ya está activo
                logger.Warning("NotificationManager ya está corriendo.", nameof(StartAsync));
                return;
            }

            try
            {
                // Marca el administrador como activo
                isRunning = true;
                // Envía el comando START al servicio de notificaciones
                await notifierPipeClient.SendMessageAsync("START");
                // Registra en el log que el administrador se inició correctamente
                logger.Info("NotificationManager iniciado y comando START enviado.", nameof(StartAsync));
            }
            catch (Exception ex)
            {
                // Marca el administrador como no activo si ocurre un error
                isRunning = false;
                // Registra el error en el log
                logger.Error("Error al iniciar el NotificationManager.", ex, nameof(StartAsync));
                // Relanza la excepción para que el llamador la maneje
                throw;
            }
        }

        /// <summary>
        /// Detiene el administrador de notificaciones de forma asíncrona y envía el comando STOP.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="Exception">Se lanza si ocurre un error al detener el administrador.</exception>
        public async Task StopAsync()
        {
            // Verifica si el administrador no está corriendo
            if (!isRunning)
            {
                // Registra una advertencia si no está activo
                logger.Warning("NotificationManager no está corriendo.", nameof(StopAsync));
                return;
            }

            try
            {
                // Marca el administrador como no activo
                isRunning = false;
                // Envía el comando STOP al servicio de notificaciones
                await notifierPipeClient.SendMessageAsync("STOP");
                // Registra en el log que el administrador se detuvo correctamente
                logger.Info("NotificationManager detenido y comando STOP enviado.", nameof(StopAsync));
            }
            catch (Exception ex)
            {
                // Registra el error en el log
                logger.Error("Error al detener el NotificationManager.", ex, nameof(StopAsync));
                // Relanza la excepción para que el llamador la maneje
                throw;
            }
        }

        /// <summary>
        /// Envía una notificación al servicio de notificaciones de forma asíncrona.
        /// </summary>
        /// <param name="message">El mensaje de la notificación a enviar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="Exception">Se lanza si ocurre un error al enviar la notificación.</exception>
        public async Task SendNotificationAsync(string message)
        {
            // Verifica si el administrador no está corriendo
            if (!isRunning)
            {
                // Registra una advertencia si no está activo
                logger.Warning("NotificationManager no está corriendo. No se puede enviar notificación.", nameof(SendNotificationAsync));
                return;
            }

            try
            {
                // Envía el mensaje de notificación al servicio
                await notifierPipeClient.SendMessageAsync(message);
                // Registra en el log que la notificación se envió correctamente
                logger.Info($"Notificación enviada: {message}", nameof(SendNotificationAsync));
            }
            catch (Exception ex)
            {
                // Registra el error en el log con el mensaje que se intentó enviar
                logger.Error($"Error al enviar notificación: {message}", ex, nameof(SendNotificationAsync));
                // Relanza la excepción para que el llamador la maneje
                throw;
            }
        }
    }
}