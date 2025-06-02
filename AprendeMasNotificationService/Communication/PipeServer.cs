using AprendeMasNotificationService.Utilities;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

// Define el espacio de nombres para la comunicación del servicio de notificaciones
namespace AprendeMasNotificationService.Communication
{
    /// <summary>
    /// Clase que gestiona un servidor de pipes nombrado para recibir mensajes.
    /// </summary>
    internal class PipeServer
    {
        // Nombre del pipe usado para la comunicación
        private readonly string _pipeName;
        // Objeto para registrar logs de las operaciones
        private readonly Logger _logger;
        // Acción que procesa los mensajes recibidos
        private readonly Action<string> _messageHandler;

        /// <summary>
        /// Inicializa una nueva instancia de PipeServer.
        /// </summary>
        /// <param name="pipeName">Nombre del pipe (por ejemplo, "CanalNotificaciones").</param>
        /// <param name="logger">Instancia de Logger para registrar eventos.</param>
        /// <param name="messageHandler">Acción que procesa los mensajes recibidos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si pipeName, logger o messageHandler son nulos.</exception>
        public PipeServer(string pipeName, Logger logger, Action<string> messageHandler)
        {
            // Valida que el nombre del pipe no sea nulo y lo asigna
            _pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            // Valida que el logger no sea nulo y lo asigna
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Valida que el manejador de mensajes no sea nulo y lo asigna
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        /// <summary>
        /// Escucha mensajes entrantes de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task ListenAsync(CancellationToken cancellationToken)
        {
            // Registra en el log que el servidor de pipe está iniciando
            _logger.Info($"Iniciando servidor de pipe: {_pipeName}", nameof(ListenAsync));

            // Continúa escuchando mientras no se solicite cancelación
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Crea un servidor de pipe nombrado para recibir mensajes
                    using var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In);
                    // Espera una conexión de forma asíncrona
                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    // Crea un lector para leer datos del pipe
                    using var reader = new StreamReader(pipeServer);
                    // Lee el mensaje recibido de forma asíncrona
                    string message = await reader.ReadLineAsync();

                    // Procesa el mensaje si no está vacío
                    if (!string.IsNullOrEmpty(message))
                    {
                        // Registra en el log el mensaje recibido
                        _logger.Info($"Mensaje recibido: {message}", nameof(ListenAsync));
                        // Invoca el manejador de mensajes para procesar el mensaje
                        _messageHandler(message);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Registra en el log que la escucha fue cancelada
                    _logger.Info("Escucha de mensajes cancelada.", nameof(ListenAsync));
                    // Sale del bucle al cancelar
                    break;
                }
                catch (Exception ex)
                {
                    // Registra cualquier error durante el procesamiento del mensaje
                    //_logger.Error("Error al procesar mensaje.", ex, nameof(ListenAsync));
                }
            }
        }
    }
}