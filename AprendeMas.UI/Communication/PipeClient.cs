using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using AprendeMas.UI.Utilities;

// Define el espacio de nombres para las utilidades de la interfaz de usuario
namespace AprendeMas.UI.Helpers
{
    // Clase que gestiona la comunicación con el servicio AprendeMasService a través de un pipe nombrado
    public class PipeClient
    {
        // Nombre del pipe usado para la comunicación
        private const string PipeName = "AprendeMasPipe";
        // Tiempo de espera en segundos para conectar al pipe
        private const int ConnectionTimeoutSeconds = 10;

        // Objeto para registrar logs de las operaciones del PipeClient
        private readonly Logger logger;

        /// <summary>
        /// Constructor de la clase PipeClient. Inicializa el logger para registrar eventos.
        /// </summary>
        public PipeClient()
        {
            // Crea una nueva instancia del logger con el nombre del módulo y la ruta de los logs
            logger = new Logger("PipeClient", @"C:\Program Files (x86)\Aprende Mas\AprendeMas.UI");
        }

        /// <summary>
        /// Envía un comando al servicio a través de un pipe nombrado de forma síncrona.
        /// </summary>
        /// <param name="command">El comando a enviar al servicio.</param>
        /// <exception cref="Exception">Se lanza si no se puede conectar al servicio o si ocurre un error al enviar el comando.</exception>
        public void SendCommand(string command)
        {
            try
            {
                // Crea una nueva instancia del cliente de pipe nombrado para enviar datos
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Intenta conectar al pipe con un tiempo de espera definido
                    pipeClient.Connect(ConnectionTimeoutSeconds * 1000);

                    // Convierte el comando a un arreglo de bytes usando codificación UTF-8
                    byte[] buffer = Encoding.UTF8.GetBytes(command);

                    // Escribe el comando en el pipe
                    pipeClient.Write(buffer, 0, buffer.Length);
                    // Asegura que los datos se envíen inmediatamente
                    pipeClient.Flush();
                    // Registra en el log que el comando fue enviado exitosamente
                    logger.Info($"Comando enviado: {command}", nameof(SendCommand));
                }
            }
            catch (TimeoutException ex)
            {
                // Registra el error de tiempo de espera en el log
                logger.Error($"Error al conectar al servicio en: {command}", ex, nameof(SendCommand));
                // Lanza una excepción con un mensaje claro para el usuario
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
                // Registra cualquier otro error en el log
                logger.Error($"Error al conectar al servicio en: {command}", ex, nameof(SendCommand));
                // Lanza una excepción con detalles sobre el error
                throw new Exception($"Error al enviar el comando '{command}' al servicio: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Envía un comando al servicio a través de un pipe nombrado de forma asíncrona.
        /// </summary>
        /// <param name="command">El comando a enviar al servicio.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="Exception">Se lanza si no se puede conectar al servicio o si ocurre un error al enviar el comando.</exception>
        public async Task SendCommandAsync(string command)
        {
            try
            {
                // Crea una nueva instancia del cliente de pipe nombrado para enviar datos
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Conecta al pipe de forma asíncrona con un tiempo de espera
                    await pipeClient.ConnectAsync(ConnectionTimeoutSeconds * 1000);

                    // Convierte el comando a un arreglo de bytes usando codificación UTF-8
                    byte[] buffer = Encoding.UTF8.GetBytes(command);

                    // Escribe el comando en el pipe de forma asíncrona
                    await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                    // Asegura que los datos se envíen inmediatamente
                    await pipeClient.FlushAsync();
                    // Registra en el log que el comando fue enviado exitosamente
                    logger.Info($"Comando enviado: {command}", nameof(SendCommandAsync));
                }
            }
            catch (TimeoutException ex)
            {
                // Registra el error de tiempo de espera en el log
                logger.Error($"Error al enviar comando: {command}", ex, nameof(SendCommandAsync));
                // Lanza una excepción con un mensaje claro para el usuario
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
                // Registra cualquier otro error en el log
                logger.Error($"Error al enviar comando: {command}", ex, nameof(SendCommandAsync));
                // Lanza una excepción con detalles sobre el error
                throw new Exception($"Error al enviar el comando '{command}' al servicio: {ex.Message}", ex);
            }
        }
    }
}