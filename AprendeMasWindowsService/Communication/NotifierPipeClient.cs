using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Communication
{
    public class NotifierPipeClient
    {
        private const string NotifierPipeName = "CanalNotificaciones";

        // Envía un mensaje al notificador
        public async Task SendMessageAsync(string message)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", NotifierPipeName, PipeDirection.Out))
                {
                    await pipeClient.ConnectAsync(5000); // 5 segundos de tiempo de espera

                    using (var writer = new StreamWriter(pipeClient, Encoding.UTF8) { AutoFlush = true })
                    {
                        await writer.WriteLineAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el mensaje '{message}' al notificador: {ex.Message}", ex);
            }
        }
    }
}