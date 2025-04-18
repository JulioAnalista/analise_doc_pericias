using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class TipoDependenciaBLL
    {
        private readonly TipoDependenciaDAL _tipo_dependenciaDal;

        public TipoDependenciaBLL(TipoDependenciaDAL tipo_dependenciaDal)
        {
            _tipo_dependenciaDal = tipo_dependenciaDal;
        }

        public int Inserir(TipoDependenciaENT entidade)
        {
            return _tipo_dependenciaDal.Inserir(entidade);
        }
    }
}