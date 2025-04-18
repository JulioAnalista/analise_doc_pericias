using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ProcessoDependenciasBLL
    {
        private readonly ProcessoDependenciasDAL _processo_dependenciasDal;

        public ProcessoDependenciasBLL(ProcessoDependenciasDAL processo_dependenciasDal)
        {
            _processo_dependenciasDal = processo_dependenciasDal;
        }

        public int Inserir(ProcessoDependenciasENT entidade)
        {
            return _processo_dependenciasDal.Inserir(entidade);
        }
    }
}