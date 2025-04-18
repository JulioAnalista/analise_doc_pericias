using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProcessoAdministrativoENT
    {
        public int Id_proc_adm { get; set; }
        public int? Membro_responsavel { get; set; }
        public int? Numero_interessados { get; set; }
    }
}