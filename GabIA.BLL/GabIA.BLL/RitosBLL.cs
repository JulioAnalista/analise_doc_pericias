using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class RitosBLL
    {
        private readonly RitosDAL _ritosDal;

        public RitosBLL(RitosDAL ritosDal)
        {
            _ritosDal = ritosDal;
        }

        public int Inserir(RitosENT entidade)
        {
            return _ritosDal.Inserir(entidade);
        }
    }
}