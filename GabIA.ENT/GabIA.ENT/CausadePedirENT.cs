using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    /// <summary>
    /// Representa a causa de pedir em um processo jurídico.
    /// </summary>
    public class CausaDePedirENT
    {
        [Key]
        public int IdCausa { get; set; }

        /// <summary>
        /// Descrição detalhada da causa de pedir.
        /// </summary>
        [Required]
        [StringLength(500)] // Supondo um limite de caracteres, ajuste conforme necessário
        public string DescricaoCausa { get; set; }
    }
}
