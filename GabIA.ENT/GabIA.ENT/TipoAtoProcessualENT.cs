using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace GabIA.ENT
{
    public class TipoAtoProcessualENT
    {
        public object Id_Tipo_Ato_processual { get; set; }
        public string Tipo { get; set; }

        [JsonProperty(Order = 1)]
        public string Descricao { get; set; }
    }
}