using AprendeMas.UI.Utilities;
using System;
using System.Windows.Forms;
using AprendeMas.UI.Helpers;

namespace AprendeMas.UI
{
    /// <summary>
    /// Clase que representa la ventana principal de la aplicación AprendeMas.
    /// Permite iniciar y detener un servicio mediante comandos enviados a través de un pipe.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly PipeClient pipeClient;
        private readonly Logger logger;

        /// <summary>
        /// Inicializa una nueva instancia de la ventana principal, configurando el cliente de pipe y el logger.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            pipeClient = new PipeClient();
            logger = new Logger("UI", @"C:\Program Files (x86)\Aprende Mas\AprendeMas.UI");
        }

        /// <summary>
        /// Inicia el servicio enviando el comando "START" a través del pipe.
        /// Muestra un mensaje de éxito o error al usuario.
        /// </summary>
        /// <param name="sender">Objeto que desencadenó el evento (botón).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStartService_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Info("Botón Iniciar Servicio presionado.", nameof(btnStartService_Click));
                await pipeClient.SendCommandAsync("START");
                MessageBox.Show("Servicio iniciado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                logger.Info("Comando START enviado exitosamente.", nameof(btnStartService_Click));
            }
            catch (Exception ex)
            {
                logger.Error("Error al iniciar el servicio.", ex, nameof(btnStartService_Click));
                MessageBox.Show($"Error al iniciar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Detiene el servicio enviando el comando "STOP" a través del pipe.
        /// Muestra un mensaje de éxito o error al usuario.
        /// </summary>
        /// <param name="sender">Objeto que desencadenó el evento (botón).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Info("Botón Detener Servicio presionado.", nameof(btnStopService_Click));
                await pipeClient.SendCommandAsync("STOP");
                MessageBox.Show("Servicio detenido correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                logger.Info("Comando STOP enviado exitosamente.", nameof(btnStopService_Click));
            }
            catch (Exception ex)
            {
                logger.Error("Error al detener el servicio.", ex, nameof(btnStopService_Click));
                MessageBox.Show($"Error al detener el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}