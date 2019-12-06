using System;
using System.Windows.Forms;

namespace SistemaDePagos
{
    public partial class FormPresentacion : Form
    {
        Timer timerLogin, timerProgressBar;

        public FormPresentacion()
        {
            InitializeComponent();
        }

        private void FormPresentacion_Load(object sender, EventArgs e)
        {
            timerLogin = new Timer();
            timerLogin.Interval = 2000;
            timerLogin.Tick += new EventHandler(IniciarLogin);
            timerLogin.Start();

            pgbInicio.Maximum = 10;
            timerProgressBar = new Timer();
            timerProgressBar.Interval = 200;
            timerProgressBar.Tick += new EventHandler(RellenarProgressBar);
            timerProgressBar.Start();
        }

        private void IniciarLogin(object sender, EventArgs e)
        {
            timerLogin.Stop();
            timerProgressBar.Stop();
            new FormLogin().Show();
            this.Hide();
        }

        private void RellenarProgressBar(object sender, EventArgs e)
        {
            pgbInicio.Value++;
        }
    }
}
