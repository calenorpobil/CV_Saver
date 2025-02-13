using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    class Informe
    {
        public int CODIGO_INF { get; set; }
        public DateTime FECHA { get; set; }
        public int FK_CODIGO_EMP { get; set; }
    }
}
