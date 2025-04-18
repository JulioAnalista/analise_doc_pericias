using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabIA.ENT
{
    /// <summary>
    /// Representa um pedido em um processo jurídico.
    /// </summary>
    public class PedidoENT
    {
        [Key]
        public int IdPedido { get; set; }

        /// <summary>
        /// Detalhes do pedido.
        /// </summary>
        [Required]
        [StringLength(500)] // limite de caracteres
        public string Pedido { get; set; }

    }
}
