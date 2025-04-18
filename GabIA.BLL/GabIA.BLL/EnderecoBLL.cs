using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class EnderecoBLL
    {
        private readonly EnderecoDAL _enderecoDal;

        public EnderecoBLL(EnderecoDAL enderecoDal)
        {
            _enderecoDal = enderecoDal;
        }

        public int Inserir(EnderecoENT entidade)
        {
            return _enderecoDal.Inserir(entidade);
        }
    }
}