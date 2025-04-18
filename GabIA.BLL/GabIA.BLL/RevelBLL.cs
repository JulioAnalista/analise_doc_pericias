using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class RevelBLL
    {
        private readonly RevelDAL _revelDal;

        public RevelBLL(RevelDAL revelDal)
        {
            _revelDal = revelDal;
        }

        public int Inserir(RevelENT entidade)
        {
            return _revelDal.Inserir(entidade);
        }
    }
}