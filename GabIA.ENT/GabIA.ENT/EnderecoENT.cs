using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class EnderecoENT
    {
        public int IdEndereco { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdLogradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public int? Bairro { get; set; }
        public int? IdCidade { get; set; }
        public int? IdEstado { get; set; }
        public int? Cep { get; set; }
        public object Residencial { get; set; }
        public object Compartilhado { get; set; }
        public string Referencia { get; set; }
        public object Atual { get; set; }
        public object CoordLat { get; set; }
        public object CoordLong { get; set; }
    }
}