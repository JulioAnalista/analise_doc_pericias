using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class PublicacoesBLL
    {
        private readonly PublicacoesDAL _publicacoesDal;

        public PublicacoesBLL(PublicacoesDAL publicacoesDal)
        {
            _publicacoesDal = publicacoesDal;
        }

        public int Inserir(PublicacoesENT entidade)
        {
            return _publicacoesDal.Inserir(entidade);
        }
    }
}