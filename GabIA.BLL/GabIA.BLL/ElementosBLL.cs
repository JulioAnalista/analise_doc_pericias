using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ElementosBLL
    {
        private readonly ElementosDAL _elementosDal;

        public ElementosBLL(ElementosDAL elementosDal)
        {
            _elementosDal = elementosDal;
        }

        public int Inserir(ElementosENT entidade)
        {
            return _elementosDal.Inserir(entidade);
        }
    }
}