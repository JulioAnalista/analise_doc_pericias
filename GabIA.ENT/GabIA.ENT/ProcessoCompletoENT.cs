using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GabIA.ENT
{
    public class ProcessoCompletoENT
    {
        [Key]
        public int IdProcessoCompleto { get; set; }

        public string Tipo { get; set; }
        public string Movimento { get; set; }
        public string NumeroProcesso { get; set; }
        public DateTime DataDaAbertura { get; set; }
        public int PrazoParaConsulta { get; set; }

        // Alterado para coleções
        public ICollection<string> PoloAtivo { get; set; }
        public ICollection<string> PoloPassivo { get; set; }
        public string Classe { get; set; }

        // Propriedades relacionadas a entidades jurídicas e administrativas
        public string MembroResponsavel { get; set; }
        public string Promotoria { get; set; }
        public int IdPJ { get; set; }

        public int IdP { get; set; }

        // Lista de atos processuais do processo completo
        public virtual ICollection<AtoProcessualENT> AtosProcessuais { get; set; }

        // Processo Judicial associado
        public virtual ProcessoJudicialENT ProcessoJudicial { get; set; }

        // Construtor
        public ProcessoCompletoENT()
        {
            AtosProcessuais = new HashSet<AtoProcessualENT>();
            // Inicializando as coleções para evitar NullReferenceException
            PoloAtivo = new List<string>();
            PoloPassivo = new List<string>();
        }

        // Métodos adicionais conforme necessário...
    }
}
