using System;
using System.Drawing;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace SistemaDePagos.Biblioteca
{
    class Exportador
    {
        private DataGridView dgv;

        public Exportador(DataGridView dgv)
        {
            this.dgv = dgv;
        }

        public void Exportar()
        {
            this.dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dgv.MultiSelect = true;
            this.dgv.SelectAll();
            DataObject dataObj = this.dgv.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);

            Microsoft.Office.Interop.Excel.Application xlexcel;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            xlexcel = new Excel.Application();
            xlexcel.Visible = true;
            xlWorkBook = xlexcel.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            Excel.Range rango = (Excel.Range)xlWorkSheet.Cells[1, 1];
            rango.Select();
            xlWorkSheet.PasteSpecial(rango, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);

            // la primera fila en negrita, centrada y con fondo gris claro
            Excel.Range fila1 = (Excel.Range)xlWorkSheet.Rows[1];
            fila1.Select();
            fila1.EntireRow.Font.Bold = true;
            fila1.EntireRow.HorizontalAlignment = HorizontalAlignment.Center;
            fila1.EntireRow.Interior.Color = Color.LightGray;

            // si la primera celda de la primera columna está vacía, elimino la primera columna
            // esto se puede omitir, pero lo dejo para ver cómo se podrían añadir/eliminar datos a posteriori
            Excel.Range c1f1 = (Excel.Range)xlWorkSheet.Cells[1, 1];
            if (c1f1.Text == "")
            {
                Excel.Range columna1 = (Excel.Range)xlWorkSheet.Columns[1];
                columna1.Select();
                columna1.Delete();
            }

            // selecciono la primera celda de la primera columna
            Excel.Range c1 = (Excel.Range)xlWorkSheet.Cells[1, 1];
            c1.Select();

            this.dgv.ClearSelection();
            this.dgv.MultiSelect = false;
        }
    }
}
