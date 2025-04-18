using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TipoTelefoneBLL
    {
        private readonly TipoTelefoneDAL _tipo_telefoneDal;

        public TipoTelefoneBLL(TipoTelefoneDAL tipo_telefoneDal)
        {
            _tipo_telefoneDal = tipo_telefoneDal;
        }

        public int Inserir(TipoTelefoneENT entidade)
        {
            return _tipo_telefoneDal.Inserir(entidade);
        }
    }
}