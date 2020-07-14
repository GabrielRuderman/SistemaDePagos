using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using SistemaDePagos.Dominio;

namespace SistemaDePagos.Biblioteca
{
    class RellenadorDeDatos
    {
        public int filas_mostradas = 30;
        private static RellenadorDeDatos instancia;
        private static GestorDB gestor = GestorDB.GetInstance();
        private static BufferDB buffer = BufferDB.GetInstance();
        private static ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();
        private MatrizEstadistica matriz;
        private decimal[] total_columnas;

        // Utilizo el patrón Singleton
        public static RellenadorDeDatos GetInstance()
        {
            if (instancia == null) instancia = new RellenadorDeDatos();
            return instancia;
        }

        public void FilasMostradas(int cantidad)
        {
            this.filas_mostradas = cantidad;
        }

        public void LlenarCombo(ComboBox cmb, HashSet<string> lista)
        {
            foreach (string item in lista) cmb.Items.Add(item);
        }

        public void LlenarListBox(ListBox lsb, HashSet<string> lista)
        {
            foreach (string item in lista) lsb.Items.Add(item);
        }

        public void LlenarDataGridPaginado(bool real, DataGridView dgv, Label lblTotal, string query, string filtro, int pagina_actual = 1)
        {
            int offset = (pagina_actual - 1) * filas_mostradas + 1;
            int fin_pagina = pagina_actual * filas_mostradas;
            string paginado = " WHERE seq BETWEEN " + offset + " AND " + fin_pagina;
            this.LlenarDataGrid(real, dgv, lblTotal, query + paginado, filtro);

            /*
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            int ancho_pix_pantalla = Screen.PrimaryScreen.Bounds.Width; //Obtiene el ancho de la pantalla principal en pixeles.
            int ancho_pix_mostrado = 0;
            foreach (DataGridViewColumn col in dgv.Columns)
                if (col.Visible) ancho_pix_mostrado += col.Width;
            MessageBox.Show("px pantalla: " + ancho_pix_pantalla + " - px mostrado: " + ancho_pix_mostrado);
            if (ancho_pix_pantalla >= ancho_pix_mostrado)
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            */
        }

        public void LlenarDataGrid(bool real, DataGridView dgv, Label lblTotal, string query, string filtro)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(query);

            if (real)
                while (lector.Read())
                {
                    string[] row = new string[]
                    {
                        lector["id_pago"].ToString(),
                        Convert.ToDateTime(lector["periodo_prestacion_real"].ToString()).ToShortDateString().Substring(3),
                        Convert.ToDateTime(lector["fecha_pago_real"].ToString()).ToShortDateString(),
                        lector["persona"].ToString(),
                        validadorDeDatos.ObtenerFormatoMoneda(lector["monto"]),
                        validadorDeDatos.ObtenerFormatoMoneda(lector["retenciones_ganancias"]),
                        validadorDeDatos.ObtenerFormatoMoneda(lector["retenciones_ingresos_brutos_caba"]),
                        validadorDeDatos.ObtenerFormatoMoneda(lector["retenciones_ingresos_brutos_provincia"]),
                        lector["rubro"].ToString(),
                        lector["sucursal"].ToString(),
                        lector["numero_de_factura"].ToString(),
                        lector["medio_de_pago"].ToString(),
                        lector["banco"].ToString(),
                        lector["numero_de_cheque"].ToString(),                    
                        lector["observaciones"].ToString()
                    };
                    if (!Convert.ToBoolean(lector["fecha_pago_real_checked"])) row[2] = ""; // Fecha de pago real
                    if (row[13] == "0") row[13] = ""; // Número de cheque
                    dgv.Rows.Add(row);
                    /* PARA CALCULAR TOTAL CON RETENCIONES:
                    retenciones += validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ganancias"].ToString())
                        + validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ingresos_brutos_caba"].ToString())
                        + validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ingresos_brutos_provincia"].ToString());
                    */
                }
            else
                while (lector.Read())
                {
                    string[] row = new string[]
                    {
                        lector["id_proyectado"].ToString(),
                        Convert.ToDateTime(lector["periodo_prestacion_real"].ToString()).ToShortDateString().Substring(3),
                        lector["persona"].ToString(),
                        validadorDeDatos.ObtenerFormatoMoneda(lector["monto"]),
                        lector["rubro"].ToString(),
                        lector["sucursal"].ToString(),
                        lector["prestacion"].ToString(),
                    };
                    dgv.Rows.Add(row);
                }

