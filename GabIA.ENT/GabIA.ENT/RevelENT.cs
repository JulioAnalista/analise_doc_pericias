using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class RevelENT
    {
        public int id_revel { get; set; }
        public int? Id_processo { get; set; }
        public int? Id_pessoa { get; set; }
        public object Data_revelia { get; set; }
        public object Nomeado_curador { get; set; }
        public int? Id_curador { get; set; }
    }
}