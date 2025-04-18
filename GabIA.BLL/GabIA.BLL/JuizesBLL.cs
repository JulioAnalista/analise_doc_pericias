using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class JuizesBLL
    {
        private readonly JuizesDAL _juizesDal;

        public JuizesBLL(JuizesDAL juizesDal)
        {
            _juizesDal = juizesDal;
        }

        public int Inserir(JuizesENT entidade)
        {
            return _juizesDal.Inserir(entidade);
        }
    }
}