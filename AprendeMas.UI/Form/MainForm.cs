using System;
using System.Windows.Forms;

namespace AprendeMas.UI
{
    public partial class MainForm : Form
    {
        private readonly Helpers.ServiceHelper serviceHelper;

        public MainForm()
        {
            InitializeComponent();
            serviceHelper = new Helpers.ServiceHelper();
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            try
            {
                serviceHelper.StartServiceAndNotifier();
                MessageBox.Show("Servicio iniciado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                serviceHelper.StopServiceAndNotifier();
                MessageBox.Show("Servicio detenido con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}