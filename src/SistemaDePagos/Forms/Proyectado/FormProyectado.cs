using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SistemaDePagos.Biblioteca;
using SistemaDePagos.Dominio;

namespace SistemaDePagos.Forms.Proyectado
{
    public partial class FormProyectado : Form
    {
        private DataGridViewClonable TablaProyectadoOriginal;
        private static GestorDB gestor = GestorDB.GetInstance();
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();
        private static RellenadorDeFechas rellenadorDeFechas = RellenadorDeFechas.GetInstance();
        private static FuncionesPolimorficas funcionesPolimorficas = FuncionesPolimorficas.GetInstance();
        private Tuple<string, string, bool> tupla_actual = null;
        private string query_general, query_actual, filtro_actual;
        private int pagina_actual, total_registros, total_paginas, registros_por_pagina = 100;
        private List<Tuple<string, string, bool>> nombre_columnas = new List<Tuple<string, string, bool>>();
        private static BufferDB buffer = BufferDB.GetInstance();

        public FormProyectado()
        {
            InitializeComponent();
        }

        private void FormProyectado_Load(object sender, EventArgs e)
        {
            this.TablaProyectadoOriginal = new DataGridViewClonable(dgvProyectado);
            this.TablaProyectado_Load();
        }

        private void TablaProyectado_Load()
        {
            this.LlenarVectorNombresColumnas();

            rellenadorDeDatos.LlenarCombo(cmbSucursalesProyectado, buffer.Sucursales());
            rellenadorDeDatos.LlenarCombo(cmbRubrosProyectado, buffer.Rubros());
            rellenadorDeFechas.LlenarMeses(cmbMesPrestacionalProyectado);
            rellenadorDeFechas.LlenarAnos(cmbAnoPrestacionalProyectado);
            rellenadorDeDatos.LlenarCombo(cmbPersonasProyectado, buffer.Personas());

            dgvProyectado.ColumnCount = 7;

            lblFiltroActivoProyectado.Visible = false;
            lblTotalGeneralProyectado.Visible = false;

            this.query_general = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY id_proyectado DESC) AS seq FROM SIDOM.proyectado)t";
            this.query_actual = this.query_general;
            this.filtro_actual = "";
            this.pagina_actual = 1;
            lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("proyectado", "");
            rellenadorDeDatos.FilasMostradas(this.registros_por_pagina);
            this.total_paginas = Math.Max(1, (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina));
            rellenadorDeDatos.LlenarDataGridPaginado(false, dgvProyectado, lblTotalGeneralProyectado, query_general, "", this.pagina_actual);
            this.ActualizarBotonesDePaginacion();

        }

        private void ActualizarTablaProyectado(int pagina = 1)
        {
            dgvProyectado.Rows.Clear();
            rellenadorDeDatos.LlenarDataGridPaginado(false, dgvProyectado, lblTotalGeneralProyectado, this.query_actual, this.filtro_actual, pagina);
        }

        private void LlenarVectorNombresColumnas()
        {
            this.nombre_columnas.Add(Tuple.Create("periodo_prestacion_real", "PERÍODO", true));
            this.nombre_columnas.Add(Tuple.Create("persona", "PAGADO A", false));
            this.nombre_columnas.Add(Tuple.Create("monto", "MONTO", true));
            this.nombre_columnas.Add(Tuple.Create("rubro", "RUBRO", false));
            this.nombre_columnas.Add(Tuple.Create("sucursal", "SUCURSAL", false));
            this.nombre_columnas.Add(Tuple.Create("prestacion", "PRESTACIONES", true));
        }

        private void dgvProyectado_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Tuple<string, string, bool> tupla_buscada = this.nombre_columnas.Find(x => x.Item2 == dgvProyectado.Columns[e.ColumnIndex].HeaderText);

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
                this.query_actual = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY " + tupla_buscada.Item1 + " ASC) AS seq FROM SIDOM.proyectado " + this.filtro_actual + ")t";
            else
                this.query_actual = "SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY " + tupla_buscada.Item1 + " DESC) AS seq FROM SIDOM.proyectado " + this.filtro_actual + ")t";

            this.pagina_actual = 1;
            lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("proyectado", this.filtro_actual);
            this.total_paginas = (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaProyectado();
            this.ActualizarBotonesDePaginacion();
        }

        private void ActualizarBotonesDePaginacion()
        {
            btnPrimeraProyectado.Enabled = true;
            btnAnteriorProyectado.Enabled = true;
            btnSiguienteProyectado.Enabled = true;
            btnUltimaProyectado.Enabled = true;

            if (this.pagina_actual == 1)
            {
                btnPrimeraProyectado.Enabled = false;
                btnAnteriorProyectado.Enabled = false;
            }
            if (this.pagina_actual == this.total_paginas)
            {
                btnSiguienteProyectado.Enabled = false;
                btnUltimaProyectado.Enabled = false;
            }
        }

        private void pcbExportarProyectado_Click(object sender, EventArgs e)
        {
            if (dgvProyectado.Rows.Count > 0)
            {
                DataGridViewClonable TablaProyectadoClonada = (DataGridViewClonable) this.TablaProyectadoOriginal.Clone();
                TablaProyectadoClonada.Dgv().Rows.Clear();
                rellenadorDeDatos.LlenarDataGrid(false, TablaProyectadoClonada.Dgv(), new Label(), this.query_actual, "");
                new Exportador(TablaProyectadoClonada.Dgv()).Exportar();
            }
            else
                MessageBox.Show("No se muestran pagos para exportar.", "Sistema de Pagos - SIDOM S.A.");
        }

        private void pcbSalirProyectado_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro/a que desea cerrar sesión?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                new FormLogin().Show();
                buffer.FormPrincipal().Hide();
                this.Hide();
            }
        }

        private void btnMostrarTodoProyectado_Click(object sender, EventArgs e)
        {
            this.btnLimpiarProyectado_Click(sender, e);
            lblFiltroActivoProyectado.Visible = false;
            lblTotalGeneralProyectado.Visible = false;
            this.query_actual = this.query_general;
            this.filtro_actual = "";
            this.pagina_actual = 1;
            lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("proyectado", "");
            this.total_paginas = (int)Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaProyectado();
            this.ActualizarBotonesDePaginacion();
            this.tupla_actual = null;
        }

        private void btnLimpiarProyectado_Click(object sender, EventArgs e)
        {
            cmbSucursalesProyectado.Text = "";
            cmbRubrosProyectado.Text = "";
            cmbMesPrestacionalProyectado.Text = "";
            cmbAnoPrestacionalProyectado.Text = "";
            cmbPersonasProyectado.Text = "";
        }

        private void nudCantMinPrestaciones_ValueChanged(object sender, EventArgs e)
        {
            ckbMinPrestaciones.Checked = true;
        }

        private void nudCantMaxPrestaciones_ValueChanged(object sender, EventArgs e)
        {
            ckbMaxPrestaciones.Checked = true;
        }


        private void btnBuscarProyectado_Click(object sender, EventArgs e)
        {
            if (cmbSucursalesProyectado.Text == "" && cmbRubrosProyectado.Text == "" && cmbMesPrestacionalProyectado.Text == "" && cmbAnoPrestacionalProyectado.Text == "" && cmbPersonasProyectado.Text == "" && !ckbMinPrestaciones.Checked && !ckbMaxPrestaciones.Checked)
            {
                this.btnMostrarTodoProyectado_Click(sender, e);
                return;
            }

            bool bandera = false;

            this.filtro_actual = " WHERE ";
            if (cmbSucursalesProyectado.Text != "")
            {
                this.filtro_actual += "sucursal LIKE '%" + cmbSucursalesProyectado.Text + "%'";
                bandera = true;
            }
            if (cmbRubrosProyectado.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "rubro LIKE '%" + cmbRubrosProyectado.Text + "%'";
                bandera = true;
            }
            if (cmbMesPrestacionalProyectado.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "MONTH(periodo_prestacion_real) = '" + rellenadorDeFechas.MesANumero(cmbMesPrestacionalProyectado.Text) + "'";
                bandera = true;
            }
            if (cmbAnoPrestacionalProyectado.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "YEAR(periodo_prestacion_real) = '" + cmbAnoPrestacionalProyectado.Text + "'";
                bandera = true;
            }
            if (cmbPersonasProyectado.Text != "")
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "persona LIKE '%" + cmbPersonasProyectado.Text + "%'";
                bandera = true;
            }
            if (ckbMinPrestaciones.Checked)
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "prestacion >= " + nudCantMinPrestaciones.Value;
                bandera = true;
            }
            if (ckbMaxPrestaciones.Checked)
            {
                if (bandera) this.filtro_actual += " AND ";
                this.filtro_actual += "prestacion <= " + nudCantMaxPrestaciones.Value;
            }
            this.query_actual = this.query_general.Substring(0, this.query_general.Length - 2) + this.filtro_actual + ")t";
            this.pagina_actual = 1;
            lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
            this.total_registros = gestor.ObtenerTotalRegistros("proyectado", this.filtro_actual);
            this.total_paginas = (int) Math.Ceiling((decimal) this.total_registros / (decimal) this.registros_por_pagina);
            this.ActualizarTablaProyectado();
            this.ActualizarBotonesDePaginacion();
            this.tupla_actual = null;
            lblFiltroActivoProyectado.Visible = true;
            lblTotalGeneralProyectado.Visible = true;
        }

        private void btnAnteriorProyectado_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != 1)
            {
                this.pagina_actual = Math.Max(1, this.pagina_actual - 1);
                lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
                this.ActualizarTablaProyectado(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnSiguienteProyectado_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != this.total_paginas)
            {
                this.pagina_actual = Math.Min(this.total_paginas, this.pagina_actual + 1);
                lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
                this.ActualizarTablaProyectado(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnPrimeraProyectado_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != 1)
            {
                this.pagina_actual = 1;
                lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
                this.ActualizarTablaProyectado(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnUltimaProyectado_Click(object sender, EventArgs e)
        {
            if (this.pagina_actual != this.total_paginas)
            {
                this.pagina_actual = this.total_paginas;
                lblPaginaActualProyectado.Text = this.pagina_actual.ToString();
                this.ActualizarTablaProyectado(this.pagina_actual);
                this.ActualizarBotonesDePaginacion();
            }
        }

        private void btnReal_Click(object sender, EventArgs e)
        {
            FormTablas formPrincipal = (FormTablas) buffer.FormPrincipal();
            formPrincipal.Show();
            this.Hide();
        }

        private void FormProyectado_FormClosing(object sender, FormClosingEventArgs e)
        {
            funcionesPolimorficas.CerrarApp(e);
        }
    }
}
