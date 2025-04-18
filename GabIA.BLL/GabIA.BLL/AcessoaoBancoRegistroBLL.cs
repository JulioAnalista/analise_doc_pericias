using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class AcessoaoBancoRegistroBLL
    {
        private readonly AcessoaoBancoRegistroDAL _acessoaobancoregistroDal;

        public AcessoaoBancoRegistroBLL(AcessoaoBancoRegistroDAL acessoaobancoregistroDal)
        {
            _acessoaobancoregistroDal = acessoaobancoregistroDal;
        }

        public int Inserir(AcessoaoBancoRegistroENT entidade)
        {
            return _acessoaobancoregistroDal.Inserir(entidade);
        }
    }
}