using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class Empleado
    {
        public int CodigoEmp { get; set; }
        public string Nombre { get; set; }
        public string DNI { get; set; }

        public Empleado() 
        { 
            CodigoEmp = 0;
            Nombre = "";
            DNI = "";
        }

    }
}
