using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SistemaDePagos.Biblioteca
{
    class MatrizEstadistica
    {
        private int cant_filas;
        private int cant_columnas;
        private object[,] matriz;
        private int fila_actual;
        private int columna_actual;
        private ValidadorDeDatos validadorDeDatos = ValidadorDeDatos.GetInstance();

        public MatrizEstadistica(List<string> criterio, int cant_columnas)
        {
            this.cant_filas = criterio.Count; // Sumo 1 por los totales de columna
            this.cant_columnas = cant_columnas + 1; // Sumo 1 por los encabezados laterales
            this.matriz = new object[this.cant_filas, this.cant_columnas];
            this.fila_actual = 0;
            this.columna_actual = 0;
            this.CompletarEncabezados(this.matriz, criterio);
        }

        private void CompletarEncabezados(object[,] matriz, List<string> criterio)
        {
            for (int i = 0; i < this.cant_filas; i++)
            {
                this.matriz[i, 0] = criterio[i];
            }
            this.columna_actual++;
        }

        public int CantidadDeFilas()
        {
            return this.cant_filas;
        }

        public void AgregarColumna(SqlDataReader lector)
        {
            double total = 0;
            int i;
            for (i = 0; i < this.cant_filas - 1; i++)
            {
                if (lector.Read())
                {
                    double valor = Convert.ToDouble(lector["total"].ToString());
                    matriz[i, this.columna_actual] = valor;
                    total += valor;
                }
                else
                    matriz[i, this.columna_actual] = 0;
            }
            matriz[i, this.columna_actual] = total;
            this.columna_actual++;
        }

        public object[] ObtenerFila()
        {
            /* PARA OBTENER TOTAL POR FILA (comentado) */
            //object[] row = new object[this.cant_columnas + 1];
            //double total = 0;

            object[] row = new object[this.cant_columnas];

            for (int j = 0; j < this.cant_columnas; j++)
            {
                row[j] = this.matriz[this.fila_actual, j];
                //if (j > 0) total += Convert.ToDouble(row[j]); // j = 0 => ENCABEZADO FILA
            }
            //row[row.Length - 1] = total;
            this.fila_actual++;

            // Una vez armada la row, la formateo a moneda
            for (int i = 1; i < row.Length; i++)
                row[i] = validadorDeDatos.ObtenerFormatoMoneda(row[i]);

            return row;
        }
    }
}
