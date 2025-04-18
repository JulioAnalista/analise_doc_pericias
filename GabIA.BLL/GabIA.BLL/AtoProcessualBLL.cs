using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class AtoProcessualBLL
    {
        private readonly AtoProcessualDAL _atoprocessualDal;

        public AtoProcessualBLL(AtoProcessualDAL atoprocessualDal)
        {
            _atoprocessualDal = atoprocessualDal;
        }

        public int Inserir(AtoProcessualENT entidade)
        {
            return _atoprocessualDal.Inserir(entidade);
        }
    }
}