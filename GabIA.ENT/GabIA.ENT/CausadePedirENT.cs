using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    /// <summary>
    /// Representa a causa de pedir em um processo jur�dico.
    /// </summary>
    public class CausaDePedirENT
    {
        [Key]
        public int IdCausa { get; set; }

        /// <summary>
        /// Descri��o detalhada da causa de pedir.
        /// </summary>
        [Required]
        [StringLength(500)] // Supondo um limite de caracteres, ajuste conforme necess�rio
        public string DescricaoCausa { get; set; }
    }
}