            gestor.Desconectar();
            dgv.AutoResizeColumns();

            decimal total = 0;
            gestor.Conectar();

            if (real)
                lector = gestor.Consulta("SELECT ISNULL(SUM(monto), 0) AS total FROM SIDOM.pagos " + filtro);
            else
                lector = gestor.Consulta("SELECT ISNULL(SUM(monto), 0) AS total FROM SIDOM.proyectado " + filtro);

            if (lector.Read())
                total = Convert.ToDecimal(lector["total"]);
            lblTotal.Text = "$" + validadorDeDatos.ObtenerFormatoMoneda(total);
        }

        public void InstanciarGeneracionDeTablaEstadistica(List<string> criterio, int cant_periodos)
        {
            matriz = new MatrizEstadistica(criterio, cant_periodos);
        }

        public void AgregarInfoPeriodo(string query)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(query);
            this.matriz.AgregarColumna(lector);
            gestor.Desconectar();
        }

        public void LlenarDataGridRubroOSucursal(DataGridView dgv)
        {
            for (int i = 0; i < this.matriz.CantidadDeFilas(); i++)
                dgv.Rows.Add(this.matriz.ObtenerFila());
        }

        public void InicializarTotalesPorColumna()
        {
            this.total_columnas = new decimal[buffer.Sucursales().Count + 1];
            for (int i = 0; i < total_columnas.Length; i++)
                this.total_columnas[i] = 0;
        }

        public void AgregarInfoRubroPorSucursal(DataGridView dgv, string rubro, string query)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(query);
            string[] row = new string[buffer.Sucursales().Count + 2];
            row[0] = rubro;
            decimal total_fila = 0;
            int i = 1;
            while (lector.Read())
            {
                object valor = lector["total"];
                total_fila += Convert.ToDecimal(valor);
                this.total_columnas[i - 1] += Convert.ToDecimal(valor);
                row[i] = validadorDeDatos.ObtenerFormatoMoneda(valor);
                i++;
            }
            row[row.Length - 1] = validadorDeDatos.ObtenerFormatoMoneda(total_fila);
            this.total_columnas[i - 1] += total_fila;
            dgv.Rows.Add(row);
            gestor.Desconectar();
        }

        public void AgregarTotalesPorColumnaRubroPorSucursal(DataGridView dgv)
        {
            string[] row = new string[buffer.Sucursales().Count + 2];
            row[0] = "TOTAL";
            for (int i = 0; i < total_columnas.Length; i++)
                row[i + 1] = validadorDeDatos.ObtenerFormatoMoneda(this.total_columnas[i]);
            dgv.Rows.Add(row);
        }

        public void CargarPago(Pago pago, string query)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(query);
            if (lector.Read())
            {
                pago.PeriodoPrestacionReal(Convert.ToDateTime(lector["periodo_prestacion_real"].ToString()));
                pago.FechaPagoRealChecked(Convert.ToBoolean(lector["fecha_pago_real_checked"]));
                pago.FechaPagoReal(Convert.ToDateTime(lector["fecha_pago_real"].ToString()));
                pago.NumeroDeFactura(lector["numero_de_factura"].ToString());
                pago.MedioDePago(lector["medio_de_pago"].ToString());
                pago.Banco(lector["banco"].ToString());
                pago.NumeroDeCheque(Convert.ToInt32(lector["numero_de_cheque"].ToString()));
                pago.Monto(validadorDeDatos.ObtenerFormatoDecimal(lector["monto"].ToString()));
                pago.RetencionesGanancias(validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ganancias"].ToString()));
                pago.RetencionesIngresosBrutosCaba(validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ingresos_brutos_caba"].ToString()));
                pago.RetencionesIngresosBrutosProvincia(validadorDeDatos.ObtenerFormatoDecimal(lector["retenciones_ingresos_brutos_provincia"].ToString()));
                pago.Persona(lector["persona"].ToString());
                pago.Sucursal(lector["sucursal"].ToString());
                pago.Rubro(lector["rubro"].ToString());
                pago.Observaciones(lector["observaciones"].ToString());
            }
            gestor.Desconectar();
        }

    }
}
