using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TipoAtoProcessualBLL
    {
        private readonly TipoAtoProcessualDAL _tipo_ato_processualDal;

        public TipoAtoProcessualBLL(TipoAtoProcessualDAL tipo_ato_processualDal)
        {
            _tipo_ato_processualDal = tipo_ato_processualDal;
        }

        public int Inserir(TipoAtoProcessualENT entidade)
        {
            return _tipo_ato_processualDal.Inserir(entidade);
        }
    }
}