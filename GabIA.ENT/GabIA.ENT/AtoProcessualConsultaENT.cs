using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.ENT
{
    public class AtoProcessualConsulta
    {
        public DateTime DataInclusao { get; set; }
        public string Resumo { get; set; }
        public string IdMovimento { get; set; }
        public string Tipo { get; set; }
        public int IdAtoProcessual { get; set; }

        public ProcessoJudicialENT ProcessoJudicial { get; set; }
        // Outras propriedades e relacionamentos relevantes

        public AtoProcessualConsulta()
        {
            ProcessoJudicial = new ProcessoJudicialENT(); // Inicialização do objeto relacionado
        }
    }
}


