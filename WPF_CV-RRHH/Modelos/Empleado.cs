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
        public ArrayList empleadoList { get; set; }

        private string Nombre { get; set; }
        private string DNI { get; set; }

        public Empleado() 
        { 

            Nombre = "";
            DNI = "";
        }
        private void AddEmpleado(Informe i)
        {
            empleadoList.Add(i);
        }
        public ArrayList GetEmpleados()
        {
            return empleadoList;
        }

        public Empleado(ArrayList informeList, string nombre, string dNI)
        {
            this.empleadoList = informeList;
            Nombre = nombre;
            DNI = dNI;
        }
        public Empleado(string nombre, string dNI)
        {
            Nombre = nombre;
            DNI = dNI;
        }

        public string getNombre()
        {
            return Nombre;
        }
        public string getDni()
        {
            return DNI;
        }
    }
}
