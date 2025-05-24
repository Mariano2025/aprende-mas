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
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NotificationService.log");

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Leer estado inicial
            isListening = LoadConfig();
            Log($"Cliente iniciado. Escuchando mensajes: {isListening}");

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

            // Ejecutar bucle de mensajes de Windows Forms
            Application.Run();

            // Limpiar al salir
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
                        Log($"Mensaje recibido: {mensaje}");
                        HandleMessage(mensaje);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log("Escucha de mensajes cancelada.");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
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
                Log("Estado cambiado a START.");
            }
            else if (mensaje.Equals("STOP", StringComparison.OrdinalIgnoreCase))
            {
                isListening = false;
                SaveConfig();
                notifyIcon.BalloonTipTitle = "Servicio Pausado";
                notifyIcon.BalloonTipText = "Notificador activo, pero en pausa.";
                notifyIcon.ShowBalloonTip(2000);
                Log("Estado cambiado a STOP.");
            }
            else if (isListening)
            {
                // Mostrar notificación solo si está escuchando
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
                Log($"Error al cargar config: {ex.Message}");
            }
            return false; // Por defecto, no escuchar
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
                Log($"Error al guardar config: {ex.Message}");
            }
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignorar errores de logging
            }
        }
    }

    internal class Config
    {
        public bool IsListening { get; set; }
    }
}