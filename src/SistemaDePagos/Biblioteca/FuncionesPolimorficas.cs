using System.Windows.Forms;

namespace SistemaDePagos.Biblioteca
{
    class FuncionesPolimorficas
    {
        private static FuncionesPolimorficas instancia;

        // Utilizo el patrón Singleton
        public static FuncionesPolimorficas GetInstance()
        {
            if (instancia == null)
            {
                if (instancia == null) instancia = new FuncionesPolimorficas();
                return instancia;
            }
            return instancia;
        }

        public void CerrarApp(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro/a que desea salir de POMPY?", "Sistema de Pagos - SIDOM S.A.", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.ExitThread();
                Application.Exit();
            }
            else
                e.Cancel = true;
        }
    }
}
