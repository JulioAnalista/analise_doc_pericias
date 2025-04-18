using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ProcessoJudicialBLL
    {
        private readonly ProcessoJudicialDAL _processo_JudicialDal;

        public ProcessoJudicialBLL(ProcessoJudicialDAL processo_judicialDal)
        {
            _processo_JudicialDal = processo_judicialDal;
        }

        public int Inserir(ProcessoJudicialENT entidade)
        {
            return _processo_JudicialDal.Inserir(entidade);
        }
    }
}