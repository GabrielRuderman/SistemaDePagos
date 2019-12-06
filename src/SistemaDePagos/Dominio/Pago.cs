using System;
using SistemaDePagos.Biblioteca;

namespace SistemaDePagos.Dominio
{
    public class Pago
    {
        private static RellenadorDeDatos rellenadorDeDatos = RellenadorDeDatos.GetInstance();

        private readonly int id;
        private DateTime periodoPrestacionReal;
        private bool fechaPagoRealChecked;
        private DateTime fechaPagoReal;
        private string numeroDeFactura; 
        private string medioDePago;
        private string banco;
        private int numeroDeCheque;
        private decimal monto;
        private decimal retencionesGanancias;
        private decimal retencionesIngresosBrutosCaba;
        private decimal retencionesIngresosBrutosProvincia;
        private string persona;
        private string sucursal;
        private string rubro;
        private string observaciones;

        public Pago() { }

        public Pago(int id, DateTime periodo_prestacion_real, DateTime fecha_pago_real, string numero_de_factura, string medio_de_pago, string banco, int numero_de_cheque, int monto, int retenciones_ganancias, int retenciones_ingresos_brutos_caba, int retenciones_ingresos_brutos_provincia, string persona, string sucursal, string rubro, string observaciones)
        {
            this.id = id;
            this.periodoPrestacionReal = periodo_prestacion_real;
            this.fechaPagoReal = fecha_pago_real;
            this.numeroDeFactura = numero_de_factura;
            this.medioDePago = medio_de_pago;
            this.banco = banco;
            this.numeroDeCheque = numero_de_cheque;
            this.monto = monto;
            this.retencionesGanancias = retenciones_ganancias;
            this.retencionesIngresosBrutosCaba = retenciones_ingresos_brutos_caba;
            this.retencionesIngresosBrutosProvincia = retenciones_ingresos_brutos_provincia;
            this.persona = persona;
            this.sucursal = sucursal;
            this.rubro = rubro;
            this.observaciones = observaciones;
        }

        // Se usa cuando se crea un nuevo Pago
        public Pago(DateTime periodo_prestacion_real, bool fecha_pago_real_checked, DateTime fecha_pago_real, string sucursal, string rubro)
        {
            this.periodoPrestacionReal = periodo_prestacion_real;
            this.fechaPagoReal = fecha_pago_real;
            this.fechaPagoRealChecked = fecha_pago_real_checked;
            this.sucursal = sucursal;
            this.rubro = rubro;
            this.numeroDeCheque = 0;
            this.retencionesGanancias = 0;
            this.retencionesIngresosBrutosCaba = 0;
            this.retencionesIngresosBrutosProvincia = 0;
        }

        // Se usa cuando se modifica un pago existente
        public Pago(int id)
        {
            this.id = id;
            this.CargarValores();
        }

        private void CargarValores()
        {
            string query = "SELECT * FROM SIDOM.pagos WHERE id_pago = " + this.Id();
            rellenadorDeDatos.CargarPago(this, query);
        }

        public int Id()
        {
            return this.id;
        }

        public DateTime PeriodoPrestacionReal()
        {
            return this.periodoPrestacionReal;
        }

        public void PeriodoPrestacionReal(DateTime periodoPrestacionReal)
        {
            this.periodoPrestacionReal = periodoPrestacionReal;
        }

        public bool FechaPagoRealChecked()
        {
            return this.fechaPagoRealChecked;
        }

        public void FechaPagoRealChecked(bool fechaPagoRealChecked)
        {
            this.fechaPagoRealChecked = fechaPagoRealChecked;
        }

        public DateTime FechaPagoReal()
        {
            return this.fechaPagoReal;
        }

        public void FechaPagoReal(DateTime fechaPagoReal)
        {
            this.fechaPagoReal = fechaPagoReal;
        }

        public string NumeroDeFactura()
        {
            return this.numeroDeFactura;
        }

        public void NumeroDeFactura(string numeroDeFactura)
        {
            this.numeroDeFactura = numeroDeFactura;
        }

        public string MedioDePago()
        {
            return this.medioDePago;
        }

        public void MedioDePago(string medioDePago)
        {
            this.medioDePago = medioDePago;
        }

        public string Banco()
        {
            return this.banco;
        }

        public void Banco(string banco)
        {
            this.banco = banco;
        }

        public int NumeroDeCheque()
        {
            return this.numeroDeCheque;
        }

        public void NumeroDeCheque(int numeroDeCheque)
        {
            this.numeroDeCheque = numeroDeCheque;
        }

        public decimal Monto()
        {
            return this.monto;
        }

        public void Monto(decimal monto)
        {
            this.monto = monto;
        }

        public decimal RetencionesGanancias()
        {
            return this.retencionesGanancias;
        }

        public void RetencionesGanancias(decimal retencionesGanancias)
        {
            this.retencionesGanancias = retencionesGanancias;
        }

        public decimal RetencionesIngresosBrutosCaba()
        {
            return this.retencionesIngresosBrutosCaba;
        }

        public void RetencionesIngresosBrutosCaba(decimal retencionesIngresosBrutosCaba)
        {
            this.retencionesIngresosBrutosCaba = retencionesIngresosBrutosCaba;
        }

        public decimal RetencionesIngresosBrutosProvincia()
        {
            return this.retencionesIngresosBrutosProvincia;
        }

        public void RetencionesIngresosBrutosProvincia(decimal retencionesIngresosBrutosProvincia)
        {
            this.retencionesIngresosBrutosProvincia = retencionesIngresosBrutosProvincia;
        }

        public string Persona()
        {
            return this.persona;
        }

        public void Persona(string persona)
        {
            this.persona = persona;
        }

        public string Sucursal()
        {
            return this.sucursal;
        }

        public void Sucursal(string sucursal)
        {
            this.sucursal = sucursal;
        }

        public string Rubro()
        {
            return this.rubro;
        }

        public void Rubro(string rubro)
        {
            this.rubro = rubro;
        }

        public string Observaciones()
        {
            return this.observaciones;
        }

        public void Observaciones(string observaciones)
        {
            this.observaciones = observaciones;
        }

    }
}
