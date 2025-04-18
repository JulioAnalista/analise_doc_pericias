using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ParticipanteAtoBLL
    {
        private readonly ParticipanteAtoDAL _participanteatoDal;

        public ParticipanteAtoBLL(ParticipanteAtoDAL participanteatoDal)
        {
            _participanteatoDal = participanteatoDal;
        }

        public int Inserir(ParticipanteAtoENT entidade)
        {
            return _participanteatoDal.Inserir(entidade);
        }
    }
}