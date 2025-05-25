using AprendeMasWindowsService.Communication;
using AprendeMasWindowsService.Notifications;
using AprendeMasWindowsService.Utilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Service
{
    public class AprendeMasService : BackgroundService
    {
        private readonly Logger logger;
        private readonly PipeServer pipeServer;
        private readonly NotificationManager notificationManager;

        public AprendeMasService()
        {
            logger = new Logger("Service", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            pipeServer = new PipeServer(HandleCommand);
            notificationManager = new NotificationManager();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.Info("Servicio AprendeMasWindowsService iniciando...", nameof(ExecuteAsync));

                pipeServer.Start();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                logger.Info("Servicio AprendeMasWindowsService detenido.", nameof(ExecuteAsync));
            }
            catch (Exception ex)
            {
                logger.Error("Error crítico en el servicio.", ex, nameof(ExecuteAsync));
                throw;
            }
            finally
            {
                pipeServer.Stop();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Info("Iniciando AprendeMasWindowsService...", nameof(StartAsync));
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Info("Deteniendo AprendeMasWindowsService...", nameof(StopAsync));
            await notificationManager.StopAsync();
            pipeServer.Stop();
            await base.StopAsync(cancellationToken);
        }

        private async void HandleCommand(string command)
        {
            try
            {
                switch (command.ToUpper())
                {
                    case "START":
                        logger.Info("Recibido comando START. Iniciando NotificationManager...", nameof(HandleCommand));
                        await notificationManager.StartAsync();
                        break;

                    case "STOP":
                        logger.Info("Recibido comando STOP. Deteniendo NotificationManager...", nameof(HandleCommand));
                        await notificationManager.StopAsync();
                        break;

                    default:
                        logger.Warning($"Comando desconocido recibido: {command}", nameof(HandleCommand));
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error al procesar el comando '{command}'.", ex, nameof(HandleCommand));
            }
        }
    }
}