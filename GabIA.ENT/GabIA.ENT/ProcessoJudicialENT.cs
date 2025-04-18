using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GabIA.ENT
{
    public class ProcessoJudicialENT
    {
        [Key]
        public int Id_processo_judicial { get; set; }
        public int tribunal_distr { get; set; } // Renomeado para melhor clareza
        public int vara_distr { get; set; } // Renomeado para melhor clareza
        public int unidade_jurisdicional { get; set; } // Renomeado para melhor clareza
        public int rito { get; set; } // Renomeado para melhor clareza
        public int juiz { get; set; } // Renomeado para melhor clareza
        public bool atua_MP { get; set; } // Renomeado para melhor clareza
        public bool atua_curador_especial { get; set; } // Renomeado para melhor clareza

        public double valor_da_causa { get; set; } // Renomeado para melhor clareza
        public DateTime data_despacho { get; set; }
        public int PrazoParaConsulta { get; set; }

        // Outras propriedades relacionadas ao processo
        public int membro_MP { get; set; }
        public int idorgaoministerial { get; set; }
        public int idprocesso { get; set; } // Supondo que seja um identificador para algo específico

        // Lista de atos processuais relacionados ao processo
        public virtual ICollection<AtoProcessualENT> AtosProcessuais { get; set; }

        // Lista de elementos (partes, causas de pedir, pedidos) do processo
        public virtual ICollection<ElementosENT> Elementos { get; set; }

        // Lista de juízes associados ao processo (se aplicável)
        public virtual ICollection<JuizesENT> Juizes { get; set; }

        // Lista de membros do Ministério Público associados ao processo (se aplicável)
        public virtual ICollection<MembrosMpENT> MembrosMP { get; set; }

        public ProcessoJudicialENT()
        {
            AtosProcessuais = new HashSet<AtoProcessualENT>();
            Elementos = new HashSet<ElementosENT>();
            Juizes = new HashSet<JuizesENT>();
            MembrosMP = new HashSet<MembrosMpENT>();
        }
    }
}
