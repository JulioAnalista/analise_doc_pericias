using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class PosicoesBLL
    {
        private readonly PosicoesDAL _posicoesDal;

        public PosicoesBLL(PosicoesDAL posicoesDal)
        {
            _posicoesDal = posicoesDal;
        }

        public int Inserir(PosicoesENT entidade)
        {
            return _posicoesDal.Inserir(entidade);
        }
    }
}