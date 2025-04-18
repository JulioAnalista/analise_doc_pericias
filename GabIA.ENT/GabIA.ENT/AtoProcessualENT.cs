using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GabIA.ENT
{
    public class AtoProcessualENT
    {
        public int IdAtoProcessual { get; set; }
        public int? IdProcesso { get; set; }
        public DateTime? Data { get; set; }
        public string Tipo { get; set; } // Ajuste para string ou um tipo enum específico
        public string Continuacao { get; set; } // Ajuste para string ou um tipo mais apropriado
        public int? Documento { get; set; }
        public bool? Publicado { get; set; } // Mudar para bool? se for um campo booleano
        public string Texto { get; set; } // Ajuste para string se for texto
        public string AssinadoPor { get; set; }
        public DateTime? DataInclusao { get; set; }
        public string Resumo { get; set; } // Mudança de nomenclatura

        [JsonProperty(Order = 1)]
        public string IdMovimento { get; set; }

        [JsonProperty(Order = 2)]
        public TipoAtoProcessualENT TipoAtoProcessual { get; set; }

        public List<string> Conteudo { get; set; } // Supondo que seja uma lista de strings
        public string TipoAto { get; set; }

        public string DescricaoAto { get; set; }
    }
}
