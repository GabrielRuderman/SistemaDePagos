using System;
using System.Windows.Forms;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos
{
    public partial class FormIngresoItem : Form
    {
        private string descripcion;
        private static ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();

        public FormIngresoItem(string descripcion)
        {
            InitializeComponent();
            this.descripcion = descripcion;
        }

        private void FormIngresoItem_Load(object sender, EventArgs e)
        {
            lblTitulo.Text = "Ingrese nuevo ítem " + this.descripcion + ":";
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (txtItem.Text == "")
                MessageBox.Show("El ítem " + this.descripcion + " no puede estar vacío.", "Sistema de Pagos - SIDOM S.A.");
            else
                this.DialogResult = DialogResult.OK;
        }

        public string ItemIngresado()
        {
            return txtItem.Text;
        }

        private void txtItem_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterAlfanumericoConEspacio(e);
        }
    }
}
