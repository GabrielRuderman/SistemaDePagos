using System;
using System.Windows.Forms;
using SistemaDePagos.Dominio;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos
{
    public partial class FormCargaPago : Form
    {
        private Pago pago;
        private bool nuevo;
        private static BufferDB buffer = BufferDB.GetInstance();
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();
        private static ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();
        private static RellenadorDeFechas rellenadorDeFechas = RellenadorDeFechas.GetInstance();
        private static GestorDB gestor = GestorDB.GetInstance();

        public FormCargaPago()
        {
            InitializeComponent();
        }

        public void CargarLote(Pago pago)
        {
            // Se cargan los datos para un nuevo pago

            this.pago = pago;
            cmbSucursal.Text = pago.Sucursal();
            cmbRubro.Text = pago.Rubro();
            cmbMesPrestacional.Text = rellenadorDeFechas.MesATexto(pago.PeriodoPrestacionReal().Month);
            cmbAnoPrestacional.Text = pago.PeriodoPrestacionReal().Year.ToString();
            ckbFechaDePago.Checked = pago.FechaPagoRealChecked();
            this.ckbFechaDePago_CheckedChanged(null, null);
            dtpFechaDePago.Value = pago.FechaPagoReal();

            this.nuevo = true;
            btnSiguiente.Enabled = false;
            txtNumeroDeFactura.Select();
        }

        public void CargarPago(Pago pago)
        {
            // Se cargan los datos de un pago existente (modificación)

            this.CargarLote(pago);
            txtNumeroDeFactura.Text = pago.NumeroDeFactura();
            cmbMedioDePago.Text = pago.MedioDePago();
            cmbBanco.Text = pago.Banco();
            txtNumeroDeCheque.Text = pago.NumeroDeCheque().ToString();
            if (txtNumeroDeCheque.Text == "0") txtNumeroDeCheque.Text = ""; // Número de cheque
            txtMonto.Text = pago.Monto().ToString();
            txtRetencionesGanancias.Text = pago.RetencionesGanancias().ToString();
            txtRetencionesIngresosBrutosCaba.Text = pago.RetencionesIngresosBrutosCaba().ToString();
            txtRetencionesIngresosBrutosProvincia.Text = pago.RetencionesIngresosBrutosProvincia().ToString();
            cmbPersona.Text = pago.Persona();
            txtObservaciones.Text = pago.Observaciones();

            switch (cmbMedioDePago.Text)
            {
                case "EFECTIVO":
                    cmbBanco.Enabled = false;
                    txtNumeroDeCheque.Enabled = false;
                    break;
                case "TRANSFERENCIA":
                    cmbBanco.Enabled = true;
                    txtNumeroDeCheque.Enabled = false;
                    break;
                default:
                    cmbBanco.Enabled = true;
                    txtNumeroDeCheque.Enabled = true;
                    break;
            }

            this.nuevo = false;
            btnSiguiente.Enabled = true;
            cmbSucursal.Select();
        }

        private void FormCargaPago_Load(object sender, EventArgs e)
        {
            rellenadorDeDatos.LlenarCombo(cmbSucursal, buffer.Sucursales());
            rellenadorDeDatos.LlenarCombo(cmbRubro, buffer.Rubros());
            rellenadorDeFechas.LlenarMeses(cmbMesPrestacional);
            rellenadorDeFechas.LlenarAnos(cmbAnoPrestacional);
            rellenadorDeDatos.LlenarCombo(cmbMedioDePago, buffer.MediosDePago());
            rellenadorDeDatos.LlenarCombo(cmbBanco, buffer.Bancos());
            rellenadorDeDatos.LlenarCombo(cmbPersona, buffer.Personas());
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            // --------------------------------- Validar campos ---------------------------------
            // TODO: validar longitud campos
            validadorDeDatos.NuevaValidacion("Debe completar o corregir los siguientes campos:\n");
            validadorDeDatos.ValidarCombo(cmbMesPrestacional.Text, cmbMesPrestacional.Items, "\n- MES PRESTACIONAL");
            validadorDeDatos.ValidarCombo(cmbAnoPrestacional.Text, cmbAnoPrestacional.Items, "\n- AÑO PRESTACIONAL");
            validadorDeDatos.ValidarCombo(cmbSucursal.Text, cmbSucursal.Items, "\n- SUCURSAL");
            validadorDeDatos.ValidarCombo(cmbRubro.Text, cmbRubro.Items, "\n- RUBRO");

            validadorDeDatos.ValidarAlfanumerico(txtNumeroDeFactura.Text, "\n- NÚMERO DE FACTURA");
            validadorDeDatos.ValidarCombo(cmbMedioDePago.Text, cmbMedioDePago.Items, "\n- MEDIO DE PAGO");
            if (cmbBanco.Enabled) validadorDeDatos.ValidarCombo(cmbBanco.Text, cmbBanco.Items, "\n- BANCO");
            if (txtNumeroDeCheque.Enabled) validadorDeDatos.ValidarNumericoObligatorio(txtNumeroDeCheque.Text, "\n- NÚMERO DE CHEQUE");
            validadorDeDatos.ValidarNumericoObligatorio(txtMonto.Text, "\n- MONTO");
            validadorDeDatos.ValidarNumericoOpcional(txtRetencionesGanancias.Text, "\n- RETENCIÓN DE GANANCIAS");
            validadorDeDatos.ValidarNumericoOpcional(txtRetencionesIngresosBrutosCaba.Text, "\n- RETENCIÓN POR INGRESOS BRUTOS EN CABA");
            validadorDeDatos.ValidarNumericoOpcional(txtRetencionesIngresosBrutosProvincia.Text, "\n- RETENCIÓN POR INGRESOS BRUTOS EN PROVINCIA");
            validadorDeDatos.ValidarCombo(cmbPersona.Text, cmbPersona.Items, "\n- PAGADO A");
            // ----------------------------------------------------------------------------------

            if (!validadorDeDatos.EstaCorrecto())
                MessageBox.Show(validadorDeDatos.MensajeFinal(), "Sistema de Pagos - SIDOM S.A.");
            else
            {
                string cadenaPeriodo = "01-" + cmbMesPrestacional.Text + "-" + cmbAnoPrestacional.Text;
                pago.PeriodoPrestacionReal(Convert.ToDateTime(cadenaPeriodo));
                pago.FechaPagoRealChecked(ckbFechaDePago.Checked);
                pago.FechaPagoReal(dtpFechaDePago.Value);
                pago.Sucursal(cmbSucursal.Text.ToUpper());
                pago.Rubro(cmbRubro.Text.ToUpper());

                pago.NumeroDeFactura(txtNumeroDeFactura.Text.ToUpper());
                pago.MedioDePago(cmbMedioDePago.Text.ToUpper());
                pago.Banco(cmbBanco.Text.ToUpper());
                if (txtNumeroDeCheque.Enabled) pago.NumeroDeCheque(Convert.ToInt32(txtNumeroDeCheque.Text));
                pago.Monto(validadorDeDatos.ObtenerFormatoDecimal(txtMonto.Text));
                if (txtRetencionesGanancias.Text != "") pago.RetencionesGanancias(validadorDeDatos.ObtenerFormatoDecimal(txtRetencionesGanancias.Text));
                if (txtRetencionesIngresosBrutosCaba.Text != "") pago.RetencionesIngresosBrutosCaba(validadorDeDatos.ObtenerFormatoDecimal(txtRetencionesIngresosBrutosCaba.Text));
                if (txtRetencionesIngresosBrutosProvincia.Text != "") pago.RetencionesIngresosBrutosProvincia(validadorDeDatos.ObtenerFormatoDecimal(txtRetencionesIngresosBrutosProvincia.Text));
                pago.Persona(cmbPersona.Text.ToUpper());
                pago.Observaciones(txtObservaciones.Text.ToUpper());

                string confirmacion = "";
                MessageBoxIcon icono = MessageBoxIcon.None;
                if (!validadorDeDatos.FacturaEsCorrecta(pago) && this.nuevo)
                {
                    confirmacion += "Advertencia: ya se encuentra registrada la factura " + pago.NumeroDeFactura().ToString() + " para " + pago.Persona() + ".\n\n";
                    icono = MessageBoxIcon.Warning;
                }
                double ultimo_monto = 0;
                if (!validadorDeDatos.MontoEsAcorde(pago, ref ultimo_monto))
                {
                    DateTime periodo_anterior = pago.PeriodoPrestacionReal().AddMonths(-1);
                    confirmacion += "Advertencia: " + pago.Persona() + " (sucursal: " + pago.Sucursal() + " - rubro: " + pago.Rubro() + ") tiene registrado un pago con un valor de $" + validadorDeDatos.ObtenerFormatoMoneda(ultimo_monto) + " en el período anterior (" + rellenadorDeFechas.MesATexto(periodo_anterior.Month) + " " + periodo_anterior.Year.ToString() + ").\n\n";
                    icono = MessageBoxIcon.Warning;
                }

                DialogResult result = MessageBox.Show(confirmacion += "¿Confirma los datos del pago?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo, icono);
                if (result == DialogResult.No) return;

                try
                {
                    gestor.PersistirPago(pago, this.nuevo);
                    this.ConsultarAccionSiguiente(sender, e);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error: envíe el detalle del mismo a sistemas@sidom.com.ar\n\nDETALLE:\n" + exc, "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ConsultarAccionSiguiente(object sender, EventArgs e)
        {
            string mensaje;
            if (this.nuevo)
                mensaje = "Pago registrado exitosamente.\n¿Desea cargar otro pago?";
            else
                mensaje = "Pago modificado exitosamente.\n¿Desea seguir modificando pagos?";
            DialogResult result2 = MessageBox.Show(mensaje, "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result2 == DialogResult.Yes)
                if (this.nuevo)
                {
                    this.pago = new Pago();
                    this.btnLimpiar_Click(sender, e);
                }
                else
                    this.btnSiguiente_Click(sender, e);
            else
                this.VolverFormPrincipal(true);
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            gestor.Conectar();
            bool existe_id = gestor.Consulta("SELECT id_pago FROM SIDOM.pagos WHERE id_pago = " + (this.pago.Id() + 1).ToString()).Read();
            gestor.Desconectar();
            if (existe_id)
            {
                FormCargaPago formCargaPago = new FormCargaPago();
                formCargaPago.CargarPago(new Pago(this.pago.Id() + 1)); // Pasa al próximo pago según el ID
                formCargaPago.Show();
                this.Hide();
            }
            else
                MessageBox.Show("Actualmente se encuentra en el último pago registrado.", "Sistema de Pagos - SIDOM S.A.");
        }

        private void VolverFormPrincipal(bool cambio)
        {
            FormTablas formPrincipal = (FormTablas)buffer.FormPrincipal();
            if (cambio) formPrincipal.ActualizacionExternaTablaGeneral();
            formPrincipal.Show();
            this.Hide();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.VolverFormPrincipal(false);
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNumeroDeFactura.Text = "";
            cmbMedioDePago.Text = "";
            cmbBanco.Text = "";
            cmbBanco.Enabled = true;
            txtNumeroDeCheque.Text = "";
            txtNumeroDeCheque.Enabled = true;
            txtMonto.Text = "";
            txtRetencionesGanancias.Text = "";
            txtRetencionesIngresosBrutosCaba.Text = "";
            txtRetencionesIngresosBrutosProvincia.Text = "";
            cmbPersona.Text = "";
            txtObservaciones.Text = "";
            txtNumeroDeFactura.Select();
        }

        private void ckbFechaDePago_CheckedChanged(object sender, EventArgs e)
        {
            dtpFechaDePago.Visible = ckbFechaDePago.Checked;
            dtpFechaDePago.Value = DateTime.Today;
        }

        private void cmbMedioDePago_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(cmbMedioDePago.Text)
            {
                case "EFECTIVO":
                    cmbBanco.Text = "";
                    cmbBanco.Enabled = false;
                    txtNumeroDeCheque.Text = "";
                    txtNumeroDeCheque.Enabled = false;
                    break;
                case "TRANSFERENCIA":
                    cmbBanco.Enabled = true;
                    txtNumeroDeCheque.Text = "";
                    txtNumeroDeCheque.Enabled = false;
                    break;
                default:
                    cmbBanco.Enabled = true;
                    txtNumeroDeCheque.Enabled = true;
                    break;
            }
        }

        private void txtNumeroDeFactura_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterAlfanumerico(e);
        }

        private void txtNumeroDeCheque_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterNumerico(e);
        }

        private void txtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterNumerico(e);
        }

        private void txtRetencionesGanancias_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterNumerico(e);
        }

        private void txtRetencionesIngresosBrutosCaba_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterNumerico(e);
        }

        private void txtRetencionesIngresosBrutosProvincia_KeyPress(object sender, KeyPressEventArgs e)
        {
            validadorDeDatos.CaracterNumerico(e);
        }

        private void FormCargaPago_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.btnCancelar_Click(sender, e);
        }
    }
}
