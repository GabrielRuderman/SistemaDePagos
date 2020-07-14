using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using SistemaDePagos.Dominio;

namespace SistemaDePagos.Biblioteca
{
    class GestorDB
    {
        private static GestorDB instancia;
        private SqlConnection connection;
        private SqlCommand command;
        private bool conexion_virgen = true;
        private string conexion_ok = "";

        // El constructor no puede ser invocado desde otra clase
        private GestorDB() { }

        // Utilizo el patrón Singleton
        public static GestorDB GetInstance()
        {
            if (instancia == null) instancia = new GestorDB();
            return instancia;
        }

        public string ConnectionData()
        {
            return this.connection.ConnectionString;
        }

        public void Conectar()
        {
            String connectionData;

            if (conexion_virgen)
            {
                try
                {
                    // Traigo los datos del App.config
                    connectionData = ConfigurationManager.ConnectionStrings["ConexionBaseDePagos"].ConnectionString;
                    connection = new SqlConnection(connectionData);
                    connection.Open();

                    this.conexion_virgen = false;
                    this.conexion_ok = "ConexionBaseDePagos";
                }
                catch //(Exception e)
                {
                    try
                    {
                        // Si no lo logré, intento con la IP pública
                        connectionData = ConfigurationManager.ConnectionStrings["ConexionBaseDePagos2"].ConnectionString;
                        connection = new SqlConnection(connectionData);
                        connection.Open();

                        this.conexion_virgen = false;
                        this.conexion_ok = "ConexionBaseDePagos2";
                    }
                    catch
                    {
                        DialogResult result = MessageBox.Show("Error: no se pudo establecer la comunicación con el servidor. Verifique la conexión a internet.\nSi persiste el error comuníquese con el área de sistemas.", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        if (result == DialogResult.Retry) this.Conectar();
                        //MessageBox.Show(e.ToString());
                    }
                }
            }
            else
            {
                try
                {
                    // Si no lo logré, intento con la IP pública
                    connectionData = ConfigurationManager.ConnectionStrings[this.conexion_ok].ConnectionString;
                    connection = new SqlConnection(connectionData);
                    connection.Open();
                }
                catch
                {
                    DialogResult result = MessageBox.Show("Error: no se pudo establecer la comunicación con el servidor. Verifique la conexión a internet.\nSi persiste el error comuníquese con el área de sistemas.", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    if (result == DialogResult.Retry) this.Conectar();
                    //MessageBox.Show(e.ToString());
                }
            }
        }

        public void Desconectar()
        {
            connection.Close();
        }

        public SqlDataReader Consulta(string query)
        {
            Console.WriteLine(query);
            this.command = new SqlCommand(query, connection);
            return command.ExecuteReader();
        }

        private void GenerarStoredProcedure(string procedure)
        {
            this.command = new SqlCommand("SIDOM." + procedure, connection);
            this.command.CommandType = CommandType.StoredProcedure;
        }

        private void ParametroPorValor(string nombre, object valor)
        {
            this.command.Parameters.AddWithValue(nombre, valor);
        }

        private int EjecutarStoredProcedure()
        {
            try
            {
                this.command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                this.command.Parameters["@ReturnVal"].Direction = ParameterDirection.ReturnValue;
                this.command.ExecuteNonQuery();
                return Convert.ToInt32(this.command.Parameters["@ReturnVal"].Value);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return -1;
            }
        }

        public int ObtenerTotalRegistros(string tabla, string filtro)
        {
            this.Conectar();
            SqlDataReader lector = this.Consulta("SELECT COUNT(*) AS total_registros FROM SIDOM." + tabla + filtro);
            lector.Read();
            return Convert.ToInt32(lector["total_registros"].ToString());
        }

        public void PersistirPago(Pago pago, bool nuevo)
        {
            this.Conectar();

            if (nuevo)
                this.GenerarStoredProcedure("crear_pago");
            else
            {
                this.GenerarStoredProcedure("modificar_pago");
                this.ParametroPorValor("id_pago", pago.Id());
            }

            this.ParametroPorValor("periodo_prestacion_real", pago.PeriodoPrestacionReal());
            this.ParametroPorValor("fecha_pago_real_checked", pago.FechaPagoRealChecked());
            if (pago.FechaPagoRealChecked())
                this.ParametroPorValor("fecha_pago_real", pago.FechaPagoReal());
            else
                this.ParametroPorValor("fecha_pago_real", 0);
            this.ParametroPorValor("numero_de_factura", pago.NumeroDeFactura());
            this.ParametroPorValor("medio_de_pago", pago.MedioDePago());
            this.ParametroPorValor("banco", pago.Banco());
            this.ParametroPorValor("numero_de_cheque", pago.NumeroDeCheque());
            this.ParametroPorValor("monto", pago.Monto());
            this.ParametroPorValor("retenciones_ganancias", pago.RetencionesGanancias());
            this.ParametroPorValor("retenciones_ingresos_brutos_caba", pago.RetencionesIngresosBrutosCaba());
            this.ParametroPorValor("retenciones_ingresos_brutos_provincia", pago.RetencionesIngresosBrutosProvincia());
            this.ParametroPorValor("persona", pago.Persona());
            this.ParametroPorValor("observaciones", pago.Observaciones());
            this.ParametroPorValor("sucursal", pago.Sucursal());
            this.ParametroPorValor("rubro", pago.Rubro());
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public void EliminarPago(Pago pago)
        {
            this.Conectar();
            this.GenerarStoredProcedure("eliminar_pago");
            this.ParametroPorValor("id_pago", pago.Id());
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public void PersistirItem(string procedure, string descripcion)
        {
            this.Conectar();
            this.GenerarStoredProcedure(procedure);
            this.ParametroPorValor("descripcion", descripcion.ToUpper());
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public void EliminarItem(string procedure, string descripcion)
        {
            this.Conectar();
            this.GenerarStoredProcedure(procedure);
            this.ParametroPorValor("descripcion", descripcion);
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public void AgregarUsuario(string nombre_usuario, string contrasena, int prioridad_permiso)
        {
            this.Conectar();
            this.GenerarStoredProcedure("crear_usuario");
            this.ParametroPorValor("nombre_de_usuario", nombre_usuario);
            this.ParametroPorValor("contrasena", contrasena);
            this.ParametroPorValor("prioridad_permiso", prioridad_permiso);
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public void EliminarUsuario(int id_usuario)
        {
            this.Conectar();
            this.GenerarStoredProcedure("eliminar_usuario");
            this.ParametroPorValor("id_usuario", id_usuario);
            this.EjecutarStoredProcedure();
            this.Desconectar();
        }

        public int LoginEsValido(string usuario, string contrasena)
        {
            int login_valido = -1;
            try
            {
                this.Conectar();
                SqlDataReader lector = this.Consulta("SELECT SIDOM.verificar_login('" + usuario + "', '" + contrasena + "') AS login_valido");
                if (lector.Read())
                    login_valido = Convert.ToInt32(lector["login_valido"]);
                this.Desconectar();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error de conexión a la base de datos: " + e.ToString());
                return -1;
            }
            return login_valido;
        }

    }
}
