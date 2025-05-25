using AprendeMasWindowsService.Utilities;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Communication
{
    public class NotifierPipeClient
    {
        private readonly Logger logger;

        public NotifierPipeClient()
        {
            logger = new Logger("NotifierPipeClient", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", "CanalNotificaciones", PipeDirection.Out);
                await pipeClient.ConnectAsync(5000);
                using var writer = new StreamWriter(pipeClient) { AutoFlush = true };
                await writer.WriteLineAsync(message);
                logger.Info($"Mensaje enviado al notificador: {message}", nameof(SendMessageAsync));
            }
            catch (Exception ex)
            {
                logger.Error($"Error al enviar mensaje al notificador: {message}", ex, nameof(SendMessageAsync));
                throw;
            }
        }
    }
}