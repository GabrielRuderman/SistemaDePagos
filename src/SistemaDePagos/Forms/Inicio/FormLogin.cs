using System;
using System.Windows.Forms;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos
{
    public partial class FormLogin : Form
    {
        GestorDB gestor = GestorDB.GetInstance();
        BufferDB buffer = BufferDB.GetInstance();

        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "" || txtContrasena.Text == "")
                lblError.Visible = true;
            else
            {
                int retorno = gestor.LoginEsValido(txtUsuario.Text, txtContrasena.Text);
                switch(retorno)
                {
                    case -1:
                        break;
                    case 0:
                        lblError.Visible = true;
                        break;
                    default:
                        buffer.Permisos(retorno);
                        FormTablas formTablas = new FormTablas();
                        formTablas.Show();
                        this.Hide();
                        break;
                }
            }            
        }

        private void txtUsuario_KeyPress(object sender, KeyPressEventArgs e)
        {
            lblError.Visible = false;
        }

        private void txtContrasena_KeyPress(object sender, KeyPressEventArgs e)
        {
            lblError.Visible = false;
        }

        private void FormLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            Application.Exit();
        }

    }
}
