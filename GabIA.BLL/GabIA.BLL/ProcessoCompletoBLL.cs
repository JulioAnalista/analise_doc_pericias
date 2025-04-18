using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GabIA.ENT;
using GabIA.DAL;
using Org.BouncyCastle.Bcpg;


namespace GabIA.BLL
{
    public class ProcessoCompletoBLL
    {
        // Dependências para as classes DAL
        private ProcessoDAL _processoDAL;
        private ProcessoJudicialDAL _processoJudicialDAL;
        private ElementosDAL _elementosDAL;
        private PartesDoProcessoDAL _partesdoProcessoDAL;
        private CausaDePedirDAL _causasDePedirDAL;
        private PedidoDAL _pedidosDAL;

        public ProcessoCompletoBLL()
        {
            _processoDAL = new ProcessoDAL();
        }

        public ProcessoCompletoBLL(string conectionString)
        {
            _processoDAL = new ProcessoDAL();
        }


        public ProcessoCompletoENT GetProcessoCompletoById(int id)
        {
            return _processoDAL.GetProcessoCompletoById(id);
        }

        public ProcessoCompletoENT GetProcessoCompletoByNumeroProcesso(string numeroProcesso)
        {
            return _processoDAL.GetProcessoCompletoByNumeroProcesso(numeroProcesso);
        }

        public bool PersisteDataBase(List<ProcessoCompletoENT> allData)
        {
            int i = 0;
            foreach (var processoCompleto in allData)
            {
                try
                {
                    // Inserir ou atualizar na tabela processo
                    processoCompleto.IdP = _processoDAL.InserirOuAtualizarProcessoFromNeoGab(processoCompleto);

                    _processoJudicialDAL = new ProcessoJudicialDAL();
                    // Inserir ou atualizar na tabela processoJudicial
                    processoCompleto.IdPJ = _processoJudicialDAL.InserirOuAtualizarProcessoJudicialFromNeoGab(processoCompleto);

                    _elementosDAL = new ElementosDAL();
                    _partesdoProcessoDAL = new PartesDoProcessoDAL();
                    _causasDePedirDAL = new CausaDePedirDAL();
                    _pedidosDAL = new PedidoDAL();

                    int idElementos = _elementosDAL.InserirOuAtualizarElementoFromNeoGab( processoCompleto, _partesdoProcessoDAL, _causasDePedirDAL, _pedidosDAL);
                    // Inserir ou atualizar as outras tabelas
                    // Assumindo que você tem métodos semelhantes para Elementos, PartesdoProcesso, CausasDePedir, Pedidos, AtosProcessuais
                    // Exemplo: _elementosDAL.InserirOuAtualizarElementosFromNeoGab(processoCompleto);
                    // Repita para as outras tabelas

                }
                catch (Exception ex)
                {
                    // Tratamento de exceções
                    // Você pode decidir como lidar com exceções aqui. Por exemplo, logar o erro e continuar ou parar o processo.
                    // Exemplo: Logar o erro e retornar false
                    Console.WriteLine($"Erro ao persistir dados do processo: {ex.Message}");
                    return false;
                }
            }
            return true;
        }
    }
}
