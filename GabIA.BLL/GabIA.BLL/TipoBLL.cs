using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TipoBLL
    {
        private readonly TipoDAL _tipoDal;

        public TipoBLL(TipoDAL tipoDal)
        {
            _tipoDal = tipoDal;
        }

        public int Inserir(TipoENT entidade)
        {
            return _tipoDal.Inserir(entidade);
        }
    }
}