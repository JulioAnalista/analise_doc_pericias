using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class RepresentanteENT
    {
        public int id_representante { get; set; }
        public int? id_advogado { get; set; }
        public int? processo { get; set; }
        public int? id_representado { get; set; }
        public int? mandato { get; set; }
    }
}