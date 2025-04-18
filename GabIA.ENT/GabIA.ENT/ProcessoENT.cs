using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProcessoENT
    {
        public int id_processo { get; set; }
        public string? numero_processo { get; set; }
        public byte Tipo { get; set; }
        public DateTime Ultimo_movimento { get; set; }
        public DateTime data_distribuicao { get; set; }
        public string? classe { get; set; }
        public string? classepublicidade { get; set; }
    }
}
