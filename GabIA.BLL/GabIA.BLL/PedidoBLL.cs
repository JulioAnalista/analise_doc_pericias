using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class PedidoBLL
    {
        private readonly PedidoDAL _pedidoDal;

        public PedidoBLL(PedidoDAL pedidoDal)
        {
            _pedidoDal = pedidoDal;
        }

        public int Inserir(PedidoENT entidade)
        {
            return _pedidoDal.Inserir(entidade);
        }
    }
}