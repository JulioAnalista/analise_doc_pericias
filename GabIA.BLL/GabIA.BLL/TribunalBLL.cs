using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TribunalBLL
    {
        private readonly TribunalDAL _tribunailDal;

        public TribunalBLL(TribunalDAL tribunailDal)
        {
            _tribunailDal = tribunailDal;
        }

        public int Inserir(TribunalENT entidade)
        {
            return _tribunailDal.Inserir(entidade);
        }
    }
}