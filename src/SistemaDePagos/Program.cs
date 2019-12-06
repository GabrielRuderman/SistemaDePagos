using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SistemaDePagos;

namespace SistemaDePagos
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool activo;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "SistemaDePagos", out activo);

            if (!activo)
            {
                MessageBox.Show("Ya hay una instancia de la aplicación abierta.", "Sistema de Pagos - SIDOM S.A.");
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormPresentacion()); // FormPresentacion
            }
            // Liberamos la exclusión mutua
            mutex.ReleaseMutex();
        }
    }
}
