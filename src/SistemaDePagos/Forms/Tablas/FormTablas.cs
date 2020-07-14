using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SistemaDePagos.Forms.Proyectado;
using SistemaDePagos.Biblioteca;
using SistemaDePagos.Dominio;

namespace SistemaDePagos
{
    public partial class FormTablas : Form
    {
        private DataGridViewClonable TablaGeneralOriginal;
        private static GestorDB gestor = GestorDB.GetInstance();
        private static BufferDB buffer = BufferDB.GetInstance();
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();
        private static RellenadorDeFechas rellenadorDeFechas = RellenadorDeFechas.GetInstance();
        private static FuncionesPolimorficas funcionesPolimorficas = FuncionesPolimorficas.GetInstance();
        private List<Tuple<string, string, bool>> nombre_columnas = new List<Tuple<string, string, bool>>();
        private Tuple<string, string, bool> tupla_actual = null;
        private string query_general, query_actual, filtro_actual;
        private int pagina_actual, total_registros, total_paginas, registros_por_pagina = 100;
        private int cant_periodos_sucursal = 4, cant_periodos_rubro = 4;
        private const string todas_suc_en_rubros = "TODAS LAS SUCURSALES";
        private string criterio_totxrub = "", criterio_totxsuc = "", criterio_rubxsuc = "";
        private string[] criterios_cmb = new string[2] { "SEGÚN FECHA DE PAGO REAL", "SEGÚN PERÍODO PRESTACIONAL" };
        private string[] criterios_query = new string[2] { "fecha_pago_real", "periodo_prestacion_real" };
        private enum IndiceCriterio : int { FechaDePagoReal = 0, PeriodoPrestacional = 1}
        private bool selectionbar_pxs_activa = false, selectionbar_pxr_activa = false, selectionbar_rxs_activa = false;

        public FormTablas()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");

            this.InicializarTablaInicial();
        }

