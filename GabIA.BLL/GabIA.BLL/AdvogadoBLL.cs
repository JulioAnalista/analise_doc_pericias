using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class AdvogadoBLL
    {
        private readonly AdvogadoDAL _advogadoDal;

        public AdvogadoBLL(AdvogadoDAL advogadoDal)
        {
            _advogadoDal = advogadoDal;
        }

        public int Inserir(AdvogadoENT entidade)
        {
            return _advogadoDal.Inserir(entidade);
        }
    }
}