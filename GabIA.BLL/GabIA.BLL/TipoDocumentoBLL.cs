using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TipoDocumentoBLL
    {
        private readonly TipoDocumentoDAL _tipo_documentoDal;

        public TipoDocumentoBLL(TipoDocumentoDAL tipo_documentoDal)
        {
            _tipo_documentoDal = tipo_documentoDal;
        }

        public int Inserir(TipoDocumentoENT entidade)
        {
            return _tipo_documentoDal.Inserir(entidade);
        }
    }
}