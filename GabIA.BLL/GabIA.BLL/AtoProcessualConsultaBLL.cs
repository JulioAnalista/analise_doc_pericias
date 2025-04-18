using GabIA.DAL;
using GabIA.ENT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.BLL
{
    public class AtoProcessualConsultaBLL
    {
        private AtoProcessualDAL _atoProcessualDAL;

        public AtoProcessualConsultaBLL()
        {
            _atoProcessualDAL = new AtoProcessualDAL(); // Inicialize a classe DAL correspondente
        }


        public List<AtoProcessualConsulta> ObterAtoProcessualPorNumeroProcesso(string numeroProcesso)
        {

            return _atoProcessualDAL.ObterAtoProcessualPorNumeroProcesso(numeroProcesso);
        }

        public List<AtoProcessualConsulta> ObterListaAtoProcessualPorIdProcessoJ(int IdProcessoJ)
        {
            return _atoProcessualDAL.ObterListaAtoProcessualPorProcessoJudicial(IdProcessoJ);
        }


    }
}
