using System;
using System.Windows.Forms;

namespace SistemaDePagos.Dominio
{
    class DataGridViewClonable : ICloneable
    {
        DataGridView dgv;

        public DataGridViewClonable(DataGridView dgv)
        {
            this.dgv = dgv;
        }

        public DataGridView Dgv()
        {
            return this.dgv;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
