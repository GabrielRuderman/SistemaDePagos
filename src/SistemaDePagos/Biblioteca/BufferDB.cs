using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using SistemaDePagos.Forms.Proyectado;

namespace SistemaDePagos.Biblioteca
{
    class BufferDB
    {
        private static BufferDB instancia;
        private static GestorDB gestor = GestorDB.GetInstance();
        private int permisos;
        private Form formPrincipal, formProyectado = null;
        private HashSet<string> sucursales = new HashSet<string>();
        private HashSet<string> rubros = new HashSet<string>();
        private HashSet<string> mediosDePago = new HashSet<string>();
        private HashSet<string> bancos = new HashSet<string>();
        private HashSet<string> personas = new HashSet<string>();
        string query_sucursales =
                "SELECT descripcion " +
                "FROM SIDOM.sucursales " +
                "ORDER BY descripcion ASC";
        string query_rubros =
                "SELECT descripcion " +
                "FROM SIDOM.rubros " +
                "ORDER BY descripcion ASC";
        string query_mediosDePago =
                "SELECT descripcion " +
                "FROM SIDOM.medios_de_pago " +
                "ORDER BY descripcion ASC";
        string query_bancos =
                "SELECT descripcion " +
                "FROM SIDOM.bancos " +
                "ORDER BY descripcion ASC";
        string query_personas =
                "SELECT descripcion " +
                "FROM SIDOM.personas " +
                "ORDER BY descripcion ASC";

        // Utilizo el patrón Singleton
        public static BufferDB GetInstance()
        {
            if (instancia == null)
            {
                instancia = new BufferDB();
                instancia.Llenar();
            }
            return instancia;
        }

        public int Permisos()
        {
            return this.permisos;
        }

        public void Permisos(int permisos)
        {
            this.permisos = permisos;
        }

        public Form FormPrincipal()
        {
            return this.formPrincipal;
        }

        public void FormPrincipal(Form formPrincipal)
        {
            this.formPrincipal = formPrincipal;
        }

        public Form FormProyectado()
        {
            if (this.formProyectado == null) this.formProyectado = new FormProyectado();
            return this.formProyectado;
        }

        public void FormProyectado(Form formProyectado)
        {
            this.formProyectado = formProyectado;
        }

        private void Llenar()
        {
            this.CargarLista(this.sucursales, this.query_sucursales);
            this.CargarLista(this.rubros, this.query_rubros);
            this.CargarLista(this.mediosDePago, this.query_mediosDePago);
            this.CargarLista(this.bancos, this.query_bancos);
            this.CargarLista(this.personas, this.query_personas);
        }

        public void RecargarSucursales()
        {
            this.sucursales.Clear();
            this.CargarLista(this.sucursales, this.query_sucursales);
        }

        public void RecargarRubros()
        {
            this.rubros.Clear();
            this.CargarLista(this.rubros, this.query_rubros);
        }

        public void RecargarMediosDePago()
        {
            this.mediosDePago.Clear();
            this.CargarLista(this.mediosDePago, this.query_mediosDePago);
        }

        public void RecargarBancos()
        {
            this.bancos.Clear();
            this.CargarLista(this.bancos, this.query_bancos);
        }

        public void RecargarPersonas()
        {
            this.personas.Clear();
            this.CargarLista(this.personas, this.query_personas);
        }

        private void CargarLista(HashSet<string> lista, string query)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta(query);
            while (lector.Read())
            {
                lista.Add(lector["descripcion"].ToString());
            }
            gestor.Desconectar();
        }

        public HashSet<string> Sucursales()
        {
            return this.sucursales;
        }

        public HashSet<string> Rubros()
        {
            return this.rubros;
        }

        public HashSet<string> MediosDePago()
        {
            return this.mediosDePago;
        }

        public HashSet<string> Bancos()
        {
            return this.bancos;
        }

        public HashSet<string> Personas()
        {
            return this.personas;
        }
    }
}
