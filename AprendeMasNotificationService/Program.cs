using AprendeMasNotificationService.Utilities;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AprendeMasNotificationService
{
    internal class Program
    {
        private static NotifyIcon notifyIcon;
        private static bool isListening;
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private static readonly Logger logger = new Logger("NotificationService", @"C:\Program Files (x86)\Aprende Mas\AprendeMasNotificationService");

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            isListening = LoadConfig();
            logger.Info($"Cliente iniciado. Escuchando mensajes: {isListening}", nameof(Main));

            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                BalloonTipTitle = "Servicio Activo",
                BalloonTipText = isListening ? "Escuchando notificaciones..." : "Notificador activo, pero en pausa."
            };
            notifyIcon.ShowBalloonTip(2000);

            var cts = new CancellationTokenSource();
            var task = ListenForMessagesAsync(cts.Token);

            Application.Run();

            cts.Cancel();
            task.Wait();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private static async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream("CanalNotificaciones", PipeDirection.In);
                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    using var reader = new StreamReader(pipeServer);
                    string mensaje = await reader.ReadLineAsync();

                    if (!string.IsNullOrEmpty(mensaje))
                    {
                        logger.Info($"Mensaje recibido: {mensaje}", nameof(ListenForMessagesAsync));
                        HandleMessage(mensaje);
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.Info("Escucha de mensajes cancelada.", nameof(ListenForMessagesAsync));
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error al procesar mensaje.", ex, nameof(ListenForMessagesAsync));
                }
            }
        }

        private static void HandleMessage(string mensaje)
        {
            if (mensaje.Equals("START", StringComparison.OrdinalIgnoreCase))
            {
                isListening = true;
                SaveConfig();
                notifyIcon.BalloonTipTitle = "Servicio Activo";
                notifyIcon.BalloonTipText = "Escuchando notificaciones...";
                notifyIcon.ShowBalloonTip(2000);
                logger.Info("Estado cambiado a START.", nameof(HandleMessage));
            }
            else if (mensaje.Equals("STOP", StringComparison.OrdinalIgnoreCase))
            {
                isListening = false;
                SaveConfig();
                notifyIcon.BalloonTipTitle = "Servicio Pausado";
                notifyIcon.BalloonTipText = "Notificador activo, pero en pausa.";
                notifyIcon.ShowBalloonTip(2000);
                logger.Info("Estado cambiado a STOP.", nameof(HandleMessage));
            }
            else if (isListening)
            {
                notifyIcon.BalloonTipTitle = "Notificación";
                notifyIcon.BalloonTipText = mensaje;
                notifyIcon.ShowBalloonTip(5000);
            }
        }

        private static bool LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<Config>(json);
                    return config?.IsListening ?? false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error al cargar config.", ex, nameof(LoadConfig));
            }
            return false;
        }

        private static void SaveConfig()
        {
            try
            {
                var config = new Config { IsListening = isListening };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                logger.Error($"Error al guardar config.", ex, nameof(SaveConfig));
            }
        }
    }

    internal class Config
    {
        public bool IsListening { get; set; }
    }
}