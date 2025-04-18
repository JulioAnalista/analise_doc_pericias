using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using GabIA.ENT;
using GabIA.DAL;
using System.Diagnostics;
using System.Data;
using System.Windows;

namespace GabIA.BLL
{
    public class OperacoesBLL
    {
        private readonly ProcessoDAL _processoDAL;
        private readonly ProcessoJudicialDAL _processoJudicialDAL;
        private readonly TipoAtoProcessualDAL _tipoAtoProc;
        private readonly AtoProcessualDAL _atoProc;
        private readonly PosicoesDAL _posicoes;
        private readonly ElementosDAL _elementos;
        private readonly PessoaDAL _pessoa;
        private readonly Membros_MP_DAL _membro;
        private readonly PartesDoProcessoDAL _partes;
        private readonly OrgaoMinisterialDAL _orgaoMP;

        // Adicione a seguinte linha
        private readonly AtoProcessualDAL _atoProcessualDAL;
        private readonly PartesDoProcessoDAL _partesdoProcessoDAL;

        public OperacoesBLL(
            ProcessoDAL processoDAL,
            ProcessoJudicialDAL processoJudicialDAL,
            TipoAtoProcessualDAL tipoAtoProc,
            AtoProcessualDAL atoProc,
            PosicoesDAL posicoes,
            ElementosDAL elementos,
            PessoaDAL pessoa,
            Membros_MP_DAL membro,
            PartesDoProcessoDAL partes,
            OrgaoMinisterialDAL orgaoMP,
            AtoProcessualDAL atoProcessualDAL,
            PartesDoProcessoDAL partesdoProcessoDAL,
            string connectionString
        )
        {
            _processoDAL = processoDAL;
            _processoJudicialDAL = processoJudicialDAL;
            _tipoAtoProc = tipoAtoProc;
            _atoProc = atoProc;
            _posicoes = posicoes;
            _elementos = elementos;
            _pessoa = pessoa;
            _membro = membro;
            _partes = partes;
            _orgaoMP = orgaoMP;
            _atoProcessualDAL = atoProcessualDAL;
            _partesdoProcessoDAL = partesdoProcessoDAL;
        }

        public OperacoesBLL()
        {
            _tipoAtoProc = new TipoAtoProcessualDAL();
            _processoDAL = new ProcessoDAL();
            _atoProcessualDAL = new AtoProcessualDAL();
            _partesdoProcessoDAL = new PartesDoProcessoDAL();
            _elementos = new ElementosDAL();
            _processoJudicialDAL = new ProcessoJudicialDAL();
        }

        public int idProcessoJudicial(string nProcesso)
        {
            int nProc = _processoDAL.GetPJByProcessNumber(nProcesso);
            int nProcJ = _processoJudicialDAL.GetIdProcessoJudicialByIdProcesso(nProc);
            if (nProcJ ==null)
            {
                nProcJ = 0;
            }
            return nProcJ;
        }

        public List<TipoAtoProcessualENT> GetTiposAtoProcessual()
        {
            return _tipoAtoProc.Gettipo_ato_processualENTs();
        }


        public AtoProcessualENT BuscarAtoPorIdMovimento(List<AtoProcessualENT> atosDoProcesso, string idMovimento)
        {
            return atosDoProcesso.FirstOrDefault(ato => ato.IdMovimento == idMovimento);
        }
        ////public int ObterProximoIdMovimentoSeTipo83(int idProcessoJudicial, string idMovimentoArgumento)
        ////{
        ////    var atosProcessuais = _atoProcessualDAL.ObterAtosProcessuais(idProcessoJudicial)
        ////                                           .OrderBy(ato => ato.IdMovimento)
        ////                                           .ToList();

        ////    if (atosProcessuais != null && atosProcessuais.Any())
        ////    {
        ////        short? parsedIdMovimentoArgumento = null;
        ////        if (short.TryParse(idMovimentoArgumento, out short tempId))
        ////        {
        ////            parsedIdMovimentoArgumento = tempId;
        ////        }

        ////        var atoProcessual = atosProcessuais.FirstOrDefault(ato => ato.IdMovimento == parsedIdMovimentoArgumento);

