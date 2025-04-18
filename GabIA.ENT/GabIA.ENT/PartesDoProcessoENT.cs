using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    public class PartesDoProcessoENT
    {
        [Key]
        public int IdPartesDoProcesso { get; set; }

        // Chave estrangeira para PessoaENT
        [ForeignKey("Pessoa")]
        public int? IdPessoa { get; set; }
        public virtual PessoaENT Pessoa { get; set; }

        // IdPosicao pode ser um enum ou uma chave estrangeira dependendo da estrutura
        public int? IdPosicao { get; set; }

        public int? IdElemento { get; set; }

    }
}
