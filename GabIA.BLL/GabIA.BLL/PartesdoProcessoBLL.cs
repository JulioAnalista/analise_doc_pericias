using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class PartesdoProcessoBLL
    {
        private readonly PartesDoProcessoDAL _partesdoprocessoDal;

        public PartesdoProcessoBLL(PartesDoProcessoDAL partesdoprocessoDal)
        {
            _partesdoprocessoDal = partesdoprocessoDal;
        }

        public int Inserir(PartesDoProcessoENT entidade)
        {
            return _partesdoprocessoDal.Inserir(entidade);
        }
    }
}