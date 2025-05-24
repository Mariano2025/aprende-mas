using System;
using System.ServiceProcess;
using AprendeMas.UI.Helpers;

namespace AprendeMas.UI.Helpers
{
    public class ServiceHelper
    {
        private readonly PipeClient pipeClient;
        private const string ServiceName = "AprendeMasWindowsService";
        private const string NotifierServiceName = "AprendeMasNotificationService";

        public ServiceHelper()
        {
            pipeClient = new PipeClient();
        }

        // Inicia el servicio y el notificador de forma sincronizada
        public void StartServiceAndNotifier()
        {
            try
            {
                // Iniciar el servicio de Windows
                using (var serviceController = new ServiceController(ServiceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Running)
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                }

                // Enviar comando al servicio para iniciar el notificador
                pipeClient.SendCommand("START");
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo iniciar el servicio o el notificador: {ex.Message}", ex);
            }
        }

        // Detiene el servicio y el notificador de forma sincronizada
        public void StopServiceAndNotifier()
        {
            try
            {
                // Enviar comando al servicio para detener el notificador
                pipeClient.SendCommand("STOP");

                // Detener el servicio de Windows
                using (var serviceController = new ServiceController(ServiceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Stopped)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo detener el servicio o el notificador: {ex.Message}", ex);
            }
        }
    }
}