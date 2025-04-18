using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class DocumentodeIdentificacaoENT
    {
        public int IdDocumento { get; set; }
        public int? IdPessoa { get; set; }
        public object Nome { get; set; }
        public DateTime? DataExp { get; set; }
        public int? IdNum { get; set; }
        public string Numero { get; set; }
    }
}