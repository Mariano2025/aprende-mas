using AprendeMasWindowsService.Utilities;
using System;
using System.IO.Pipes;
using System.Threading;

namespace AprendeMasWindowsService.Communication
{
    public class PipeServer
    {
        private readonly Logger logger;
        private readonly Action<string> commandHandler;
        private CancellationTokenSource cts;

        public PipeServer(Action<string> commandHandler)
        {
            logger = new Logger("PipeServer", @"C:\Program Files (x86)\Aprende Mas\AprendeMasWindowsService");
            this.commandHandler = commandHandler;
        }

        public void Start()
        {
            cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(_ => Listen(cts.Token));
            logger.Info("PipeServer iniciado.", nameof(Start));
        }

        public void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();
            logger.Info("PipeServer detenido.", nameof(Stop));
        }

        private void Listen(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream("AprendeMasPipe", PipeDirection.In);
                    pipeServer.WaitForConnection();

                    using var reader = new StreamReader(pipeServer);
                    string command = reader.ReadLine();

                    if (!string.IsNullOrEmpty(command))
                    {
                        logger.Info($"Comando recibido: {command}", nameof(Listen));
                        commandHandler?.Invoke(command);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error en PipeServer.", ex, nameof(Listen));
                }
            }
        }
    }
}