        ////        if (atoProcessual != null)
        ////        {
        ////            if (atoProcessual.Tipo is short tipoAsShort && tipoAsShort == 1)
        ////            {
        ////                // Se o tipo é short e igual a 1
        ////                int index = atosProcessuais.IndexOf(atoProcessual);
        ////                if (index >= 0 && index < atosProcessuais.Count - 1)
        ////                {
        ////                    if (int.TryParse(atosProcessuais[index + 1].IdMovimento, out int nextIdMovimento))
        ////                    {
        ////                        return nextIdMovimento;
        ////                    }
        ////                }
        ////            }
        ////            else if (atoProcessual.Tipo is short ? tipoAsNullableShort && tipoAsNullableShort.HasValue && tipoAsNullableShort.Value == 83)
        ////            {
        ////                // Se o tipo é short nullable e igual a 83
        ////                // Resto do código para quando tipoAsNullableShort é 83
        ////            }
        ////        }
        ////    }

        ////    return 0;
        ////}


        public List<AtoProcessualENT> BuscarTodosOsAtosDoProcesso(string numeroProcesso)
        {
            // Obtém o ID do processo judicial
            int idProcessoJ = _processoDAL.GetPJByProcessNumber(numeroProcesso);
            int idProcessoJudicial = _processoJudicialDAL.GetIdProcessoJudicialByIdProcesso(idProcessoJ);

            // Obtém todos os atos processuais relacionados, incluindo a descrição do tipo de ato processual
            List<AtoProcessualENT> atosProcessuais = _atoProcessualDAL.ObterAtosProcessuaisPorNumeroProcesso(idProcessoJ);

            return atosProcessuais;
        }


        public List<TipoAtoProcessualENT> BuscarTodosOsTiposDeAtosProcessuais()
        {
            // Em seguida, usamos esse ID para obter todos os atos processuais relacionados
            List<TipoAtoProcessualENT> tiposAtosProcessuais = _tipoAtoProc.Gettipo_ato_processualENTs();

            return tiposAtosProcessuais;
        }



        
        public void LoadDataFromCSV(string csvFileName)
        {

            ProcessoJudicialDAL _processo_judicialDAL;
            TipoAtoProcessualDAL _tipoAtoProc;
            AtoProcessualDAL _atoProc;
            PosicoesDAL _posicoes;
            ElementosDAL _elementos;
            PessoaDAL _pessoa;
            Membros_MP_DAL _membro;
            PartesDoProcessoDAL _partes;
            OrgaoMinisterialDAL _orgaoMP;

            _processo_judicialDAL = new ProcessoJudicialDAL();
            _tipoAtoProc = new TipoAtoProcessualDAL();
            _elementos = new ElementosDAL();
            _pessoa = new PessoaDAL();
            _membro = new Membros_MP_DAL();
            _partes = new PartesDoProcessoDAL();
            _orgaoMP = new OrgaoMinisterialDAL();
            _atoProc = new AtoProcessualDAL();
            _posicoes = new PosicoesDAL();



            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                IgnoreBlankLines = true,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim().Trim('|'),
                HeaderValidated = null // Ignorar a validação do cabeçalho
            };

