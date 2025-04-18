using GabIA.DAL;
using GabIA.ENT;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using System.Configuration;

namespace GabIA.BLL
{
    public class ProcessoBLL
    {
        // Classes DAL
        private ProcessoDAL _processoDAL;
        private ProcessoJudicialDAL _processoJudicialDAL;
        private AtoProcessualDAL _atoProcessualDAL;
        private ElementosDAL _elementosDAL;
        private PartesDoProcessoDAL _partesdoProcessoDAL;
        private CausaDePedirDAL _causasDePedirDAL;
        private PedidoDAL _pedidosDAL;

        // Construtor
        public ProcessoBLL(ProcessoDAL processoDAL, ProcessoJudicialDAL processoJudicialDAL, 
                AtoProcessualDAL atoProcessualDAL, ElementosDAL elementosDAL, 
                PartesDoProcessoDAL partesdoProcessoDAL, CausaDePedirDAL causasDePedirDAL, PedidoDAL pedidosDAL)
        {
            _processoDAL = processoDAL;
            _processoJudicialDAL = processoJudicialDAL;
            _atoProcessualDAL = atoProcessualDAL;
            _elementosDAL = elementosDAL;
            _partesdoProcessoDAL = partesdoProcessoDAL;
            _causasDePedirDAL = causasDePedirDAL;
            _pedidosDAL = pedidosDAL;
        }

        // Novo construtor
        public ProcessoBLL(ProcessoDAL processoDAL)
        {
            _processoDAL = processoDAL;
        }

        public ProcessoCompletoENT ObterProcessoCompletoPorId(int idProcessoJudicial)
        {
            try
            {
                ProcessoCompletoENT processoCompleto = _processoDAL.GetProcessoCompletoById(idProcessoJudicial);
                if (processoCompleto == null) throw new InvalidOperationException("Processo não encontrado.");

                processoCompleto.AtosProcessuais = _atoProcessualDAL.ObterAtosProcessuais(idProcessoJudicial);
                processoCompleto.ProcessoJudicial.Elementos = _elementosDAL.GetElementosByProcessoId(idProcessoJudicial).ToList();

                foreach (var elemento in processoCompleto.ProcessoJudicial.Elementos)
                {
                    elemento.PartesDoProcesso = _partesdoProcessoDAL.ObterPartesdoProcesso(elemento.IdElemento).ToList();
                    elemento.CausasDePedir = _causasDePedirDAL.ObterCausasDePedirPorIdElemento(elemento.IdElemento);
                    elemento.Pedidos = _pedidosDAL.ObterPedidosPorIdElemento(elemento.IdElemento);
                }


                return processoCompleto;
            }
            catch (Exception ex)
            {
                // Tratamento de exceções
                throw;
            }
        }

        public ProcessoCompletoENT ObterProcessoCompletoPorNumero(string numeroProcesso)
        {
            try
            {
                // Obtendo idProcessoJudicial a partir do numeroProcesso
                int idProcessoJudicial = _processoDAL.GetPJByProcessNumber(numeroProcesso);

                // Verificando se o processo foi encontrado
                if (idProcessoJudicial == 0)
                    throw new InvalidOperationException("Processo não encontrado.");

                // Utilizando o método existente para obter o ProcessoCompleto por id
                ProcessoCompletoENT processoCompleto = ObterProcessoCompletoPorId(idProcessoJudicial);

                // Atualizando o número do processo e o idPJ no objeto processoCompleto
                processoCompleto.NumeroProcesso = numeroProcesso;
                processoCompleto.IdPJ = _processoDAL.GetPJByProcessNumber(numeroProcesso);

                return processoCompleto;
            }
            catch (Exception ex)
            {
                // Tratamento de exceções
                throw;
            }
        }


        public async Task<IEnumerable<ProcessoENT>> ObterTodosAsync()
        {
            return await _processoDAL.ObterTodosAsync();
        }

        // Outros métodos e lógica de negócios relacionados à entidade Processo
        public List<ProcessoENT> ListarProcessos()
        {
            return _processoDAL.GetProcessos();
        }
        public void AddProcessos(List<ProcessoENT> processos)
        {
            foreach (var processo in processos)
            {
                _processoDAL.AddProcesso(processo);
            }
        }
        public void AddOrUpdateProcessosDeCSV(List<ProcessoCSV> processos)
        {
            foreach (var processo in processos)
            {
                _processoDAL.InserirOuAtualizarDeCSV(processo);
            }
        }
        public void AddProcesso(ProcessoENT processo)
        {
            _processoDAL.AddProcesso(processo);
        }
        public List<ProcessoENT> GetAllProcessos()
        {
            return _processoDAL.GetProcessos();
        }
        public int GetIdProcess()
        {
            return _processoDAL.GetIdProcessFromDatabase();
        }
        public int GetIdProcess(string numeroProcesso)
        {
            return _processoDAL.GetProcessoByNumber_returnID(numeroProcesso);
        }
    }


    public class CsvProcessor
    {
        public List<ProcessoENT> ReadCsvFile(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // Defina o delimitador usado no arquivo CSV
                HasHeaderRecord = true // Indique se o arquivo CSV possui um cabeçalho
            };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var processos = csv.GetRecords<ProcessoENT>().ToList();
                return processos;
            }
        }

    }

}
