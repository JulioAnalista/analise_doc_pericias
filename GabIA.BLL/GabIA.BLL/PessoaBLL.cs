using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class PessoaBLL
    {
        private readonly PessoaDAL _pessoaDal;

        public PessoaBLL(PessoaDAL pessoaDal)
        {
            _pessoaDal = pessoaDal;
        }

        public int Inserir(PessoaENT entidade)
        {
            return _pessoaDal.Inserir(entidade);
        }
    }
}