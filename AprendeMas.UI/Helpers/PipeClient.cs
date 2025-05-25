using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using AprendeMas.UI.Utilities;

namespace AprendeMas.UI.Helpers
{
    public class PipeClient
    {
        private const string PipeName = "AprendeMasPipe";
        private const int ConnectionTimeoutSeconds = 10;

        private readonly Logger logger;

        public PipeClient()
        {
            logger = new Logger("PipeClient", @"C:\Program Files (x86)\Aprende Mas\AprendeMas.UI");
        }

        // Envía un comando al servicio a través de un pipe nombrado
        public void SendCommand(string command)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Intentar conectar al pipe con un tiempo de espera
                    pipeClient.Connect(ConnectionTimeoutSeconds * 1000);

                    // Convertir el comando a bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(command);

                    // Enviar el comando
                    pipeClient.Write(buffer, 0, buffer.Length);
                    pipeClient.Flush();
                    logger.Info($"Comando enviado: {command}", nameof(SendCommand));
                }
            }
            catch (TimeoutException ex)
            {
                logger.Error($"Error al conectar al servicio en: {command}", ex, nameof(SendCommand));
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
                logger.Error($"Error al conectar al servicio en: {command}", ex, nameof(SendCommand));
                throw new Exception($"Error al enviar el comando '{command}' al servicio: {ex.Message}", ex);
            }
        }

        // Versión asíncrona para enviar comandos (opcional, para futura escalabilidad)
        public async Task SendCommandAsync(string command)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Conectar al pipe de forma asíncrona
                    await pipeClient.ConnectAsync(ConnectionTimeoutSeconds * 1000);

                    // Convertir el comando a bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(command);

                    // Enviar el comando de forma asíncrona
                    await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                    await pipeClient.FlushAsync();
                    logger.Info($"Comando enviado: {command}", nameof(SendCommandAsync));
                }
            }
            catch (TimeoutException ex)
            {
                logger.Error($"Error al enviar comando: {command}", ex, nameof(SendCommandAsync));
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
                logger.Error($"Error al enviar comando: {command}", ex, nameof(SendCommandAsync));
                throw new Exception($"Error al enviar el comando '{command}' al servicio: {ex.Message}", ex);
            }
        }
    }
}