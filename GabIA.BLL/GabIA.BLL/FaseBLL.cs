using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class FaseBLL
    {
        private readonly FaseDAL _faseDal;

        public FaseBLL(FaseDAL faseDal)
        {
            _faseDal = faseDal;
        }

        public int Inserir(FaseENT entidade)
        {
            return _faseDal.Inserir(entidade);
        }
    }
}