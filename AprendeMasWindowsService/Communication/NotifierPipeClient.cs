using AprendeMasWindowsService.Utilities;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

// Define el espacio de nombres para la comunicación del servicio de Windows
namespace AprendeMasWindowsService.Communication
{
    // Clase que gestiona el envío de mensajes al servicio de notificaciones a través de un pipe nombrado
    public class NotifierPipeClient
    {
        // Objeto para registrar logs de las operaciones
        private readonly Logger logger;

        /// <summary>
        /// Constructor de la clase NotifierPipeClient. Inicializa el logger para registrar eventos.
        /// </summary>
        public NotifierPipeClient()
        {
            // Crea una nueva instancia del logger con el nombre del módulo y la ruta de los logs
            logger = new Logger("NotifierPipeClient", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
        }

        /// <summary>
        /// Envía un mensaje al servicio de notificaciones de forma asíncrona a través de un pipe nombrado.
        /// </summary>
        /// <param name="message">El mensaje a enviar al servicio de notificaciones.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <exception cref="Exception">Se lanza si no se puede conectar al pipe o si ocurre un error al enviar el mensaje.</exception>
        public async Task SendMessageAsync(string message)
        {
            try
            {
                // Crea un cliente de pipe nombrado para enviar mensajes al canal de notificaciones (NotificationService.cs)
                using var pipeClient = new NamedPipeClientStream(".", "CanalNotificaciones", PipeDirection.Out);
                // Intenta conectar al pipe con un tiempo de espera de 5 segundos
                await pipeClient.ConnectAsync(5000);
                // Crea un escritor para enviar datos al pipe con flush automático
                using var writer = new StreamWriter(pipeClient) { AutoFlush = true };
                // Escribe el mensaje en el pipe de forma asíncrona
                await writer.WriteLineAsync(message);
                // Registra en el log que el mensaje fue enviado exitosamente
                logger.Info($"Mensaje enviado al notificador: {message}", nameof(SendMessageAsync));
            }
            catch (Exception ex)
            {
                // Registra el error en el log con el mensaje que se intentó enviar
                logger.Error($"Error al enviar mensaje al notificador: {message}", ex, nameof(SendMessageAsync));
                // Relanza la excepción para que el llamador pueda manejarla
                throw;
            }
        }
    }
}