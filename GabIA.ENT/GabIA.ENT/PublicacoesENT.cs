using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class PublicacoesENT
    {
        public int idpublicacoes { get; set; }
        public int? id_processo { get; set; }
        public object data { get; set; }
        public object DJe { get; set; }
        public object disponibilizacao { get; set; }
        public object pagina { get; set; }
        public object publicacao { get; set; }
    }
}