using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using SistemaDePagos.Biblioteca;
using SistemaDePagos.Forms.Usuarios;

namespace SistemaDePagos
{
    public partial class FormABMUsuarios : Form
    {
        GestorDB gestor = GestorDB.GetInstance();
        BufferDB buffer = BufferDB.GetInstance();
        List<string> lista_usuarios = new List<string>();

        public FormABMUsuarios()
        {
            InitializeComponent();
        }

        private void FormABMUsuarios_Load(object sender, EventArgs e)
        {
            lsvUsuarios.View = View.Details;
            lsvUsuarios.Font = new Font("Verdana", 11, FontStyle.Bold);
            lsvUsuarios.Columns.Add("USUARIO");
            lsvUsuarios.Columns.Add("PERMISO");

            this.CargarListaUsuarios();

            lsvUsuarios.Columns[0].Width = (lsvUsuarios.Width * 4 / 7) - 2;
            lsvUsuarios.Columns[1].Width = (lsvUsuarios.Width * 3 / 7) - 2;
            //lsvUsuarios.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void pcbVolver_Click(object sender, EventArgs e)
        {
            buffer.FormPrincipal().Show();
            this.Hide();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            FormCargaUsuario formCargaUsuario = new FormCargaUsuario(this.lista_usuarios);
            if (formCargaUsuario.ShowDialog(this) == DialogResult.OK)
                this.CargarListaUsuarios();
            formCargaUsuario.Dispose();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (lsvUsuarios.SelectedItems.Count == 1)
            {
                int id_usuario = Convert.ToInt32(lsvUsuarios.SelectedItems[0].SubItems[2].Text);
                string nombre_usuario = lsvUsuarios.SelectedItems[0].SubItems[0].Text;
                string msg = "¿Confirma la baja del usuario " + nombre_usuario + "?";
                DialogResult result = MessageBox.Show(msg, "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    gestor.EliminarUsuario(id_usuario);
                    this.CargarListaUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un usuario.", "Sistema de Pagos - SIDOM S.A.");
            }
        }

        private void CargarListaUsuarios()
        {
            this.lista_usuarios.Clear();
            lsvUsuarios.Items.Clear();
            Font font_items = new Font("Verdana", 11);

            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(
                "SELECT U.id_usuario, U.nombre_de_usuario, P.descripcion " +
                "FROM SIDOM.usuarios U JOIN SIDOM.permisos P ON U.id_permiso = P.id_permiso " +
                "ORDER BY P.descripcion ASC, U.nombre_de_usuario ASC");
            while (lector.Read())
            {
                this.lista_usuarios.Add(lector["nombre_de_usuario"].ToString());
                ListViewItem item = new ListViewItem(lector["nombre_de_usuario"].ToString());
                item.SubItems.Add(lector["descripcion"].ToString());
                item.SubItems.Add(lector["id_usuario"].ToString());
                item.Font = font_items;
                lsvUsuarios.Items.Add(item);
            }
            gestor.Desconectar();
        }

        private void FormABMUsuarios_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.pcbVolver_Click(sender, e);
        }
    }
}
