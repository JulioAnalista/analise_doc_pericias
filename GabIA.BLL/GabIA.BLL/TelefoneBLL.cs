using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TelefoneBLL
    {
        private readonly TelefoneDAL _telefoneDal;

        public TelefoneBLL(TelefoneDAL telefoneDal)
        {
            _telefoneDal = telefoneDal;
        }

        public int Inserir(TelefoneENT entidade)
        {
            return _telefoneDal.Inserir(entidade);
        }
    }
}