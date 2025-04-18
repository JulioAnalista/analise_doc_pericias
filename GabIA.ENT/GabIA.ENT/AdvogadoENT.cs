using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class AdvogadoENT
    {
        public int IdAdvogado { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdEndereco { get; set; }
        public int? IdTelefone { get; set; }
        public int? IdRede { get; set; }
        public int? NrOab { get; set; }
        public int? OabUf { get; set; }
        public object Atual { get; set; }
    }
}