using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class RedeSocialBLL
    {
        private readonly RedeSocialDAL _rede_socialDal;

        public RedeSocialBLL(RedeSocialDAL rede_socialDal)
        {
            _rede_socialDal = rede_socialDal;
        }

        public int Inserir(RedeSocialENT entidade)
        {
            return _rede_socialDal.Inserir(entidade);
        }
    }
}