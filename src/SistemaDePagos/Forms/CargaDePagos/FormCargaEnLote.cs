using System;
using System.Windows.Forms;
using SistemaDePagos.Biblioteca;
using SistemaDePagos.Dominio;

namespace SistemaDePagos
{
    public partial class FormCargaEnLote : Form
    {
        private static BufferDB buffer = BufferDB.GetInstance();
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();
        private static ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();
        private static RellenadorDeFechas rellenadorDeFechas = RellenadorDeFechas.GetInstance();

        public FormCargaEnLote()
        {
            InitializeComponent();
        }

        private void FormCargaEnLote_Load(object sender, EventArgs e)
        {
            rellenadorDeDatos.LlenarCombo(cmbSucursal, buffer.Sucursales());
            rellenadorDeDatos.LlenarCombo(cmbRubro, buffer.Rubros());
            rellenadorDeFechas.LlenarMeses(cmbMesPrestacional);
            rellenadorDeFechas.LlenarAnos(cmbAnoPrestacional);
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            // Validar campos
            validadorDeDatos.NuevaValidacion("Faltaron completar los siguientes campos:\n");
            validadorDeDatos.ValidarCombo(cmbSucursal.Text, cmbSucursal.Items, "\n- SUCURSAL");
            validadorDeDatos.ValidarCombo(cmbRubro.Text, cmbRubro.Items, "\n- RUBRO");
            validadorDeDatos.ValidarCombo(cmbMesPrestacional.Text, cmbMesPrestacional.Items, "\n- MES PRESTACIONAL");
            validadorDeDatos.ValidarCombo(cmbAnoPrestacional.Text, cmbAnoPrestacional.Items, "\n- AÑO PRESTACIONAL");

            if (!validadorDeDatos.EstaCorrecto())
                MessageBox.Show(validadorDeDatos.MensajeFinal(), "Sistema de Pagos - SIDOM S.A.");
            else
            {
                string cadenaPeriodo = "01-" + cmbMesPrestacional.Text + "-" + cmbAnoPrestacional.Text;
                DateTime periodoPrestacional = Convert.ToDateTime(cadenaPeriodo);
                Pago pago = new Pago(periodoPrestacional, dtpFechaDePago.Checked, dtpFechaDePago.Value, cmbSucursal.Text, cmbRubro.Text);
                FormCargaPago formCargaPago = new FormCargaPago();
                formCargaPago.CargarLote(pago);
                formCargaPago.Show();
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
