using System.Text;
using System.IO.Pipes;
namespace MiServicioWindows
{
    public static class NamedPipeHelper
    {
        public static async Task EnviarMensajeAsync(string mensaje)
        {
            using var pipeClient = new NamedPipeClientStream(".", "CanalNotificaciones", PipeDirection.Out);
            await pipeClient.ConnectAsync();
            using var writer = new StreamWriter(pipeClient, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync(mensaje);
        }
    }
}
