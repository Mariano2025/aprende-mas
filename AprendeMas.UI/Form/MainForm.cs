using AprendeMas.UI.Utilities;
using System;
using System.Windows.Forms;
using AprendeMas.UI.Helpers;

// Define el espacio de nombres para la interfaz de usuario de la aplicaci�n AprendeMas
namespace AprendeMas.UI
{
    // Clase parcial que representa la ventana principal de la aplicaci�n
    public partial class MainForm : Form
    {
        // Cliente para comunicaci�n con el servicio a trav�s de un pipe
        private readonly PipeClient pipeClient;
        // Objeto para registrar logs de la aplicaci�n
        private readonly Logger logger;

        /// <summary>
        /// Constructor de la clase MainForm. Inicializa los componentes de la interfaz y los objetos necesarios.
        /// </summary>
        public MainForm()
        {
            // Inicializa los componentes de la interfaz gr�fica generados por el dise�ador
            InitializeComponent();
            // Crea una nueva instancia del cliente de pipe para comunicarse con el servicio
            pipeClient = new PipeClient();
            // Crea una nueva instancia del logger, especificando el nombre del m�dulo y la ruta de los logs
            logger = new Logger("UI", @"C:\Program Files (x86)\Aprende Mas\AprendeMas.UI");
        }

        /// <summary>
        /// Maneja el evento de clic en el bot�n para iniciar el servicio.
        /// Env�a el comando "START" al servicio y muestra un mensaje de �xito o error.
        /// </summary>
        /// <param name="sender">El objeto que desencaden� el evento (el bot�n).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStartService_Click(object sender, EventArgs e)
        {
            try
            {
                // Registra en el log que el bot�n de iniciar servicio fue presionado
                logger.Info("Bot�n Iniciar Servicio presionado.", nameof(btnStartService_Click));
                // Env�a el comando "START" al servicio de manera as�ncrona
                await pipeClient.SendCommandAsync("START");
                // Muestra un mensaje de �xito al usuario
                MessageBox.Show("Servicio iniciado correctamente.", "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Registra en el log que el comando fue enviado exitosamente
                logger.Info("Comando START enviado exitosamente.", nameof(btnStartService_Click));
            }
            catch (Exception ex)
            {
                // Registra el error en el log, incluyendo la excepci�n
                logger.Error("Error al iniciar el servicio.", ex, nameof(btnStartService_Click));
                // Muestra un mensaje de error al usuario con los detalles de la excepci�n
                MessageBox.Show($"Error al iniciar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Maneja el evento de clic en el bot�n para detener el servicio.
        /// Env�a el comando "STOP" al servicio y muestra un mensaje de �xito o error.
        /// </summary>
        /// <param name="sender">El objeto que desencaden� el evento (el bot�n).</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                // Registra en el log que el bot�n de detener servicio fue presionado
                logger.Info("Bot�n Detener Servicio presionado.", nameof(btnStopService_Click));
                // Env�a el comando "STOP" al servicio de manera as�ncrona
                await pipeClient.SendCommandAsync("STOP");
                // Muestra un mensaje de �xito al usuario
                MessageBox.Show("Servicio detenido correctamente.", "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Registra en el log que el comando fue enviado exitosamente
                logger.Info("Comando STOP enviado exitosamente.", nameof(btnStopService_Click));
            }
            catch (Exception ex)
            {
                // Registra el error en el log, incluyendo la excepci�n
                logger.Error("Error al detener el servicio.", ex, nameof(btnStopService_Click));
                // Muestra un mensaje de error al usuario con los detalles de la excepci�n
                MessageBox.Show($"Error al detener el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}