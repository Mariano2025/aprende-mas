using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static NotifyIcon notifyIcon;

    [STAThread]
    private static async Task Main(string[] args)
    {
        // Iniciar notificaciones del sistema
        System.Windows.Forms.Application.EnableVisualStyles();
        notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Visible = true,
            BalloonTipTitle = "Servicio Activo",
            BalloonTipText = "Esperando notificaciones..."
        };

        notifyIcon.ShowBalloonTip(2000);

        Console.WriteLine("Cliente iniciado. Escuchando mensajes...");

        while (true)
        {
            try
            {
                using var pipeServer = new NamedPipeServerStream("CanalNotificaciones");
                await pipeServer.WaitForConnectionAsync();

                using var reader = new StreamReader(pipeServer);
                string mensaje = await reader.ReadLineAsync();

                if (!string.IsNullOrEmpty(mensaje))
                {
                    MostrarNotificacion(mensaje);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void MostrarNotificacion(string mensaje)
    {
        notifyIcon.BalloonTipTitle = "Notificación";
        notifyIcon.BalloonTipText = mensaje;
        notifyIcon.ShowBalloonTip(5000);
    }
}