using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    public class MembrosMpENT
    {
        [Key]
        public int IdMembro { get; set; }
        public int? IdPessoa { get; set; }
        public int? IdProcessoJudicial { get; set; }

        [ForeignKey("IdProcessoJudicial")]
        public virtual ProcessoJudicialENT ProcessoJudicial { get; set; }

        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public object Atual { get; set; }
        // Outros métodos e propriedades...
    }
}
