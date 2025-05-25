using AprendeMasWindowsService.Communication;
using AprendeMasWindowsService.Utilities;
using System;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Notifications
{
    public class NotificationManager
    {
        private readonly Logger logger;
        private readonly NotifierPipeClient notifierPipeClient;
        private bool isRunning;

        public NotificationManager()
        {
            logger = new Logger("NotificationManager", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            notifierPipeClient = new NotifierPipeClient();
            isRunning = false;
        }

        public async Task StartAsync()
        {
            if (isRunning)
            {
                logger.Warning("NotificationManager ya está corriendo.", nameof(StartAsync));
                return;
            }

            try
            {
                isRunning = true;
                await notifierPipeClient.SendMessageAsync("START");
                logger.Info("NotificationManager iniciado y comando START enviado.", nameof(StartAsync));
            }
            catch (Exception ex)
            {
                isRunning = false;
                logger.Error("Error al iniciar el NotificationManager.", ex, nameof(StartAsync));
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!isRunning)
            {
                logger.Warning("NotificationManager no está corriendo.", nameof(StopAsync));
                return;
            }

            try
            {
                isRunning = false;
                await notifierPipeClient.SendMessageAsync("STOP");
                logger.Info("NotificationManager detenido y comando STOP enviado.", nameof(StopAsync));
            }
            catch (Exception ex)
            {
                logger.Error("Error al detener el NotificationManager.", ex, nameof(StopAsync));
                throw;
            }
        }

        public async Task SendNotificationAsync(string message)
        {
            if (!isRunning)
            {
                logger.Warning("NotificationManager no está corriendo. No se puede enviar notificación.", nameof(SendNotificationAsync));
                return;
            }

            try
            {
                await notifierPipeClient.SendMessageAsync(message);
                logger.Info($"Notificación enviada: {message}", nameof(SendNotificationAsync));
            }
            catch (Exception ex)
            {
                logger.Error($"Error al enviar notificación: {message}", ex, nameof(SendNotificationAsync));
                throw;
            }
        }
    }
}