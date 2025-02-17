using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    class CONTENIDOS_EN_EL_CV
    {
        public int Id {  get; set; }
        public String RutaArchivo { get; set; }
        public String TipoMime {  get; set; }
        public int FK_CODIGO_INF { get; set; }

    }
}
