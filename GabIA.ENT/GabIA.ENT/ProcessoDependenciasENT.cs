using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProcessoDependenciasENT
    {
        public int Id_dependencia { get; set; }
        public int? Id_processo { get; set; }
        public int? Id_processo_dependente { get; set; }
        public int? Id_tipo_dependendia { get; set; }
    }
}