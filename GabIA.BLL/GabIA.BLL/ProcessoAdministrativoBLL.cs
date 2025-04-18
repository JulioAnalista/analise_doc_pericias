using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ProcessoAdministrativoBLL
    {
        private readonly ProcessoAdministrativoDAL _processoadministrativoDal;

        public ProcessoAdministrativoBLL(ProcessoAdministrativoDAL processoadministrativoDal)
        {
            _processoadministrativoDal = processoadministrativoDal;
        }

        public int Inserir(ProcessoAdministrativoENT entidade)
        {
            return _processoadministrativoDal.Inserir(entidade);
        }
    }
}