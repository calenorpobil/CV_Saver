using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class Informe
    {
        public int CODIGO_INF { get; set; }
        public DateTime FECHA { get; set; }
        public string FK_CODIGO_EMP { get; set; }
        public Informe(int codigo, DateTime fecha, string dniEmp)
        {
            CODIGO_INF = codigo;
            FECHA = fecha;
            FK_CODIGO_EMP = dniEmp;
        }
        public Informe(DateTime fecha, string dniEmp)
        {
            FECHA = fecha;
            FK_CODIGO_EMP = dniEmp;
        }
        public Informe()
        {
            CODIGO_INF = 0;
            FECHA = DateTime.MinValue;
            FK_CODIGO_EMP = "";
        }
        public int getCodigo_Inf() {
            return CODIGO_INF;
        }
    }
}
