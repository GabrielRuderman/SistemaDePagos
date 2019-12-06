using System;
using System.Windows.Forms;

namespace SistemaDePagos.Biblioteca
{
    class RellenadorDeFechas
    {
        private static RellenadorDeFechas instancia;

        // Utilizo el patrón Singleton
        public static RellenadorDeFechas GetInstance()
        {
            if (instancia == null) instancia = new RellenadorDeFechas();
            return instancia;
        }

        public void LlenarMeses(ComboBox cmb)
        {
            cmb.Items.Add("ENERO");
            cmb.Items.Add("FEBRERO");
            cmb.Items.Add("MARZO");
            cmb.Items.Add("ABRIL");
            cmb.Items.Add("MAYO");
            cmb.Items.Add("JUNIO");
            cmb.Items.Add("JULIO");
            cmb.Items.Add("AGOSTO");
            cmb.Items.Add("SEPTIEMBRE");
            cmb.Items.Add("OCTUBRE");
            cmb.Items.Add("NOVIEMBRE");
            cmb.Items.Add("DICIEMBRE");
        }

        public int MesANumero(string mes)
        {
            switch (mes)
            {
                case "ENERO": return 1;
                case "FEBRERO": return 2;
                case "MARZO": return 3;
                case "ABRIL": return 4;
                case "MAYO": return 5;
                case "JUNIO": return 6;
                case "JULIO": return 7;
                case "AGOSTO": return 8;
                case "SEPTIEMBRE": return 9;
                case "OCTUBRE": return 10;
                case "NOVIEMBRE": return 11;
                case "DICIEMBRE": return 12;
                default: return 0;
            }
        }

        public string MesATexto(int mes)
        {
            switch (mes)
            {
                case 1: return "ENERO";
                case 2: return "FEBRERO";
                case 3: return "MARZO";
                case 4: return "ABRIL";
                case 5: return "MAYO";
                case 6: return "JUNIO";
                case 7: return "JULIO";
                case 8: return "AGOSTO";
                case 9: return "SEPTIEMBRE";
                case 10: return "OCTUBRE";
                case 11: return "NOVIEMBRE";
                case 12: return "DICIEMBRE";
                default: return "";
            }
        }

        public int UltimoDiaDelMes(string mes, int ano)
        {
            return this.UltimoDiaDelMes(this.MesANumero(mes), ano);
        }

        public int UltimoDiaDelMes(int mes, int ano)
        {
            switch (mes)
            {
                case 1: return 31;
                case 2:
                    if (ano % 4 == 0 && (ano % 100 != 0 || ano % 400 == 0))
                        return 29;
                    else
                        return 28;
                case 3: return 31;
                case 4: return 30;
                case 5: return 31;
                case 6: return 30;
                case 7: return 31;
                case 8: return 31;
                case 9: return 30;
                case 10: return 31;
                case 11: return 30;
                case 12: return 31;
                default: return 1;
            }
        }

        public void LlenarAnos(ComboBox cmb)
        {
            int ano_origen = 2018;
            int ano_tope = DateTime.Today.Year + 1;
            for (int i = ano_origen; i <= ano_tope; i++)
            {
                cmb.Items.Add(i.ToString());
            }
        }
    }
}
