using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class RepresentanteBLL
    {
        private readonly RepresentanteDAL _representanteDal;

        public RepresentanteBLL(RepresentanteDAL representanteDal)
        {
            _representanteDal = representanteDal;
        }

        public int Inserir(RepresentanteENT entidade)
        {
            return _representanteDal.Inserir(entidade);
        }
    }
}