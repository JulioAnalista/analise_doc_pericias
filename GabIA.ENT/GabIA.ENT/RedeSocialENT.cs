using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class RedeSocialENT
    {
        public int Id_rede { get; set; }
        public int? Id_pessoa { get; set; }
        public string Rede { get; set; }
        public string Endereco { get; set; }
    }
}