using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class MembrosMpBLL
    {
        private readonly Membros_MP_DAL _membrosmpDal;

        public MembrosMpBLL(Membros_MP_DAL membrosmpDal)
        {
            _membrosmpDal = membrosmpDal;
        }

        public int Inserir(MembrosMpENT entidade)
        {
            return _membrosmpDal.Inserir(entidade);
        }
    }
}