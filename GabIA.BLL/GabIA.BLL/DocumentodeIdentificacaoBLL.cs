using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class DocumentodeIdentificacaoBLL
    {
        private readonly DocumentodeIdentificacaoDAL _documentodeidentificacaoDal;

        public DocumentodeIdentificacaoBLL(DocumentodeIdentificacaoDAL documentodeidentificacaoDal)
        {
            _documentodeidentificacaoDal = documentodeidentificacaoDal;
        }

        public int Inserir(DocumentodeIdentificacaoENT entidade)
        {
            return _documentodeidentificacaoDal.Inserir(entidade);
        }
    }
}