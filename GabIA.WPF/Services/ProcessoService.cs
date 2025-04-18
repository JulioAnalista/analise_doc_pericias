using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.BLL;
using GabIA.DAL;

namespace GabIA.WPF.Services
{
    public class ProcessoService
    {
        private readonly ProcessoBLL _processoBLL;

        public ProcessoService()
        {
            ProcessoDAL processoDAL = new ProcessoDAL();
            _processoBLL = new ProcessoBLL(processoDAL);
        }

        public int GetIdProcess(string processo)
        {
            return _processoBLL.GetIdProcess(processo);
        }

        // Se necessário, adicione outros métodos que utilizem a instância de ProcessoBLL
    }
}