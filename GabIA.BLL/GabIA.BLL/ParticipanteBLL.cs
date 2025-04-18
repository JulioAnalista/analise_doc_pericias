using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.DAL;
using GabIA.ENT;

namespace GabIA.BLL
{
    public class ParticipanteBLL
    {
        private readonly ParticipanteDAL _participanteDal;

        public ParticipanteBLL(ParticipanteDAL participanteDal)
        {
            _participanteDal = participanteDal;
        }

        public int Inserir(ParticipanteENT entidade)
        {
            return _participanteDal.Inserir(entidade);
        }
    }
}