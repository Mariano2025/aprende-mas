using AprendeMasWindowsService.Communication;
using AprendeMasWindowsService.Notifications;
using AprendeMasWindowsService.Utilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

// Define el espacio de nombres para el servicio principal de Windows
namespace AprendeMasWindowsService.Service
{
    // Clase que implementa el servicio en segundo plano para AprendeMas
    public class AprendeMasService : BackgroundService
    {
        // Objeto para registrar logs de las operaciones
        private readonly Logger logger;
        // Servidor de pipe para recibir comandos
        private readonly PipeServer pipeServer;
        // Administrador de notificaciones para enviar y controlar notificaciones
        private readonly NotificationManager notificationManager;

        /// <summary>
        /// Constructor de la clase AprendeMasService. Inicializa el logger, el servidor de pipe y el administrador de notificaciones.
        /// </summary>
        public AprendeMasService()
        {
            // Crea una nueva instancia del logger con el nombre del módulo y la ruta de los logs
            logger = new Logger("Service", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            // Crea un nuevo servidor de pipe con el manejador de comandos
            pipeServer = new PipeServer(HandleCommand);
            // Crea una nueva instancia del administrador de notificaciones
            notificationManager = new NotificationManager();
        }

        /// <summary>
        /// Ejecuta la lógica principal del servicio de forma asíncrona.
        /// </summary>
        /// <param name="stoppingToken">Token para cancelar la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Registra en el log que el servicio está iniciando
                logger.Info("Servicio AprendeMasWindowsService iniciando...", nameof(ExecuteAsync));

                // Inicia el servidor de pipe para escuchar comandos
                pipeServer.Start();

                // Mantiene el servicio activo hasta que se solicite cancelación
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Introduce un retraso de 1 segundo para evitar consumo excesivo de CPU
                    await Task.Delay(1000, stoppingToken);
                }

                // Registra en el log que el servicio se ha detenido
                logger.Info("Servicio AprendeMasWindowsService detenido.", nameof(ExecuteAsync));
            }
            catch (Exception ex)
            {
                // Registra cualquier error crítico en el servicio
                logger.Error("Error crítico en el servicio.", ex, nameof(ExecuteAsync));
                // Relanza la excepción para que el sistema la maneje
                throw;
            }
            finally
            {
                // Detiene el servidor de pipe al finalizar el servicio
                pipeServer.Stop();
            }
        }

        /// <summary>
        /// Inicia el servicio de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Registra en el log que el servicio está iniciando
            logger.Info("Iniciando AprendeMasWindowsService...", nameof(StartAsync));
            // Llama al método base para iniciar el servicio
            await base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Detiene el servicio de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Registra en el log que el servicio está deteniéndose
            logger.Info("Deteniendo AprendeMasWindowsService...", nameof(StopAsync));
            // Detiene el administrador de notificaciones
            await notificationManager.StopAsync();
            // Detiene el servidor de pipe
            pipeServer.Stop();
            // Llama al método base para detener el servicio
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Procesa los comandos recibidos a través del servidor de pipe.
        /// </summary>
        /// <param name="command">El comando recibido.</param>
        private async void HandleCommand(string command)
        {
            try
            {
                // Convierte el comando a mayúsculas para comparación
                switch (command.ToUpper())
                {
                    case "START":
                        // Registra en el log que se recibió el comando START
                        logger.Info("Recibido comando START. Iniciando NotificationManager...", nameof(HandleCommand));
                        // Inicia el administrador de notificaciones
                        await notificationManager.StartAsync();
                        break;

                    case "STOP":
                        // Registra en el log que se recibió el comando STOP
                        logger.Info("Recibido comando STOP. Deteniendo NotificationManager...", nameof(HandleCommand));
                        // Detiene el administrador de notificaciones
                        await notificationManager.StopAsync();
                        break;

                    default:
                        // Registra una advertencia para comandos desconocidos
                        logger.Warning($"Comando desconocido recibido: {command}", nameof(HandleCommand));
                        break;
                }
            }
            catch (Exception ex)
            {
                // Registra cualquier error al procesar el comando
                logger.Error($"Error al procesar el comando '{command}'.", ex, nameof(HandleCommand));
            }
        }
    }
}