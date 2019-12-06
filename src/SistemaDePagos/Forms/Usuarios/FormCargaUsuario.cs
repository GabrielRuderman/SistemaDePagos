using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos.Forms.Usuarios
{
    public partial class FormCargaUsuario : Form
    {
        GestorDB gestor = GestorDB.GetInstance();
        ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();
        int id_usuario;
        string nombre_usuario;
        List<string> lista_usuarios;

        public FormCargaUsuario(List<string> lista_usuarios)
        {
            InitializeComponent();
            this.lista_usuarios = lista_usuarios;
        }

        private void FormCargaUsuario_Load(object sender, EventArgs e)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta("SELECT * FROM SIDOM.permisos ORDER BY prioridad ASC");
            while (lector.Read())
                cmbPermisos.Items.Add(lector["prioridad"].ToString() + " - " + lector["descripcion"].ToString());
            gestor.Desconectar();
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            validadorDeDatos.NuevaValidacion("Faltaron completar los siguientes campos:\n");
            validadorDeDatos.ValidarAlfanumerico(txtUsuario.Text, "\n- USUARIO");
            validadorDeDatos.ValidarCombo(cmbPermisos.Text, cmbPermisos.Items, "\n- PERMISOS");
            if (!validadorDeDatos.EstaCorrecto())
                MessageBox.Show(validadorDeDatos.MensajeFinal(), "Sistema de Pagos - SIDOM S.A.");
            else if (validadorDeDatos.ItemRepetido(txtUsuario.Text, lista_usuarios))
                MessageBox.Show("Ya existe un usuario con ese nombre.", "Sistema de Pagos - SIDOM S.A.");
            else if (txtContrasena.Text != "" && !pcbTilde.Visible)
                MessageBox.Show("Las contraseñas no coinciden.", "SistemaDePagos de Pagos - SIDOM S.A.");
            else
            {
                string contrasena = txtContrasena.Text;
                string mensaje = "Usuario creado correctamente.";
                if (txtContrasena.Text == "")
                {
                    contrasena = "Pompy1234";
                    mensaje += "\n\nContraseña: " + contrasena;
                    DialogResult result = MessageBox.Show("Se configurará una contraseña por defecto. ¿Está seguro?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No) return;
                }
                gestor.AgregarUsuario(txtUsuario.Text, contrasena, Convert.ToInt32(cmbPermisos.Text.Substring(0, 1)));
                MessageBox.Show(mensaje, "Sistema de Pagos - SIDOM S.A.");
                this.DialogResult = DialogResult.OK;
            }
        }

        private void cmbPermisos_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void txtContrasena2_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtContrasena.Text == "")
                e.Handled = true;
            else
            {
                if (txtContrasena2.Text == txtContrasena.Text)
                    pcbTilde.Visible = true;
                else
                    pcbTilde.Visible = false;
            }
        }
    }
}
