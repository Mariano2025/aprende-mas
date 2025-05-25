using AprendeMas.UI.Utilities;
using System;
using System.Windows.Forms;
using AprendeMas.UI.Helpers;

// Define el espacio de nombres para la interfaz de usuario de la aplicación AprendeMas
namespace AprendeMas.UI
{
    // Clase parcial que representa la ventana principal de la aplicación
    public partial class MainForm : Form
    {
        // Cliente para comunicación con el servicio a través de un pipe
        private readonly PipeClient pipeClient;
        // Objeto para registrar logs de la aplicación
        private readonly Logger logger;

        /// <summary>
        /// Constructor de la clase MainForm. Inicializa los componentes de la interfaz y los objetos necesarios.
        /// </summary>
        public MainForm()
        {
            // Inicializa los componentes de la interfaz gráfica generados por el diseñador
            InitializeComponent();
            // Crea una nueva instancia del cliente de pipe para comunicarse con el servicio
            pipeClient = new PipeClient();
            // Crea una nueva instancia del logger, especificando el nombre del módulo y la ruta de los logs
            logger = new Logger("UI", @"C:\Program Files (x86)\Aprende Mas\AprendeMas.UI");
        }

        /// <summary>
        /// Maneja el evento de clic en el botón para iniciar el servicio.
        /// Envía el comando "START" al servicio y muestra un mensaje de éxito o error.
        /// </summary>
        /// <param name="sender">El objeto que desencadenó el evento (el botón).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStartService_Click(object sender, EventArgs e)
        {
            try
            {
                // Registra en el log que el botón de iniciar servicio fue presionado
                logger.Info("Botón Iniciar Servicio presionado.", nameof(btnStartService_Click));
                // Envía el comando "START" al servicio de manera asíncrona
                await pipeClient.SendCommandAsync("START");
                // Muestra un mensaje de éxito al usuario
                MessageBox.Show("Servicio iniciado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Registra en el log que el comando fue enviado exitosamente
                logger.Info("Comando START enviado exitosamente.", nameof(btnStartService_Click));
            }
            catch (Exception ex)
            {
                // Registra el error en el log, incluyendo la excepción
                logger.Error("Error al iniciar el servicio.", ex, nameof(btnStartService_Click));
                // Muestra un mensaje de error al usuario con los detalles de la excepción
                MessageBox.Show($"Error al iniciar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Maneja el evento de clic en el botón para detener el servicio.
        /// Envía el comando "STOP" al servicio y muestra un mensaje de éxito o error.
        /// </summary>
        /// <param name="sender">El objeto que desencadenó el evento (el botón).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                // Registra en el log que el botón de detener servicio fue presionado
                logger.Info("Botón Detener Servicio presionado.", nameof(btnStopService_Click));
                // Envía el comando "STOP" al servicio de manera asíncrona
                await pipeClient.SendCommandAsync("STOP");
                // Muestra un mensaje de éxito al usuario
                MessageBox.Show("Servicio detenido correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Registra en el log que el comando fue enviado exitosamente
                logger.Info("Comando STOP enviado exitosamente.", nameof(btnStopService_Click));
            }
            catch (Exception ex)
            {
                // Registra el error en el log, incluyendo la excepción
                logger.Error("Error al detener el servicio.", ex, nameof(btnStopService_Click));
                // Muestra un mensaje de error al usuario con los detalles de la excepción
                MessageBox.Show($"Error al detener el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}