            using (var reader = new StreamReader(csvFileName))
            using (var csv = new CsvReader(reader, csvConfiguration))
            {
                var processos = new List<ProcessoCSV>();

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var idAndType = ExtractIdAndType(csv.GetField<string>("ID"));
                    var dataDaAbertura = DateTime.Parse(csv.GetField<string>("DataDaAbertura").Replace(" às ", " "));

                    var processo = new ProcessoCSV
                    {
                        ID = idAndType.Item1,
                        Tipo = idAndType.Item2,
                        Processo = csv.GetField<string>("Processo"),
                        DataDaAbertura = dataDaAbertura,
                        PoloAtivo = csv.GetField<string>("PoloAtivo"),
                        PoloPassivo = csv.GetField<string>("PoloPassivo"),
                        MembroResponsavel = csv.GetField<string>("MembroResponsavel"),
                        Promotoria = csv.GetField<string>("Promotoria")
                    };

                    processos.Add(processo);
                }
                // aqui deu o erro 
                foreach (var processo in processos)
                {
                    // Primeiro, precisamos transformar a entidade ProcessoCSV em ProcessoENT
                    var processoEntidade = new ProcessoENT
                    {
                        numero_processo = processo.Processo,
                        Ultimo_movimento = processo.DataDaAbertura
                        // ... preencha os outros campos aqui
                    };

                    // Verifica se o processo já existe
                    var processoExistente = _processoDAL.GetProcessoByNumber_returnID(processoEntidade.numero_processo);

                    if (processoExistente > 0)
                    {
                        // Se existir, atualizamos o processo
                        _processoDAL.AtualizarProcesso(processoEntidade);
                    }
                    else
                    {
                        // Se não existir, inserimos um novo processo
                        Debug.WriteLine(processoEntidade.numero_processo);
                        _processoDAL.InserirProcesso(processoEntidade);
                    }

                    // Após a criação ou atualização, recuperamos o id_processo
                    int idProcesso = _processoDAL.GetPJByProcessNumber(processoEntidade.numero_processo);

                    processoExistente = _processoDAL.GetProcessoByNumber_returnID(processoEntidade.numero_processo);

                    // Agora podemos usar o idProcesso para as operações de inserção/edição em 'processo_judicial' e 'atoProcessual'
                    var processoJudicial = new ProcessoJudicialENT
                    {
                        Id_processo_judicial = processoExistente,
                    };

                    int idProcessoJudicial = _processo_judicialDAL.GetIdProcessoJudicialByIdProcesso(processoExistente);

                    // Chame o método para inserir/editar o registro em 'processo_judicial'
                    if (idProcessoJudicial == 0)
                    {
                        _processo_judicialDAL.InserirProcessoJudicial(processoJudicial);
                    }
                    else
                    {
                        _processo_judicialDAL.AtualizarProcessoJudicial(processoJudicial);
                    }

                    idProcessoJudicial = _processo_judicialDAL.GetIdProcessoJudicialByIdProcesso(processoExistente);

                    var tipoAtoProcessual = new TipoAtoProcessualENT
                    {
                        Tipo = processo.Tipo
                    };

                    int idTipoAto = _tipoAtoProc.GetId_ato_processualByTipo(processo.Tipo);


                    if (idTipoAto == 0)
                    {
                        // Se não existir, incluímos o tipo de ato processual
                        _tipoAtoProc.Addtipo_ato_processualENT(tipoAtoProcessual);
                    }

                    idTipoAto = _tipoAtoProc.GetId_ato_processualByTipo(processo.Tipo);

                    if (idTipoAto > 0)
                    {
                        var atoProcessualENT = new AtoProcessualENT
                        {
                            IdProcesso = idProcessoJudicial,
                            Tipo = idTipoAto.ToString(),
                            DataInclusao = DateTime.Now,
                            Resumo = processo.Tipo,
                            IdMovimento = processo.ID.ToString()
                        };
                        _atoProc.AddatoprocessualENT(atoProcessualENT);
                    }
                    else
                    {
                        Debug.WriteLine("erro ao recuperar id");
                    }

                    var elementos = new ElementosENT
                    {
                        IdProcessoJudicial = idProcessoJudicial
                    };

                    try
                    {
                        // Tente obter os elementos do processo aqui
                        IEnumerable<ElementosENT> elementosCol = _elementos.GetElementosByProcessoId(idProcessoJudicial);

                        // Se a obtenção for bem-sucedida, faça o que precisa ser feito com os elementos

                        // ...

                    }
                    catch (Exception ex)
                    {
                        // Se ocorrer uma exceção, exiba uma mensagem de erro
                        Debug.WriteLine("Erro ao obter elementos do processo: " + ex.Message, "Erro");
                    }

                    //Polo ativo
                    var posicaoAtiva = new PosicoesENT
                    {
                        posicao = "ativo"
                    };
                    
                    int idPosicaoA = _posicoes.GetIdPosicao("ativo");

                    if(idPosicaoA == 0)
                    {
                        _posicoes.Inserir(posicaoAtiva);
                    }

                    idPosicaoA = _posicoes.GetIdPosicao("ativo");

                    string[] nomes = SepararStringNome(processo.PoloAtivo);

                    var pessoa = new PessoaENT
                    {
                        Nome = nomes[1]
                    };

                    int idPessoa = _pessoa.GetPessoaId_FromNome(pessoa.Nome);

                    if(idPessoa == 0)
                    {
                        _pessoa.Inserir(pessoa);
                        idPessoa = _pessoa.GetPessoaId_FromNome(pessoa.Nome) ;
                    }

                    var parte = new PartesDoProcessoENT
                    {
                        IdPessoa = idPessoa,
                        IdPosicao = idPosicaoA,
                    };

                    var idPartes = _partes.GetPartesDoProcessoByElementoId(idPessoa);

                    if (idPartes.Count > 0)
                    {
                        //_partes.AddpartesdoprocessoENT(parte);
                        //idPartes = _partes.GetIdPartesDoProcesso(parte);
                        Debug.WriteLine(idPartes.Count);
                    }

                    string poloPassivo = processo.PoloPassivo;

                    if (poloPassivo.Contains("passivo"))
                    {
                        //Polo Passivo
                        var posicaoPassiva = new PosicoesENT
                        {
                            posicao = "passivo"
                        };
                        int idPosicaoB = _posicoes.GetIdPosicao("passivo");

                        if (idPosicaoB == 0)
                        {
                            _posicoes.Inserir(posicaoPassiva);
                        }

                        idPosicaoB = _posicoes.GetIdPosicao("passivo");

                        string[] nomesB = SepararStringNome(processo.PoloPassivo);


                        pessoa = new PessoaENT
                        {
                            Nome = nomesB[1]
                        };

                        idPessoa = _pessoa.GetPessoaId_FromNome(pessoa.Nome);

                        if (idPessoa == 0)
                        {
                            _pessoa.Inserir(pessoa);
                            idPessoa = _pessoa.GetPessoaId_FromNome(pessoa.Nome);
                        }

                        parte = new PartesDoProcessoENT
                        {
                            IdPessoa = idPessoa,
                            IdPosicao = idPosicaoB,
                            //IdElemento = idElementos
                        };

                        //idPartes = _partes.GetIdPartesDoProcesso(parte);

                        //if (idPartes == 0)
                        //{
                        //    _partes.AddpartesdoprocessoENT(parte);
                        //}

                    }

                    //membro responsável
                    var membro = new PosicoesENT
                    {
                        posicao = "Membro Responsável"
                    };
                    int idMembro= _posicoes.GetIdPosicao("Membro Responsável");

                    if (idMembro == 0)
                    {
                        _posicoes.Inserir(membro);
                    }

                    idMembro = _posicoes.GetIdPosicao("Membro Responsável");

                    var _membroMP = new PessoaENT
                    {
                        Nome = processo.MembroResponsavel
                    };

                    idPessoa = _pessoa.GetPessoaId_FromNome(processo.MembroResponsavel);

                    if (idPessoa == 0)
                    {
                        _pessoa.Inserir(_membroMP);
                        idPessoa = _pessoa.GetPessoaId_FromNome(processo.MembroResponsavel);
                    }

                    //promotoria
                    var orgaoMP = new OrgaoMinisterialENT
                    {
                        orgaoministerial = processo.Promotoria
                    };

                    int idOM = _orgaoMP.GetId_OrgaoMinisterial(processo.Promotoria);

                    if (idOM == 0)
                    {
                        _orgaoMP.Inserir(orgaoMP);
                    }
                    idOM = _orgaoMP.GetId_OrgaoMinisterial(processo.Promotoria);

                    idProcessoJudicial = _processo_judicialDAL.GetIdProcessoJudicialByIdProcesso(processoExistente);

                    // Chame o método para editar o registro em 'processo_judicial'
                    if (idProcessoJudicial != 0)
                    {
                        processoJudicial = new ProcessoJudicialENT
                        {
                            Id_processo_judicial = idProcessoJudicial,
                            idorgaoministerial = idOM
                        };

                        _processo_judicialDAL.AtualizarProcessoJudicial(processoJudicial);
                    }
                }
            }
        }

        public IEnumerable<ProcessoCSV> GetAllProcessosCSV()
        {
            ProcessoJudicialDAL _processo_judicialDAL;
            TipoAtoProcessualDAL _tipoAtoProc;
            AtoProcessualDAL _atoProc;
            PosicoesDAL _posicoes;
            ElementosDAL _elementos;
            PessoaDAL _pessoa;
            Membros_MP_DAL _membro;
            PartesDoProcessoDAL _partes;
            OrgaoMinisterialDAL _orgaoMP;
            ProcessoDAL _processoDAL;


            _atoProc = new AtoProcessualDAL();
            _processo_judicialDAL = new ProcessoJudicialDAL();
            _tipoAtoProc = new TipoAtoProcessualDAL();
            _elementos = new ElementosDAL();
            _pessoa = new PessoaDAL();
            _membro = new Membros_MP_DAL();
            _partes = new PartesDoProcessoDAL();
            _orgaoMP = new OrgaoMinisterialDAL();
            _atoProc = new AtoProcessualDAL();
            _posicoes = new PosicoesDAL();
            _processoDAL = new ProcessoDAL();

            var processosCSV = new List<ProcessoCSV>();
            try
            {
                var processos = _processoDAL.GetProcessos();

                foreach (var processoENT in processos)
                {
                    Debug.WriteLine(processoENT.numero_processo);
                    if (processoENT.numero_processo.ToString() == "0700701-16.2022.8.07.0021") 
                    {
                        Debug.WriteLine("Este!");
                    }
                    ProcessoJudicialENT processoJudicial = _processo_judicialDAL.GetProcessoJudicialById(processoENT.id_processo);

                    if (processoJudicial == null)
                    {
                        continue;
                    }

                    var atosProcessuais = _atoProc.ObterAtoProcessualPorNumeroProcesso(processoENT.numero_processo);
                    atosProcessuais = _atoProc.ObterListaAtoProcessualPorProcessoJudicial(processoJudicial.Id_processo_judicial);

                    var elementos = _elementos.GetElementosByIdProcesso(processoJudicial.Id_processo_judicial);

                    string partesAtivas = null;
                    string partesPassivas = null;
                    string membroResponsavel = null;

                    if (elementos != null)
                    {
                        int posicao = _posicoes.GetIdPosicao("Polo Ativo");
                        partesAtivas = _partes.GetNomeParteByIdElementosAndIdPosicao(elementos.IdElemento, 1);
                        posicao = _posicoes.GetIdPosicao("Polo Passivo");
                        partesPassivas = _partes.GetNomeParteByIdElementosAndIdPosicao(elementos.IdElemento, _posicoes.GetIdPosicao("Polo Passivo"));

                        membroResponsavel = _partes.GetNomeParteByIdElementosAndIdPosicao(elementos.IdElemento, _posicoes.GetIdPosicao("Membro Responsável"));

                    }

                    var orgaoMinisterial = _orgaoMP.GetOrgaoMinisterialByIdParte(processoJudicial.idorgaoministerial);

                    var ultimoAtoProcessual = atosProcessuais?.OrderByDescending(ap => ap.DataInclusao).FirstOrDefault();
                    if (ultimoAtoProcessual != null)
                    {
                        processosCSV.Add(new ProcessoCSV
                        {
                            ID = processoJudicial.Id_processo_judicial,
                            Tipo = ultimoAtoProcessual.Tipo,
                            Processo = processoENT.numero_processo,
                            DataDaAbertura = processoENT.Ultimo_movimento,
                            PoloAtivo = partesAtivas,
                            PoloPassivo = partesPassivas,
                            MembroResponsavel = membroResponsavel,
                            Promotoria ="",
                            idPJ = processoJudicial.Id_processo_judicial
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception to your logging framework here.
                // This could be as simple as:
                Debug.WriteLine(ex.Message);
            }

            return processosCSV.OrderByDescending(p => p.DataDaAbertura);
        }

        public (int, string) ExtractIdAndType(string input)
        {
            var splitInput = input.Split(' ');

            int idMovimento = int.Parse(splitInput[0]); // Extrai o idMovimento como um número inteiro.

            var tipo = string.Join(" ", splitInput.Skip(1)).Trim('(', ')'); // Junta todas as partes restantes da string e remove os parênteses.

            return (idMovimento, tipo);
        }

        public static string[] SepararStringNome(string input)
        {
            string[] partes = input.Split(':');
            string polo = partes[0].Trim();
            string nome = "";
            if (partes.Length < 2)
            {
                Debug.WriteLine($"{partes.Length} partes");
                nome = "Parte não cadastrada";
            }
            else
            {
                nome = partes[1].Trim();
            }

            return new string[] { polo, nome };
        }

    }
}
