using Microsoft.Extensions.Logging;
using AprendeMasWindowsService.Communication;
using System;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Notifications
{
    public class NotificationManager
    {
        private readonly ILogger<NotificationManager> logger;
        private readonly NotifierPipeClient notifierPipeClient;
        private bool isRunning;

        public NotificationManager(ILogger<NotificationManager> logger)
        {
            this.logger = logger;
            notifierPipeClient = new NotifierPipeClient();
            isRunning = false;
        }

        public async Task StartAsync()
        {
            if (isRunning)
            {
                logger.LogWarning("NotificationManager ya está corriendo.");
                return;
            }

            try
            {
                isRunning = true;
                await notifierPipeClient.SendMessageAsync("START");
                logger.LogInformation("NotificationManager iniciado y comando START enviado.");
            }
            catch (Exception ex)
            {
                isRunning = false;
                logger.LogError(ex, "Error al iniciar el NotificationManager.");
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!isRunning)
            {
                logger.LogWarning("NotificationManager no está corriendo.");
                return;
            }

            try
            {
                isRunning = false;
                await notifierPipeClient.SendMessageAsync("STOP");
                logger.LogInformation("NotificationManager detenido y comando STOP enviado.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al detener el NotificationManager.");
                throw;
            }
        }

        public async Task SendNotificationAsync(string message)
        {
            if (!isRunning)
            {
                logger.LogWarning("NotificationManager no está corriendo. No se puede enviar notificación.");
                return;
            }

            try
            {
                await notifierPipeClient.SendMessageAsync(message);
                logger.LogInformation($"Notificación enviada: {message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error al enviar notificación: {message}");
                throw;
            }
        }
    }
}