using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ProvasBLL
    {
        private readonly ProvasDAL _provasDal;

        public ProvasBLL(ProvasDAL provasDal)
        {
            _provasDal = provasDal;
        }

        public int Inserir(ProvasENT entidade)
        {
            return _provasDal.Inserir(entidade);
        }
    }
}