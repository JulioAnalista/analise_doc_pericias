using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    [Table("Elementos")]
    public class ElementosENT
    {
        [Key]
        public int IdElemento { get; set; }

        // Adicionando chave estrangeira para associar Elementos a um ProcessoJudicial
        public int IdProcessoJudicial { get; set; }

        // Relacionamento um-para-muitos com PartesdoProcessoENT
        [InverseProperty("Elementos")]
        public virtual ICollection<PartesDoProcessoENT> PartesDoProcesso { get; set; }

        // Relacionamento um-para-muitos com CausaDePedirENT
        [InverseProperty("Elementos")]
        public virtual ICollection<CausaDePedirENT> CausasDePedir { get; set; }

        // Relacionamento um-para-muitos com PedidoENT
        [InverseProperty("Elementos")]
        public virtual ICollection<PedidoENT> Pedidos { get; set; }

        // Adicionando a propriedade de navegação para o ProcessoJudicial
        [ForeignKey("IdProcessoJudicial")]
        public virtual ProcessoJudicialENT ProcessoJudicial { get; set; }

        public ElementosENT()
        {
            PartesDoProcesso = new HashSet<PartesDoProcessoENT>();
            CausasDePedir = new HashSet<CausaDePedirENT>();
            Pedidos = new HashSet<PedidoENT>();
        }
    }
}


