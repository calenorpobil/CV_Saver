using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class Empleado
    {
        public ArrayList informeList { get; set; }

        private int CodigoEmp { get; set; }
        private string Nombre { get; set; }
        private string DNI { get; set; }

        public Empleado() 
        { 

            CodigoEmp = 0;
            Nombre = "";
            DNI = "";
        }
        private void AddInforme(Informe i)
        {
            informeList.Add(i);
        }
        public ArrayList GetInformes()
        {
            return informeList;
        }

        public Empleado(ArrayList informeList, int codigoEmp, string nombre, string dNI)
        {
            this.informeList = informeList;
            CodigoEmp = codigoEmp;
            Nombre = nombre;
            DNI = dNI;
        }
    }
}
