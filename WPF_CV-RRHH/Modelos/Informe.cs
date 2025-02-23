using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class Informe
    {
        public DateTime FECHA { get; set; }
        public string FK_CODIGO_EMP { get; set; }
        public Informe(DateTime fecha, string dniEmp)
        {
            FECHA = fecha;
            FK_CODIGO_EMP = dniEmp;
        }
        public Informe()
        {
            FECHA = DateTime.MinValue;
            FK_CODIGO_EMP = "";
        }
        public DateTime getFechaInf() {
            return FECHA;
        }
    }
}
