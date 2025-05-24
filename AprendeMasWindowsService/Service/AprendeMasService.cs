using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AprendeMasWindowsService.Communication;
using AprendeMasWindowsService.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Service
{
    public class AprendeMasService : BackgroundService
    {
        private readonly ILogger<AprendeMasService> logger;
        private readonly PipeServer pipeServer;
        private readonly NotificationManager notificationManager;

        public AprendeMasService(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<AprendeMasService>();
            pipeServer = new PipeServer(HandleCommand);
            notificationManager = new NotificationManager(loggerFactory.CreateLogger<NotificationManager>());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Servicio AprendeMasWindowsService iniciando...");

                pipeServer.Start();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                logger.LogInformation("Servicio AprendeMasWindowsService detenido.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico en el servicio.");
                throw;
            }
            finally
            {
                pipeServer.Stop();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Iniciando AprendeMasWindowsService...");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Deteniendo AprendeMasWindowsService...");
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
                        logger.LogInformation("Recibido comando START. Iniciando NotificationManager...");
                        await notificationManager.StartAsync();
                        break;

                    case "STOP":
                        logger.LogInformation("Recibido comando STOP. Deteniendo NotificationManager...");
                        await notificationManager.StopAsync();
                        break;

                    default:
                        logger.LogWarning($"Comando desconocido recibido: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error al procesar el comando '{command}'.");
            }
        }
    }
}