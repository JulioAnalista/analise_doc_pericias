using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class ProcessoCSV
    {
        public ProcessoCSV()
        {
            AtosProcessuais = new List<AtoProcessualENT>();
        }
        public int ID { get; set; }
        public string Tipo { get; set; }
        public string Movimento { get; set; }
        public string Processo { get; set; }
        public DateTime DataDaAbertura { get; set; }
        public int PrazoParaConsulta { get; set; }
        public string PoloAtivo { get; set; }
        public string PoloPassivo { get; set; }
        public string MembroResponsavel { get; set; }
        public string Promotoria { get; set; }
        public int idPJ { get; set; }
        public List<AtoProcessualENT> AtosProcessuais { get; set; }
    }
}
