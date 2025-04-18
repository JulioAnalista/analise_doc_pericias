using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class CausaDePedirBLL
    {
        private readonly CausaDePedirDAL _CausaDePedirDal;

        public CausaDePedirBLL(CausaDePedirDAL CausaDePedirDal)
        {
            _CausaDePedirDal = CausaDePedirDal;
        }

        public int Inserir(CausaDePedirENT entidade)
        {
            return _CausaDePedirDal.Inserir(entidade);
        }
    }
}