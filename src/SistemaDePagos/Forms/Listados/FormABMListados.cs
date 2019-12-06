using System;
using System.Windows.Forms;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos
{
    public partial class FormABMListados : Form
    {
        private static BufferDB buffer = BufferDB.GetInstance();
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();
        private static GestorDB gestorDB = GestorDB.GetInstance();

        public FormABMListados()
        {
            InitializeComponent();
        }

        private void FormABMListados_Load(object sender, EventArgs e)
        {
            lblConexion.Text = "Conexión: " + gestorDB.ConnectionData();
            rellenadorDeDatos.LlenarListBox(lsbPersonas, buffer.Personas());
            rellenadorDeDatos.LlenarListBox(lsbRubros, buffer.Rubros());
            rellenadorDeDatos.LlenarListBox(lsbSucursales, buffer.Sucursales());
            rellenadorDeDatos.LlenarListBox(lsbMediosDePago, buffer.MediosDePago());
            rellenadorDeDatos.LlenarListBox(lsbBancos, buffer.Bancos());
        }

        private bool CargarItem(string procedure, string descripcion)
        {
            bool hubo_cambio = false;
            FormIngresoItem formIngresoItem = new FormIngresoItem(descripcion);
            if (formIngresoItem.ShowDialog(this) == DialogResult.OK)
            {
                gestorDB.PersistirItem(procedure, formIngresoItem.ItemIngresado());
                hubo_cambio = true;
            }
            formIngresoItem.Dispose();
            return hubo_cambio;
        }

        private bool QuitarItem(string procedure, string item)
        {
            DialogResult result = MessageBox.Show("¿Confirma la baja del ítem " + item + "?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                gestorDB.EliminarItem(procedure, item);
                return true;
            }
            return false;                
        }

        private bool ValidacionDeBaja(ListBox lsb)
        {
            if (lsb.SelectedItems.Count != 1)
            {
                MessageBox.Show("Debe seleccionar un ítem para eliminar.", "Sistema de Pagos - SIDOM S.A.");
                return false;
            }
            return true;
        }

        private void btnAgregarPersona_Click(object sender, EventArgs e)
        {
            if (this.CargarItem("crear_persona", "persona"))
            {
                buffer.RecargarPersonas();
                lsbPersonas.Items.Clear();
                rellenadorDeDatos.LlenarListBox(lsbPersonas, buffer.Personas());
            }
        }

        private void btnEliminarPersona_Click(object sender, EventArgs e)
        {
            if (this.ValidacionDeBaja(lsbPersonas))
            {
                if (this.QuitarItem("eliminar_persona", lsbPersonas.SelectedItem.ToString()))
                {
                    buffer.RecargarPersonas();
                    lsbPersonas.Items.Clear();
                    rellenadorDeDatos.LlenarListBox(lsbPersonas, buffer.Personas());
                }
            }
        }

        private void btnAgregarRubro_Click(object sender, EventArgs e)
        {
            if (this.CargarItem("crear_rubro", "rubro"))
            {
                buffer.RecargarRubros();
                lsbRubros.Items.Clear();
                rellenadorDeDatos.LlenarListBox(lsbRubros, buffer.Rubros());
            }
        }

        private void btnEliminarRubro_Click(object sender, EventArgs e)
        {
            if (this.ValidacionDeBaja(lsbRubros))
            {
                if (this.QuitarItem("eliminar_rubro", lsbRubros.SelectedItem.ToString()))
                {
                    buffer.RecargarRubros();
                    lsbRubros.Items.Clear();
                    rellenadorDeDatos.LlenarListBox(lsbRubros, buffer.Rubros());
                }
            }
        }

        private void btnAgregarSucursal_Click(object sender, EventArgs e)
        {
            if (this.CargarItem("crear_sucursal", "sucursal"))
            {
                buffer.RecargarSucursales();
                lsbSucursales.Items.Clear();
                rellenadorDeDatos.LlenarListBox(lsbSucursales, buffer.Sucursales());
            }
        }

        private void btnEliminarSucursal_Click(object sender, EventArgs e)
        {
            if (this.ValidacionDeBaja(lsbSucursales))
            {
                if (this.QuitarItem("eliminar_sucursal", lsbSucursales.SelectedItem.ToString()))
                {
                    buffer.RecargarSucursales();
                    lsbSucursales.Items.Clear();
                    rellenadorDeDatos.LlenarListBox(lsbSucursales, buffer.Sucursales());
                }
            }
        }

        private void btnAgregarMedioDePago_Click(object sender, EventArgs e)
        {
            if (this.CargarItem("crear_medio_de_pago", "sucursal"))
            {
                buffer.RecargarMediosDePago();
                lsbMediosDePago.Items.Clear();
                rellenadorDeDatos.LlenarListBox(lsbMediosDePago, buffer.MediosDePago());
            }
        }

        private void btnEliminarMedioDePago_Click(object sender, EventArgs e)
        {
            if (this.ValidacionDeBaja(lsbMediosDePago))
            {
                if (this.QuitarItem("eliminar_medio_de_pago", lsbMediosDePago.SelectedItem.ToString()))
                {
                    buffer.RecargarMediosDePago();
                    lsbMediosDePago.Items.Clear();
                    rellenadorDeDatos.LlenarListBox(lsbMediosDePago, buffer.MediosDePago());
                }
            }
        }

        private void btnAgregarBanco_Click(object sender, EventArgs e)
        {
            if (this.CargarItem("crear_banco", "banco"))
            {
                buffer.RecargarBancos();
                lsbBancos.Items.Clear();
                rellenadorDeDatos.LlenarListBox(lsbBancos, buffer.Bancos());
            }
        }

        private void btnEliminarBanco_Click(object sender, EventArgs e)
        {
            if (this.ValidacionDeBaja(lsbBancos))
            {
                if (this.QuitarItem("eliminar_banco", lsbBancos.SelectedItem.ToString()))
                {
                    buffer.RecargarBancos();
                    lsbBancos.Items.Clear();
                    rellenadorDeDatos.LlenarListBox(lsbBancos, buffer.Bancos());
                }
            }
        }

        private void pcbVolver_Click(object sender, EventArgs e)
        {
            FormTablas formPrincipal = (FormTablas) buffer.FormPrincipal();
            formPrincipal.ActualizarCombos();
            formPrincipal.Show();
            this.Hide();
        }

        private void FormABMListados_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.pcbVolver_Click(sender, e);
        }
    }
}
