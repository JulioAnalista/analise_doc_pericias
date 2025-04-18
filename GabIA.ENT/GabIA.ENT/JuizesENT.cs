using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    public class JuizesENT
    {
        [Key]
        public int IdJuiz { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdProcessoJudicial { get; set; }

        [ForeignKey("IdProcessoJudicial")]
        public virtual ProcessoJudicialENT ProcessoJudicial { get; set; }

        public DateTime? DatetimeInicial { get; set; }
        public DateTime? DatetimeFinal { get; set; }
        // Outros métodos e propriedades...
    }
}
