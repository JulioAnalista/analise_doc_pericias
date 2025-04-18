using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    /// <summary>
    /// Representa os elementos de um processo judicial.
    /// </summary>
    public class ElementosENT
    {
        [Key]
        public int IdElemento { get; set; }

        /// <summary>
        /// Identificador do processo judicial ao qual estes elementos estão associados.
        /// </summary>
        [ForeignKey("ProcessoJudicial")]
        public int IdProcessoJudicial { get; set; }

        /// <summary>
        /// Processo judicial ao qual estes elementos estão associados.
        /// </summary>
        public virtual ProcessoJudicialENT ProcessoJudicial { get; set; }

        /// <summary>
        /// Lista de partes do processo associadas a estes elementos.
        /// </summary>
        public virtual ICollection<PartesDoProcessoENT> PartesDoProcesso { get; set; }

        /// <summary>
        /// Lista de causas de pedir associadas a estes elementos.
        /// </summary>
        public virtual ICollection<CausaDePedirENT> CausasDePedir { get; set; }

        /// <summary>
        /// Lista de pedidos associados a estes elementos.
        /// </summary>
        public virtual ICollection<PedidoENT> Pedidos { get; set; }

        /// <summary>
        /// Construtor padrão.
        /// </summary>
        public ElementosENT()
        {
            PartesDoProcesso = new List<PartesDoProcessoENT>();
            CausasDePedir = new List<CausaDePedirENT>();
            Pedidos = new List<PedidoENT>();
        }
    }
}
