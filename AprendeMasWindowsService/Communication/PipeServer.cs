using AprendeMasWindowsService.Utilities;
using System;
using System.IO.Pipes;
using System.Threading;

// Define el espacio de nombres para la comunicación del servicio de Windows
namespace AprendeMasWindowsService.Communication
{
    // Clase que gestiona un servidor de pipes nombrado para recibir comandos
    public class PipeServer
    {
        // Objeto para registrar logs de las operaciones
        private readonly Logger logger;
        // Acción que procesa los comandos recibidos
        private readonly Action<string> commandHandler;
        // Fuente de token para controlar la cancelación de operaciones
        private CancellationTokenSource cts;

        /// <summary>
        /// Constructor de la clase PipeServer. Inicializa el logger y el manejador de comandos.
        /// </summary>
        /// <param name="commandHandler">Acción que procesa los comandos recibidos.</param>
        public PipeServer(Action<string> commandHandler)
        {
            // Crea una nueva instancia del logger con el nombre del módulo y la ruta de los logs
            logger = new Logger("PipeServer", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            // Asigna el manejador de comandos proporcionado
            this.commandHandler = commandHandler;
        }

        /// <summary>
        /// Inicia el servidor de pipe para escuchar comandos en un hilo separado.
        /// </summary>
        public void Start()
        {
            // Crea una nueva fuente de token de cancelación
            cts = new CancellationTokenSource();
            // Inicia la escucha en un hilo del pool de hilos
            ThreadPool.QueueUserWorkItem(_ => Listen(cts.Token));
            // Registra en el log que el servidor de pipe ha iniciado
            logger.Info("PipeServer iniciado.", nameof(Start));
        }

        /// <summary>
        /// Detiene el servidor de pipe y libera recursos.
        /// </summary>
        public void Stop()
        {
            // Cancela la operación de escucha si existe un token
            cts?.Cancel();
            // Libera los recursos del token de cancelación
            cts?.Dispose();
            // Registra en el log que el servidor de pipe ha sido detenido
            logger.Info("PipeServer detenido.", nameof(Stop));
        }

        /// <summary>
        /// Escucha comandos entrantes de forma síncrona en un bucle hasta que se cancele.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        private void Listen(CancellationToken cancellationToken)
        {
            // Continúa escuchando mientras no se solicite cancelación
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Crea un servidor de pipe nombrado para recibir comandos
                    using var pipeServer = new NamedPipeServerStream("AprendeMasPipe", PipeDirection.In);
                    // Espera una conexión de un cliente
                    pipeServer.WaitForConnection();

                    // Crea un lector para leer datos del pipe
                    using var reader = new StreamReader(pipeServer);
                    // Lee el comando recibido
                    string command = reader.ReadLine();

                    // Procesa el comando si no está vacío
                    if (!string.IsNullOrEmpty(command))
                    {
                        // Registra en el log el comando recibido
                        logger.Info($"Comando recibido: {command}", nameof(Listen));
                        // Invoca el manejador de comandos si está definido
                        commandHandler?.Invoke(command);
                    }
                }
                catch (Exception ex)
                {
                    // Registra cualquier error durante la escucha o procesamiento
                    logger.Error("Error en PipeServer.", ex, nameof(Listen));
                }
            }
        }
    }
}