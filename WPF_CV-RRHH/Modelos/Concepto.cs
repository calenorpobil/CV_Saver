using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class Concepto
    {
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public DateTime FK_CODIGO_INF { get; set; }

        public Concepto() { 
            Nombre= String.Empty; Descripcion= String.Empty; FK_CODIGO_INF = DateTime.MinValue;
        }
        public Concepto(string nombre, string desc, DateTime informe)
        {
            Nombre = nombre;
            Descripcion = desc;
            FK_CODIGO_INF = informe;
        }
        public String getNombre()
        {
            return Nombre;
        }

        public String getDescripcion()
        {
            return Descripcion;
        }

        public DateTime getCodigo()
        {
            return FK_CODIGO_INF;
        }

    }
}
