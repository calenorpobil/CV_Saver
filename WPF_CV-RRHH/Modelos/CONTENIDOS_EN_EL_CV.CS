using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_CV_RRHH.Modelos
{
    public class CONTENIDOS_EN_EL_CV
    {
        public String RutaArchivo { get; set; }
        public String TipoMime {  get; set; }
        public DateTime FK_CODIGO_INF { get; set; }

        public CONTENIDOS_EN_EL_CV() { }
        public CONTENIDOS_EN_EL_CV(string ruta, string tipo, DateTime informe)
        {
            RutaArchivo = ruta;
            TipoMime = tipo;
            FK_CODIGO_INF = informe;
        }

    }
}
