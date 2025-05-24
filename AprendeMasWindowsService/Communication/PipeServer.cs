using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AprendeMasWindowsService.Communication
{
    public class PipeServer
    {
        private const string PipeName = "AprendeMasPipe";
        private CancellationTokenSource cancellationTokenSource;
        private bool isRunning;
        private readonly Action<string> commandHandler;

        public PipeServer(Action<string> commandHandler)
        {
            this.commandHandler = commandHandler;
        }

        // Inicia el servidor de pipes
        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            try
            {
                isRunning = true;
                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => ListenForCommandsAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al iniciar el servidor de pipes: {ex.Message}", ex);
            }
        }

        // Detiene el servidor de pipes
        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            try
            {
                isRunning = false;
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al detener el servidor de pipes: {ex.Message}", ex);
            }
        }

        // Escucha comandos de forma asíncrona
        private async Task ListenForCommandsAsync(CancellationToken cancellationToken)
        {
            while (isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        // Esperar conexión de un cliente
                        await pipeServer.WaitForConnectionAsync(cancellationToken);

                        // Leer el comando
                        byte[] buffer = new byte[1024];
                        int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                        // Procesar el comando
                        commandHandler?.Invoke(command);

                        // Desconectar el cliente
                        pipeServer.Disconnect();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Cancelación solicitada, salir del bucle
                    break;
                }
                catch (Exception ex)
                {
                    // Loguear el error (integrar con ILogger más adelante)
                }
            }
        }
    }
}