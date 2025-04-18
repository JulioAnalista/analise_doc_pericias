using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProvasENT
    {
        public int Id_prova { get; set; }
        public int? Id_processo { get; set; }
        public DateTime? DATE_juntada { get; set; }
        public int? Id_juntada { get; set; }
        public int? Id_no_processo { get; set; }
        public string Documento { get; set; }
    }
}