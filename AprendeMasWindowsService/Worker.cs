using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MiServicioWindows
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Servicio MiServicioWindows iniciando...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Enviar notificación cada 10 segundos
                        await NamedPipeHelper.EnviarMensajeAsync("¡Hola desde el servicio!");
                        _logger.LogInformation("Mensaje enviado correctamente.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar mensaje a través del pipe.");
                    }
                    await Task.Delay(10000, stoppingToken);
                }

                _logger.LogInformation("Servicio MiServicioWindows detenido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el servicio.");
                throw; // Relanzar para que el host maneje el error
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando MiServicioWindows...");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deteniendo MiServicioWindows...");
            await base.StopAsync(cancellationToken);
        }
    }
}