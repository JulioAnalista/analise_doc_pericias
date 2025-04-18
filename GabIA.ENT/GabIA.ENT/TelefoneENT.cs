using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class TelefoneENT
    {
        public int Id_telefone { get; set; }
        public int? Id_pessoa { get; set; }
        public string Numero { get; set; }
        public object Id_tipo { get; set; }
    }
}