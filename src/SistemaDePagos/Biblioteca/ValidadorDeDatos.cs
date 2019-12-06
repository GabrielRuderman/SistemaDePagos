using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;
using SistemaDePagos.Dominio;

namespace SistemaDePagos.Biblioteca
{
    class ValidadorDeDatos
    {
        private static ValidadorDeDatos instancia;
        private static GestorDB gestor = GestorDB.GetInstance();
        private string mensaje;
        private bool esta_correcto;

        // Utilizo el patrón Singleton
        public static ValidadorDeDatos GetInstance()
        {
            if (instancia == null) instancia = new ValidadorDeDatos();
            return instancia;
        }

        public void NuevaValidacion(string mensaje)
        {
            this.mensaje = mensaje;
            this.esta_correcto = true;
        }

        public void ValidarAlfanumerico(string texto, string descripcion)
        {
            if (texto == "")
            {
                this.mensaje += descripcion;
                this.esta_correcto = false;
            }
        }

        public void ValidarNumericoObligatorio(string texto, string descripcion)
        {
            if (texto == "")
            {
                this.mensaje += descripcion;
                this.esta_correcto = false;
            }
            else
            {
                try
                {
                    Convert.ToDouble(texto);
                }
                catch
                {
                    this.mensaje += descripcion;
                    this.esta_correcto = false;
                }
            }
        }

        public void ValidarNumericoOpcional(string texto, string descripcion)
        {
            if (texto != "")
            {
                try
                {
                    Convert.ToDouble(texto);
                }
                catch
                {
                    this.mensaje += descripcion;
                    this.esta_correcto = false;
                }
            }
        }

        public void ValidarCombo(object item, ComboBox.ObjectCollection lista_items, string descripcion)
        {
            if (!lista_items.Contains(item))
            {
                this.mensaje += descripcion;
                this.esta_correcto = false;
            }
        }

        public bool ItemRepetido(string item, List<string> lista_items)
        {
            return lista_items.Contains(item);
        }

        public bool EstaCorrecto()
        {
            return this.esta_correcto;
        }

        public string MensajeFinal()
        {
            return this.mensaje;
        }

        public bool FacturaEsCorrecta(Pago pago)
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta("SELECT numero_de_factura, persona FROM SIDOM.pagos WHERE numero_de_factura = '" + pago.NumeroDeFactura() + "' AND persona = '" + pago.Persona() + "'");
            bool retorno = !lector.Read();
            gestor.Desconectar();
            return retorno;
        }

        public bool MontoEsAcorde(Pago pago, ref double ultimo_monto) // Si esta dentro del 20% superior o inferior del ultimo monto cargado es acorde
        {
            gestor.Conectar();
            SqlDataReader lector = gestor.Consulta("SELECT TOP 1 monto FROM SIDOM.pagos WHERE persona = '" + pago.Persona() +
                                                   "' AND sucursal = '" + pago.Sucursal() + "' AND rubro = '" + pago.Rubro() +
                                                   "' AND periodo_prestacion_real = CONVERT(DATE, '" + pago.PeriodoPrestacionReal().AddMonths(-1) +
                                                   "') ORDER BY id_pago DESC");
            if (!lector.Read()) return true;
            ultimo_monto = Double.Parse(lector["monto"].ToString());
            double nuevo_monto = Double.Parse(pago.Monto().ToString());
            gestor.Desconectar();
            return (nuevo_monto >= ultimo_monto * 0.80) && (nuevo_monto <= ultimo_monto * 1.20);
        }

        // -------------------------- VALIDACIÓN EN TIEMPO REAL --------------------------

        public void CaracterDeTexto(KeyPressEventArgs e)
        {
            if (Char.IsLetter(e.KeyChar) || Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        public void CaracterNumerico(KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar) || Char.Equals(e.KeyChar, ','))
                e.Handled = false;
            else
                e.Handled = true;
        }

        public void CaracterAlfanumerico(KeyPressEventArgs e)
        {
            if (Char.IsLetterOrDigit(e.KeyChar) || Char.IsControl(e.KeyChar))
                e.Handled = false;
            else
                e.Handled = true;
        }

        public void CaracterAlfanumericoConEspacio(KeyPressEventArgs e)
        {
            if (Char.IsLetterOrDigit(e.KeyChar) || Char.IsControl(e.KeyChar) || Char.IsWhiteSpace(e.KeyChar))
                e.Handled = false;
            else
                e.Handled = true;
        }

        // -------------------------- CONVERSIONES --------------------------

        public decimal ObtenerFormatoDecimal(string valor)
        {
            return Decimal.Round(Decimal.Parse(valor.Replace(',', '.'), CultureInfo.InvariantCulture), 2);
        }

        public string ObtenerFormatoMoneda(object valor)
        {
            decimal conversion = this.ObtenerFormatoDecimal(valor.ToString());
            return conversion.ToString("C").Substring(0, conversion.ToString("C").Length - 2);
        }

    }
}
