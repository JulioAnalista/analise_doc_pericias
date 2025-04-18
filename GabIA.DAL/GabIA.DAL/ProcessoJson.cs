using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProcessoJson
    {
        public string Titulo { get; set; }
        public string Consulta { get; set; }
        public string Data { get; set; }
        public string Numero { get; set; }
        public string Classe { get; set; }
        public string OrgaoJulgador { get; set; }
        public string ValorCausa { get; set; }
        public string Assuntos { get; set; }
        public bool SegredoJustica { get; set; }
    }

}