        private void tbcPagos_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbcPagos.SelectedTab.Hide();
            tbcPagos.SelectedTab.Show();
            if (!this.selectionbar_pxs_activa) dgvPagosPorSucursal.CurrentCell = null;
            if (!this.selectionbar_pxr_activa) dgvPagosPorRubro.CurrentCell = null;
            if (!this.selectionbar_rxs_activa) dgvPagosRubrosPorSucursal.CurrentCell = null;
        }

        private void AsignarCriterio(ComboBox cmb, ref string campo_query, IndiceCriterio ind)
        {
            cmb.Text = criterios_cmb[(int) ind];
            campo_query = criterios_query[(int) ind];
        }

        public void InicializarTablaInicial()
        {
            if (buffer.Permisos() != 2) // USUARIOS ADMINISTRADOR
            {
                tbcPagos.SelectedIndex = 0;
                this.AsignarCriterio(cmbCriterioTotalesPorSucursal, ref this.criterio_totxsuc, IndiceCriterio.FechaDePagoReal);
                this.AsignarCriterio(cmbCriterioTotalesPorRubro, ref this.criterio_totxrub, IndiceCriterio.FechaDePagoReal);
                this.AsignarCriterio(cmbCriterioRubrosPorSucursal, ref this.criterio_rubxsuc, IndiceCriterio.FechaDePagoReal);
            }
            else // USUARIOS SOLO LECTURA
            {
                tbcPagos.SelectedIndex = 1;  // RUBROS POR SUCURSAL
                this.AsignarCriterio(cmbCriterioTotalesPorSucursal, ref this.criterio_totxsuc, IndiceCriterio.PeriodoPrestacional);
                this.AsignarCriterio(cmbCriterioTotalesPorRubro, ref this.criterio_totxrub, IndiceCriterio.PeriodoPrestacional);
                this.AsignarCriterio(cmbCriterioRubrosPorSucursal, ref this.criterio_rubxsuc, IndiceCriterio.PeriodoPrestacional);
            }
        }

        private void FormTablas_Load(object sender, EventArgs e)
        {
            this.TablaGeneralOriginal = new DataGridViewClonable(dgvPagos);
            this.TablaGeneral_Load();
            this.TablaRubrosPorSucursal_Load();
            this.TablaTotalesPorSucursal_Load();
            this.TablaTotalesPorRubro_Load();
            //tbcPagos.TabPages.Remove(tbcPagos.TabPages[4]);

            // Llena el combo de sucursal en rubros (debería estar en TablaTotalesPorRubro_Load, pero el problema es que se llama varias veces a esa funcion)
            rellenadorDeDatos.LlenarCombo(cmbSucursalEnRubros, buffer.Sucursales());
            cmbSucursalEnRubros.Text = todas_suc_en_rubros;
            cmbSucursalEnRubros.Items.Add(todas_suc_en_rubros);

            // Agrego este Form al Buffer para poder volver en la navegación entre otros Forms
            buffer.FormPrincipal(this);

            /*
            // Displays the property values of the RegionInfo for "ARG".
            RegionInfo myRI1 = new RegionInfo("AR");
            Console.WriteLine("   Name:                         {0}", myRI1.Name);
            Console.WriteLine("   DisplayName:                  {0}", myRI1.DisplayName);
            Console.WriteLine("   EnglishName:                  {0}", myRI1.EnglishName);
            Console.WriteLine("   IsMetric:                     {0}", myRI1.IsMetric);
            Console.WriteLine("   ThreeLetterISORegionName:     {0}", myRI1.ThreeLetterISORegionName);
            Console.WriteLine("   ThreeLetterWindowsRegionName: {0}", myRI1.ThreeLetterWindowsRegionName);
            Console.WriteLine("   TwoLetterISORegionName:       {0}", myRI1.TwoLetterISORegionName);
            Console.WriteLine("   CurrencySymbol:               {0}", myRI1.CurrencySymbol);
            Console.WriteLine("   ISOCurrencySymbol:            {0}", myRI1.ISOCurrencySymbol);
            Console.WriteLine();

            // Compares the RegionInfo above with another RegionInfo created using CultureInfo.
            RegionInfo myRI2 = new RegionInfo(new CultureInfo("en-US", false).LCID);
            if (myRI1.Equals(myRI2))
                Console.WriteLine("The two RegionInfo instances are equal.");
            else
                Console.WriteLine("The two RegionInfo instances are NOT equal.");
            */
        }

        public void ActualizarCombos()
        {
            cmbSucursales.Items.Clear();
            rellenadorDeDatos.LlenarCombo(cmbSucursales, buffer.Sucursales());
            cmbRubros.Items.Clear();
            rellenadorDeDatos.LlenarCombo(cmbRubros, buffer.Rubros());
            cmbPersonas.Items.Clear();
            rellenadorDeDatos.LlenarCombo(cmbPersonas, buffer.Personas());
            cmbSucursalEnRubros.Items.Clear();
            rellenadorDeDatos.LlenarCombo(cmbSucursalEnRubros, buffer.Sucursales());
        }

        /*
         * 
         * ------------------------ TABLA GENERAL ------------------------
         * 
         */

        private void TablaGeneral_Load()
        {
            if (buffer.Permisos() == 2) // SOLO LECTURA
            {
                pcbAgregar.Visible = false;
                pcbModificar.Visible = false;
                pcbEliminar.Visible = false;
                pcbConfigurar.Visible = false;
                pcbUsuarios.Visible = false;
                ckbRetenciones.Checked = false;
            }

            this.LlenarVectorNombresColumnas();

            rellenadorDeDatos.LlenarCombo(cmbSucursales, buffer.Sucursales());
            rellenadorDeDatos.LlenarCombo(cmbRubros, buffer.Rubros());
            rellenadorDeFechas.LlenarMeses(cmbMesPrestacional);
            rellenadorDeFechas.LlenarAnos(cmbAnoPrestacional);
            rellenadorDeDatos.LlenarCombo(cmbPersonas, buffer.Personas());

            dgvPagos.ColumnCount = 15;
            //dgvPagos.ColumnHeadersVisible = true;

            lblFiltroActivo.Visible = false;
            lblTotalGeneral.Visible = false;
            dtpFechaPagoRealDesde.Checked = false;
            dtpFechaPagoRealHasta.Checked = false;

            this.query_general = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY id_pago DESC) AS seq FROM SIDOM.pagos)t";
            this.query_actual = this.query_general;
            this.filtro_actual = "";
            this.pagina_actual = 1;
            lblPaginaActual.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("pagos", "");
            rellenadorDeDatos.FilasMostradas(this.registros_por_pagina);
            this.total_paginas = Math.Max(1, (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina));
            rellenadorDeDatos.LlenarDataGridPaginado(true, dgvPagos, lblTotalGeneral, query_general, "", this.pagina_actual);
            this.ActualizarBotonesDePaginacion();

            /*
            btnPrimera.Enabled = false;
            btnAnterior.Enabled = false;
            if (this.total_paginas == 1)
            {
                btnSiguiente.Enabled = false;
                btnUltima.Enabled = false;
            }
            */
        }

        public void ActualizacionExternaTablaGeneral()
        {
            this.ActualizarTablaGeneral(this.pagina_actual);
        }

        private void ActualizarTablaGeneral(int pagina = 1)
        {
            dgvPagos.Rows.Clear();
            rellenadorDeDatos.LlenarDataGridPaginado(true, dgvPagos, lblTotalGeneral, this.query_actual, this.filtro_actual, pagina);
        }

        private void LlenarVectorNombresColumnas()
        {
            this.nombre_columnas.Add(Tuple.Create("periodo_prestacion_real", "PERÍODO", true));
            this.nombre_columnas.Add(Tuple.Create("fecha_pago_real", "FECHA DE PAGO", true));
            this.nombre_columnas.Add(Tuple.Create("persona", "PAGADO A", false));
            this.nombre_columnas.Add(Tuple.Create("monto", "MONTO", true));
            this.nombre_columnas.Add(Tuple.Create("retenciones_ganancias", "GANANCIAS", true));
            this.nombre_columnas.Add(Tuple.Create("retenciones_ingresos_brutos_caba", "IIBB CABA", true));
            this.nombre_columnas.Add(Tuple.Create("retenciones_ingresos_brutos_provincia", "IIBB PROVINCIA", true));
            this.nombre_columnas.Add(Tuple.Create("rubro", "RUBRO", false));
            this.nombre_columnas.Add(Tuple.Create("sucursal", "SUCURSAL", false));
            this.nombre_columnas.Add(Tuple.Create("numero_de_factura", "FACTURA", false));
            this.nombre_columnas.Add(Tuple.Create("medio_de_pago", "MEDIO DE PAGO", false));
            this.nombre_columnas.Add(Tuple.Create("banco", "BANCO", false));
            this.nombre_columnas.Add(Tuple.Create("numero_de_cheque", "CHEQUE", true));
            this.nombre_columnas.Add(Tuple.Create("observaciones", "OBSERVACIONES", true));
        }

        private void dgvPagos_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Tuple<string, string, bool> tupla_buscada = this.nombre_columnas.Find(x => x.Item2 == dgvPagos.Columns[e.ColumnIndex].HeaderText);

            if (this.tupla_actual == null)
                this.tupla_actual = new Tuple<string, string, bool>(tupla_buscada.Item1, tupla_buscada.Item2, !tupla_buscada.Item3);
            else
            {
                if (this.tupla_actual.Item1 == tupla_buscada.Item1)
                    this.tupla_actual = new Tuple<string, string, bool>(this.tupla_actual.Item1, this.tupla_actual.Item2, !this.tupla_actual.Item3);
                else
                    this.tupla_actual = new Tuple<string, string, bool>(tupla_buscada.Item1, tupla_buscada.Item2, !tupla_buscada.Item3);
            }

            if (this.tupla_actual.Item3)
                this.query_actual = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY " + tupla_buscada.Item1 + " ASC) AS seq FROM SIDOM.pagos " + this.filtro_actual + ")t";
            else
                this.query_actual = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY " + tupla_buscada.Item1 + " DESC) AS seq FROM SIDOM.pagos " + this.filtro_actual + ")t";

            this.pagina_actual = 1;
            lblPaginaActual.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("pagos", this.filtro_actual);
            this.total_paginas = (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaGeneral();
            this.ActualizarBotonesDePaginacion();
        }

        private void pcbAgregar_Click(object sender, EventArgs e)
        {
            if (new FormCargaEnLote().ShowDialog(this) == DialogResult.OK)
                this.Hide();
        }

        private void pcbModificar_Click(object sender, EventArgs e)
        {
            if (dgvPagos.SelectedRows.Count != 1)
            {
                MessageBox.Show("Debe seleccionar un pago para modificar.", "Sistema de Pagos - SIDOM S.A.");
                return;
            }
                
            FormCargaPago formCargaPago = new FormCargaPago();
            formCargaPago.CargarPago(new Pago(Convert.ToInt32(dgvPagos.CurrentRow.Cells[0].Value)));
            formCargaPago.Show();
            this.Hide();
        }

        private void pcbEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPagos.SelectedRows.Count != 1)
            {
                MessageBox.Show("Debe seleccionar un pago para eliminar.", "Sistema de Pagos - SIDOM S.A.");
                return;
            }

            Pago pago = new Pago(Convert.ToInt32(dgvPagos.CurrentRow.Cells[0].Value));
            string cheque = pago.NumeroDeCheque().ToString();
            if (pago.NumeroDeCheque() == 0) cheque = "";

            string msg = "¿Está seguro que desea eliminar este pago?\n\n" +
                "PERÍODO PRESTACIONAL: " + pago.PeriodoPrestacionReal().ToShortDateString() + "\n" +
                "FECHA DE PAGO REAL: " + pago.FechaPagoReal().ToShortDateString() + "\n" +
                "NÚMERO DE FACTURA: " + pago.NumeroDeFactura() + "\n" +
                "MEDIO DE PAGO: " + pago.MedioDePago() + "\n" +
                "BANCO: " + pago.Banco() + "\n" +
                "NÚMERO DE CHEQUE: " + cheque + "\n" +
                "MONTO: " + pago.Monto() + "\n" +
                "RETENCIÓN GANANCIAS: " + pago.RetencionesGanancias() + "\n" +
                "RETENCIÓN INGRESOS BRUTOS CABA: " + pago.RetencionesIngresosBrutosCaba() + "\n" +
                "RETENCIÓN INGRESOS BRUTOS PROVINCIA: " + pago.RetencionesIngresosBrutosProvincia() + "\n" +
                "PAGADO A: " + pago.Persona() + "\n" +
                "SUCURSAL: " + pago.Sucursal() + "\n" +
                "RUBRO: " + pago.Rubro() + "\n" +
                "OBSERVACIONES: " + pago.Observaciones();
            DialogResult result = MessageBox.Show(msg, "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                gestor.EliminarPago(pago);
                this.ActualizarTablaGeneral();
            }
        }

        private void pcbUsuario_Click(object sender, EventArgs e)
        {
            FormABMUsuarios formABMUsuarios = new FormABMUsuarios();
            formABMUsuarios.Show();
            this.Hide();
        }

        private void pcbConfigurar_Click(object sender, EventArgs e)
        {
            FormABMListados formABMListados = new FormABMListados();
            formABMListados.Show();
            this.Hide();
        }

        private void pcbExportar_Click(object sender, EventArgs e)
        {
            if (dgvPagos.Rows.Count > 0)
            {
                DataGridViewClonable TablaGeneralClonada = (DataGridViewClonable) this.TablaGeneralOriginal.Clone();
                TablaGeneralClonada.Dgv().Rows.Clear();
                rellenadorDeDatos.LlenarDataGrid(true, TablaGeneralClonada.Dgv(), new Label(), this.query_actual, "");
                new Exportador(TablaGeneralClonada.Dgv()).Exportar();
            }
            else
                MessageBox.Show("No se muestran pagos para exportar.", "Sistema de Pagos - SIDOM S.A.");
        }

        private void pcbSalir_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro/a que desea cerrar sesión?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                new FormLogin().Show();
                buffer.FormProyectado().Hide();
                this.Hide();
            }
        }

        private void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            this.btnLimpiar_Click(sender, e);
            lblFiltroActivo.Visible = false;
            lblTotalGeneral.Visible = false;
            this.query_actual = this.query_general;
            this.filtro_actual = "";
            this.pagina_actual = 1;
            lblPaginaActual.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("pagos", "");
            this.total_paginas = (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaGeneral();
            this.ActualizarBotonesDePaginacion();
            this.tupla_actual = null;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cmbSucursales.Text = "";
            cmbRubros.Text = "";
            cmbMesPrestacional.Text = "";
            cmbAnoPrestacional.Text = "";
            dtpFechaPagoRealDesde.Value = DateTime.Today;
            dtpFechaPagoRealDesde.Checked = false;
            dtpFechaPagoRealHasta.Value = DateTime.Today;
            dtpFechaPagoRealHasta.Checked = false;
            cmbPersonas.Text = "";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (cmbSucursales.Text == "" && cmbRubros.Text == "" && cmbMesPrestacional.Text == "" && cmbAnoPrestacional.Text == "" && !dtpFechaPagoRealDesde.Checked && !dtpFechaPagoRealHasta.Checked && cmbPersonas.Text == "")
            {
                this.btnMostrarTodo_Click(sender, e);
                return;
            }

            bool bandera = false;

            this.filtro_actual = " WHERE ";
            if (cmbSucursales.Text != "")
            {
                this.filtro_actual += "sucursal LIKE '%" + cmbSucursales.Text + "%'";
                bandera = true;
            }
            if (cmbRubros.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "rubro LIKE '%" + cmbRubros.Text + "%'";
                bandera = true;
            }
            if (cmbMesPrestacional.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "MONTH(periodo_prestacion_real) = '" + rellenadorDeFechas.MesANumero(cmbMesPrestacional.Text) + "'";
                bandera = true;
            }
            if (cmbAnoPrestacional.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "YEAR(periodo_prestacion_real) = '" + cmbAnoPrestacional.Text + "'";
                bandera = true;
            }
            if (dtpFechaPagoRealDesde.Checked)
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "fecha_pago_real >= '" + dtpFechaPagoRealDesde.Value.ToShortDateString() + "'";
                bandera = true;
            }
            if (dtpFechaPagoRealHasta.Checked)
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "fecha_pago_real <= '" + dtpFechaPagoRealHasta.Value.ToShortDateString() + "'";
                bandera = true;
            }
            if (cmbPersonas.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "persona LIKE '%" + cmbPersonas.Text + "%'";
            }
            this.query_actual = this.query_general.Substring(0, this.query_general.Length - 2) + this.filtro_actual + ")t";
            this.pagina_actual = 1;
            lblPaginaActual.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("pagos", this.filtro_actual);
            this.total_paginas = (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaGeneral();
            this.ActualizarBotonesDePaginacion();
            this.tupla_actual = null;
            lblFiltroActivo.Visible = true;
            lblTotalGeneral.Visible = true;
        }

        private void ckbRetenciones_CheckedChanged(object sender, EventArgs e)
        {
            // Columnas de retenciones
            bool visible = ckbRetenciones.Checked;
            dgvPagos.Columns[5].Visible = visible;
            dgvPagos.Columns[6].Visible = visible;
            dgvPagos.Columns[7].Visible = visible;
            dgvPagos.AutoResizeColumns();
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != 1)
            {
                this.pagina_actual = Math.Max(1, this.pagina_actual - 1);
                lblPaginaActual.Text = this.pagina_actual.ToString();
                this.ActualizarTablaGeneral(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != this.total_paginas)
            {
                this.pagina_actual = Math.Min(this.total_paginas, this.pagina_actual + 1);
                lblPaginaActual.Text = this.pagina_actual.ToString();
                this.ActualizarTablaGeneral(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnPrimera_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != 1)
            {
                this.pagina_actual = 1;
                lblPaginaActual.Text = this.pagina_actual.ToString();
                this.ActualizarTablaGeneral(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnUltima_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != this.total_paginas)
            {
                this.pagina_actual = this.total_paginas;
                lblPaginaActual.Text = this.pagina_actual.ToString();
                this.ActualizarTablaGeneral(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void ActualizarBotonesDePaginacion()
        {
            btnPrimera.Enabled = true;
            btnAnterior.Enabled = true;
            btnSiguiente.Enabled = true;
            btnUltima.Enabled = true;

            if (this.pagina_actual == 1)
            {
                btnPrimera.Enabled = false;
                btnAnterior.Enabled = false;
            }
            if (this.pagina_actual == this.total_paginas)
            {
                btnSiguiente.Enabled = false;
                btnUltima.Enabled = false;
            }
        }

        private void btnProyectado_Click(object sender, EventArgs e)
        {
            FormProyectado formProyectado = (FormProyectado) buffer.FormProyectado();
            formProyectado.Show();
            this.Hide();
        }

        /*
         * ------------------------ Función para agregar DataGridViewLinkColumn por defecto ------------------------
         */

        void AgregarLinkColumn(DataGridView dgv, string header)
        {
            DataGridViewLinkColumn link_column = new DataGridViewLinkColumn();
            link_column.HeaderText = header;
            link_column.LinkColor = Color.Black;
            link_column.LinkBehavior = LinkBehavior.NeverUnderline;
            link_column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(link_column);
        }

        /*
         * 
         * ------------------------ TABLA DE TOTALES POR SUCURSAL ------------------------
         * 
         */

        private void TablaTotalesPorSucursal_Load()
        {
            nudMesesSucursal.Value = this.cant_periodos_sucursal;

            // Obtiene todas las sucursales con carga
            List<string> criterio = new List<string>();
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta("SELECT descripcion FROM SIDOM.Sucursales ORDER BY descripcion ASC");
            while (lector.Read())
                criterio.Add(lector["descripcion"].ToString());
            gestor.Desconectar();
            criterio.Add("TOTAL");

            rellenadorDeDatos.InstanciarGeneracionDeTablaEstadistica(criterio, this.cant_periodos_sucursal);

            dgvPagosPorSucursal.ColumnCount = 1;
            dgvPagosPorSucursal.Columns[0].HeaderText = "SUCURSAL";
            dgvPagosPorSucursal.Columns[0].DefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvPagosPorSucursal.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            DateTime periodo;
            string mes;
            int ano;
            for (int i = 0; i < this.cant_periodos_sucursal; i++)
            {
                periodo = DateTime.Today.AddMonths(i - this.cant_periodos_sucursal + 1); // Suma 1 para que cuente el mes vigente
                mes = rellenadorDeFechas.MesATexto(periodo.Month);
                ano = periodo.Year;

                this.AgregarLinkColumn(dgvPagosPorSucursal, mes + " " + ano.ToString());

                string query_sucursales =
                    "SELECT S.descripcion, " +
                        "(SELECT ISNULL(SUM(P.monto), 0) FROM SIDOM.pagos P " +
                        "WHERE P.sucursal = S.descripcion " +
                        "AND YEAR(P." + this.criterio_totxsuc + ") = " + ano +
                        " AND MONTH(P." + this.criterio_totxsuc + ") = " + periodo.Month + ") AS total " +
                    "FROM SIDOM.sucursales S GROUP BY S.descripcion ORDER BY S.descripcion ASC";

                rellenadorDeDatos.AgregarInfoPeriodo(query_sucursales);
            }
            rellenadorDeDatos.LlenarDataGridRubroOSucursal(dgvPagosPorSucursal);
            
            /* PARA VER TOTALES POR FILA */
            //dgvPagosPorSucursal.Columns[this.cant_periodos_sucursal + 1].HeaderText = "TOTAL";
            foreach (DataGridViewRow row in dgvPagosPorSucursal.Rows) // Lo pone en negrita
            {
                row.Cells[0].Style.Font = new Font(dgvPagosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                //row.Cells[this.cant_periodos_sucursal + 1].Style.Font = new Font(dgvPagosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                //row.Cells[this.cant_periodos_sucursal + 1].Style.BackColor = SystemColors.ControlLight;
            }

            foreach (DataGridViewCell cell in dgvPagosPorSucursal.Rows[dgvPagosPorSucursal.Rows.Count - 1].Cells)
            {
                cell.Style.Font = new Font(dgvPagosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                cell.Style.BackColor = Color.FromArgb(192, 192, 255);
            }

            //dgvPagosPorSucursal.ColumnHeadersVisible = true;
        }

        private void dgvPagosPorSucursal_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;

            if (e.ColumnIndex > 0)
            {
                string sucursal = dgvPagosPorSucursal.CurrentRow.Cells[0].Value.ToString();
                string[] periodo = dgvPagosPorSucursal.Columns[dgvPagosPorSucursal.CurrentCell.ColumnIndex].HeaderText.Split(' ');
                if (sucursal == "TOTAL") sucursal = "";
                tbcPagos.SelectedIndex = 0;
                this.btnLimpiar_Click(sender, e);
                cmbSucursales.Text = sucursal;

                if (cmbCriterioTotalesPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                {
                    dtpFechaPagoRealDesde.Checked = true;
                    dtpFechaPagoRealDesde.Value = DateTime.Parse("01-" + rellenadorDeFechas.MesANumero(periodo[0]) + "-" + periodo[1]);
                    dtpFechaPagoRealHasta.Checked = true;
                    dtpFechaPagoRealHasta.Value = DateTime.Parse(rellenadorDeFechas.UltimoDiaDelMes(periodo[0], Convert.ToInt32(periodo[1])) + "-" + rellenadorDeFechas.MesANumero(periodo[0]) + "-" + periodo[1]);
                }
                else
                {
                    cmbMesPrestacional.Text = periodo[0];
                    cmbAnoPrestacional.Text = periodo[1];
                }
                this.btnBuscar_Click(sender, e);
            }
        }

        private void btnActualizarTotalesPorSucursal_Click(object sender, EventArgs e)
        {
            int indice_seleccion = -1;
            if (dgvPagosPorSucursal.CurrentCell != null)
                indice_seleccion = dgvPagosPorSucursal.CurrentCell.RowIndex;

            this.cant_periodos_sucursal = Convert.ToInt32(nudMesesSucursal.Value);
            dgvPagosPorSucursal.Columns.Clear();
            this.TablaTotalesPorSucursal_Load();
            //tbcPagos.SelectedTab.Hide();
            //tbcPagos.SelectedTab.Show();

            if (indice_seleccion == -1)
                dgvPagosPorSucursal.CurrentCell = null;
            else
                dgvPagosPorSucursal.CurrentCell = dgvPagosPorSucursal.Rows[indice_seleccion].Cells[0];
        }

        private void cmbCriterioTotalesPorSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dgvPagosPorSucursal.Rows.Count == 0) return; // Esto es porque cuando inicia la app se ejecuta el SelectedIndexChanged

            if (cmbCriterioTotalesPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                this.AsignarCriterio(cmbCriterioTotalesPorSucursal, ref criterio_totxsuc, IndiceCriterio.FechaDePagoReal);
            else
                this.AsignarCriterio(cmbCriterioTotalesPorSucursal, ref criterio_totxsuc, IndiceCriterio.PeriodoPrestacional);

            this.btnActualizarTotalesPorSucursal_Click(sender, e);
        }

        private void dgvPagosPorSucursal_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void dgvPagosPorSucursal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.selectionbar_pxs_activa = true;
        }

        /*
         * 
         * ------------------------ TABLA DE TOTALES POR RUBRO ------------------------
         * 
         */

        private void TablaTotalesPorRubro_Load()
        {
            nudMesesRubro.Value = this.cant_periodos_rubro;

            // Obtiene todos los rubros
            List<string> criterio = new List<string>();
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta("SELECT descripcion FROM SIDOM.Rubros ORDER BY descripcion ASC");
            while (lector.Read())
                criterio.Add(lector["descripcion"].ToString());
            gestor.Desconectar();
            criterio.Add("TOTAL");

            rellenadorDeDatos.InstanciarGeneracionDeTablaEstadistica(criterio, this.cant_periodos_rubro);

            dgvPagosPorRubro.ColumnCount = 1;
            dgvPagosPorRubro.Columns[0].HeaderText = "RUBRO";
            dgvPagosPorRubro.Columns[0].DefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvPagosPorRubro.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            DateTime periodo;
            string mes;
            int ano;
            for (int i = 0; i < this.cant_periodos_rubro; i++)
            {
                periodo = DateTime.Today.AddMonths(i - this.cant_periodos_rubro + 1); // Suma 1 para que cuente el mes vigente
                mes = rellenadorDeFechas.MesATexto(periodo.Month);
                ano = periodo.Year;

                this.AgregarLinkColumn(dgvPagosPorRubro, mes + " " + ano.ToString());

                string query_rubros =
                    "SELECT R.descripcion, " +
                        "(SELECT ISNULL(SUM(P.monto), 0) FROM SIDOM.pagos P " +
                        "WHERE P.rubro = R.descripcion " +
                        "AND YEAR(P." + this.criterio_totxrub + ") = " + ano +
                        " AND MONTH(P." + this.criterio_totxrub + ") = " + periodo.Month;

                if (cmbSucursalEnRubros.Text != todas_suc_en_rubros)
                    query_rubros += " AND P.sucursal = '" + cmbSucursalEnRubros.Text + "'";

                query_rubros += ") AS total FROM SIDOM.rubros R GROUP BY R.descripcion ORDER BY R.descripcion ASC";

                rellenadorDeDatos.AgregarInfoPeriodo(query_rubros);
            }
            rellenadorDeDatos.LlenarDataGridRubroOSucursal(dgvPagosPorRubro);

            /* PARA VER TOTALES POR FILA */
            //dgvPagosPorRubro.Columns[this.cant_periodos_rubro + 1].HeaderText = "TOTAL";
            foreach (DataGridViewRow row in dgvPagosPorRubro.Rows) // Lo pone en negrita
            {
                row.Cells[0].Style.Font = new Font(dgvPagosPorRubro.RowsDefaultCellStyle.Font, FontStyle.Bold);
                //row.Cells[this.cant_periodos_rubro + 1].Style.Font = new Font(dgvPagosPorRubro.RowsDefaultCellStyle.Font, FontStyle.Bold);
                //row.Cells[this.cant_periodos_rubro + 1].Style.BackColor = SystemColors.ControlLight;
            }

            foreach (DataGridViewCell cell in dgvPagosPorRubro.Rows[dgvPagosPorRubro.Rows.Count - 1].Cells)
            {
                cell.Style.Font = new Font(dgvPagosPorRubro.RowsDefaultCellStyle.Font, FontStyle.Bold);
                cell.Style.BackColor = Color.FromArgb(192, 192, 255);
            }

            //dgvPagosPorRubro.ColumnHeadersVisible = true;
        }

        private void dgvPagosPorRubro_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;

            if (e.ColumnIndex > 0)
            {
                string rubro = dgvPagosPorRubro.CurrentRow.Cells[0].Value.ToString();
                string[] periodo = dgvPagosPorRubro.Columns[dgvPagosPorRubro.CurrentCell.ColumnIndex].HeaderText.Split(' ');
                if (rubro == "TOTAL") rubro = "";
                tbcPagos.SelectedIndex = 0;
                this.btnLimpiar_Click(sender, e);

                cmbRubros.Text = rubro;
                if (cmbSucursalEnRubros.Text != todas_suc_en_rubros)
                    cmbSucursales.Text = cmbSucursalEnRubros.Text;

                if (cmbCriterioTotalesPorRubro.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                {
                    dtpFechaPagoRealDesde.Checked = true;
                    dtpFechaPagoRealDesde.Value = DateTime.Parse("01-" + rellenadorDeFechas.MesANumero(periodo[0]) + "-" + periodo[1]);
                    dtpFechaPagoRealHasta.Checked = true;
                    dtpFechaPagoRealHasta.Value = DateTime.Parse(rellenadorDeFechas.UltimoDiaDelMes(periodo[0], Convert.ToInt32(periodo[1])) + "-" + rellenadorDeFechas.MesANumero(periodo[0]) + "-" + periodo[1]);
                }
                else
                {
                    cmbMesPrestacional.Text = periodo[0];
                    cmbAnoPrestacional.Text = periodo[1];
                }
                this.btnBuscar_Click(sender, e);
            }
        }

        private void btnActualizarTotalesPorRubro_Click(object sender, EventArgs e)
        {
            if (cmbSucursalEnRubros.Text != todas_suc_en_rubros)
            {
                ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();
                validadorDeDatos.NuevaValidacion("Debe completar o corregir el siguiente campo:\n");
                validadorDeDatos.ValidarCombo(cmbSucursalEnRubros.Text, cmbSucursales.Items, "\nSUCURSAL");
                if (!validadorDeDatos.EstaCorrecto())
                {
                    MessageBox.Show(validadorDeDatos.MensajeFinal(), "Sistema de Pagos - SIDOM S.A.");
                    return;
                }
            }

            int indice_seleccion = -1;
            if (dgvPagosPorRubro.CurrentCell != null)
                indice_seleccion = dgvPagosPorRubro.CurrentCell.RowIndex;

            this.cant_periodos_rubro = Convert.ToInt32(nudMesesRubro.Value);
            dgvPagosPorRubro.Columns.Clear();
            this.TablaTotalesPorRubro_Load();

            if (indice_seleccion == -1)
                dgvPagosPorRubro.CurrentCell = null;
            else
                dgvPagosPorRubro.CurrentCell = dgvPagosPorRubro.Rows[indice_seleccion].Cells[0];
        }

        private void cmbCriterioTotalesPorRubro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dgvPagosPorRubro.Rows.Count == 0) return; // Esto es porque cuando inicia la app se ejecuta el SelectedIndexChanged

            if (cmbCriterioTotalesPorRubro.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                this.AsignarCriterio(cmbCriterioTotalesPorRubro, ref criterio_totxrub, IndiceCriterio.FechaDePagoReal);
            else
                this.AsignarCriterio(cmbCriterioTotalesPorRubro, ref criterio_totxrub, IndiceCriterio.PeriodoPrestacional);

            this.btnActualizarTotalesPorRubro_Click(sender, e);
        }

        private void dgvPagosPorRubro_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void cmbSucursalEnRubros_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnActualizarTotalesPorRubro_Click(sender, e);
        }

        private void dgvPagosPorRubro_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.selectionbar_pxr_activa = true;
        }

        private void cmbSucursalEnRubros_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        /*
         * 
         * ------------------------ TABLA DE RUBROS POR SUCURSAL ------------------------
         * 
         */

        private void TablaRubrosPorSucursal_Load()
        {
            DateTime primer_dia = DateTime.Parse("01-" + DateTime.Today.Month + "-" + DateTime.Today.Year);
            dtpRubrosPorSucursalDesde.Value = primer_dia;
            DateTime ultimo_dia = primer_dia.AddMonths(1).AddDays(-1);
            dtpRubrosPorSucursalHasta.Value = ultimo_dia;

            rellenadorDeFechas.LlenarMeses(cmbMesPrestacionalRpS);
            cmbMesPrestacionalRpS.Text = rellenadorDeFechas.MesATexto(DateTime.Today.AddMonths(-3).Month);
            rellenadorDeFechas.LlenarAnos(cmbAnoPrestacionalRpS);
            cmbAnoPrestacionalRpS.Text = DateTime.Today.Year.ToString();

            this.ActualizarPagosRpS(primer_dia, ultimo_dia);

            dgvPagosRubrosPorSucursal.CurrentCell = null;

            /*
            DataGridViewLinkColumn link_column = new DataGridViewLinkColumn();
            link_column.Text = "DETALLE";
            link_column.UseColumnTextForLinkValue = true;
            dgvPagosRubrosPorSucursal.Columns.Add(link_column);
            dgvPagosRubrosPorSucursal.AutoResizeColumn(buffer.Sucursales().Count + 1);
            */
        }

        private void btnActualizarRubrosPorSucursal_Click(object sender, EventArgs e)
        {
            int indice_seleccion = -1;
            if (dgvPagosRubrosPorSucursal.CurrentCell != null)
                indice_seleccion = dgvPagosRubrosPorSucursal.CurrentCell.RowIndex;

            dgvPagosRubrosPorSucursal.Rows.Clear();
            this.ActualizarPagosRpS(dtpRubrosPorSucursalDesde.Value, dtpRubrosPorSucursalHasta.Value);

            if (indice_seleccion == -1)
                dgvPagosRubrosPorSucursal.CurrentCell = null;
            else
                dgvPagosRubrosPorSucursal.CurrentCell = dgvPagosRubrosPorSucursal.Rows[indice_seleccion].Cells[0];
        }

        private void AjustarElementosVisiblesRpS(bool es_fecha_de_pago_real)
        {
            lblDesdeRpS.Visible = es_fecha_de_pago_real;
            lblHastaRpS.Visible = es_fecha_de_pago_real;
            dtpRubrosPorSucursalDesde.Visible = es_fecha_de_pago_real;
            dtpRubrosPorSucursalHasta.Visible = es_fecha_de_pago_real;
            lblPeriodoRpS.Visible = !es_fecha_de_pago_real;
            cmbMesPrestacionalRpS.Visible = !es_fecha_de_pago_real;
            cmbAnoPrestacionalRpS.Visible = !es_fecha_de_pago_real;
        }

        private void ActualizarPagosRpS(DateTime fecha_desde, DateTime fecha_hasta)
        {
            this.AjustarElementosVisiblesRpS(cmbCriterioRubrosPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal]);

            dgvPagosRubrosPorSucursal.ColumnCount = 1;
            dgvPagosRubrosPorSucursal.Columns[0].HeaderText = "RUBRO";
            dgvPagosRubrosPorSucursal.Columns[0].DefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvPagosRubrosPorSucursal.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach (string sucursal in buffer.Sucursales())
                this.AgregarLinkColumn(dgvPagosRubrosPorSucursal, sucursal);
            this.AgregarLinkColumn(dgvPagosRubrosPorSucursal, "TOTAL");

            rellenadorDeDatos.InicializarTotalesPorColumna();

            foreach (string rubro in buffer.Rubros())
            {
                string query_rubros_por_sucursal;

                if (cmbCriterioRubrosPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                    query_rubros_por_sucursal =
                        "SELECT S.descripcion AS sucursal, " +
                            "(SELECT ISNULL(SUM(P.monto), 0) " +
                            "FROM SIDOM.Pagos P " +
                            "WHERE P.sucursal = S.descripcion" +
                            " AND P.rubro = '" + rubro + "'" +
                            " AND P.fecha_pago_real >= '" + fecha_desde.ToShortDateString() + "'" +
                            " AND P.fecha_pago_real <= '" + fecha_hasta.ToShortDateString() + "') AS total " +
                        "FROM SIDOM.Sucursales S " +
                        "GROUP BY S.descripcion " +
                        "ORDER BY S.descripcion ASC";
                else
                    query_rubros_por_sucursal =
                        "SELECT S.descripcion AS sucursal, " +
                            "(SELECT ISNULL(SUM(P.monto), 0) " +
                            "FROM SIDOM.Pagos P " +
                            "WHERE P.sucursal = S.descripcion" +
                            " AND P.rubro = '" + rubro + "'" +
                            " AND MONTH(P.periodo_prestacion_real) = '" + rellenadorDeFechas.MesANumero(cmbMesPrestacionalRpS.Text) + "'" +
                            " AND YEAR(P.periodo_prestacion_real) <= '" + cmbAnoPrestacionalRpS.Text + "') AS total " +
                        "FROM SIDOM.Sucursales S " +
                        "GROUP BY S.descripcion " +
                        "ORDER BY S.descripcion ASC";

                rellenadorDeDatos.AgregarInfoRubroPorSucursal(dgvPagosRubrosPorSucursal, rubro, query_rubros_por_sucursal);
            }

            rellenadorDeDatos.AgregarTotalesPorColumnaRubroPorSucursal(dgvPagosRubrosPorSucursal);

            foreach (DataGridViewRow row in dgvPagosRubrosPorSucursal.Rows)
            {
                row.Cells[0].Style.Font = new Font(dgvPagosRubrosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                row.Cells[row.Cells.Count - 1].Style.Font = new Font(dgvPagosRubrosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                row.Cells[row.Cells.Count - 1].Style.BackColor = Color.FromArgb(192, 192, 255);
            }

            foreach (DataGridViewCell cell in dgvPagosRubrosPorSucursal.Rows[dgvPagosRubrosPorSucursal.Rows.Count - 1].Cells)
            {
                cell.Style.Font = new Font(dgvPagosRubrosPorSucursal.RowsDefaultCellStyle.Font, FontStyle.Bold);
                cell.Style.BackColor = Color.FromArgb(192, 192, 255);
            }

            dgvPagosRubrosPorSucursal.Columns[dgvPagosRubrosPorSucursal.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        private void cmbCriterioRubrosPorSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dgvPagosRubrosPorSucursal.Rows.Count == 0) return; // Esto es porque cuando inicia la app se ejecuta el SelectedIndexChanged

            if (cmbCriterioRubrosPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                this.AsignarCriterio(cmbCriterioRubrosPorSucursal, ref criterio_rubxsuc, IndiceCriterio.FechaDePagoReal);
            else
                this.AsignarCriterio(cmbCriterioRubrosPorSucursal, ref criterio_rubxsuc, IndiceCriterio.PeriodoPrestacional);

            this.btnActualizarRubrosPorSucursal_Click(sender, e);
        }

        private void dgvPagosRubrosPorSucursal_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;

            if (e.ColumnIndex > 0)
            {
                string rubro = dgvPagosRubrosPorSucursal.CurrentRow.Cells[0].Value.ToString();
                if (rubro == "TOTAL") rubro = "";
                string sucursal = dgvPagosRubrosPorSucursal.Columns[e.ColumnIndex].HeaderText;
                if (sucursal == "TOTAL") sucursal = "";
                tbcPagos.SelectedIndex = 0;
                this.btnLimpiar_Click(sender, e);
                cmbRubros.Text = rubro;
                cmbSucursales.Text = sucursal;

                if (cmbCriterioRubrosPorSucursal.Text == criterios_cmb[(int) IndiceCriterio.FechaDePagoReal])
                {
                    dtpFechaPagoRealDesde.Checked = true;
                    dtpFechaPagoRealDesde.Value = dtpRubrosPorSucursalDesde.Value;
                    dtpFechaPagoRealHasta.Checked = true;
                    dtpFechaPagoRealHasta.Value = dtpRubrosPorSucursalHasta.Value;
                }
                else
                {
                    cmbMesPrestacional.Text = cmbMesPrestacionalRpS.Text;
                    cmbAnoPrestacional.Text = cmbAnoPrestacionalRpS.Text;
                }
                this.btnBuscar_Click(sender, e);
            }
        }

        private void dgvPagosRubrosPorSucursal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.selectionbar_rxs_activa = true;
        }

        // ------------------------------------------------------------------------------

        // Cerrar aplicación
        /*
        private void FormTablas_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro/a que desea cerrar Pompy?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
                e.Cancel = true;
        }*/

        private void FormTablas_FormClosing(object sender, FormClosingEventArgs e)
        {
            funcionesPolimorficas.CerrarApp(e);
        }
    }
}
