using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace AprendeMas.UI.Helpers
{
    public class PipeClient
    {
        private const string PipeName = "AprendeMasPipe";
        private const int ConnectionTimeoutSeconds = 10;

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
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
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
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"No se pudo conectar al servicio en {ConnectionTimeoutSeconds} segundos.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el comando '{command}' al servicio: {ex.Message}", ex);
            }
        }
    }
}