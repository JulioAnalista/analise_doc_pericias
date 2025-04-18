using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GabIA.ENT;
using System.Configuration;


namespace GabIA.DAL
{
    public class ProcessoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ProcessoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public int Inserir(ProcessoENT entidade)
        {
            var sql = $@"INSERT INTO processo (id_processo, numero_processo, tipo, Ultimo_movimento, data_distribuicao, classe, classePublicidade)
                VALUES (@id_processo, @numero_processo, @tipo, @Ultimo_movimento, @data_distribuicao, @classe, @classePublicidade);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public ProcessoCompletoENT GetProcessoCompletoByNumeroProcesso(string numeroProcesso)
        {

            var sql = @"
                    SELECT 
                        p.*,
                        pj.id_processo_judicial as idPJ, 
                        a.*, 
                        t.*
                    FROM processo AS p
                    JOIN processo_judicial AS pj ON p.id_processo = pj.idProcesso
                    JOIN atoProcessual AS a ON pj.id_processo_judicial = a.idProcesso
                    JOIN tipo_Ato_Processual AS t ON a.Tipo = t.Id_Tipo_Ato_processual
                    WHERE p.numero_processo = @NumeroProcesso";

            var processoCompleto = _dbConnection.Query<ProcessoCompletoENT, AtoProcessualENT, TipoAtoProcessualENT, ProcessoCompletoENT>(
                sql,
                (processo, atoProcessual, tipo_Ato_Processual) =>
                {
                    atoProcessual.TipoAtoProcessual = tipo_Ato_Processual;
                    processo.AtosProcessuais.Add(atoProcessual);
                    return processo;
                },
                new { NumeroProcesso = numeroProcesso },
                splitOn: "idPJ,Id_Tipo_Ato_processual"
            ).FirstOrDefault();

            return processoCompleto;
        }

        public int InserirOuAtualizarProcessoFromNeoGab(ProcessoCompletoENT processoCompleto)
        {
            // Verifica se a conexão está fechada antes de abri-la
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
            string np = processoCompleto.NumeroProcesso.ToString();

            var sqlExistente = "SELECT id_processo FROM Processo WHERE numero_processo = @NumeroProcesso";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { NumeroProcesso = np }).FirstOrDefault();

            if (idExistente.HasValue)
            {
                DateTime dateTime = processoCompleto.DataDaAbertura;
                string classe = processoCompleto.Classe;
                var sqlAtualizar = "UPDATE Processo SET Ultimo_movimento = @UltimoMovimento, ClasseTxt = @ClasseTxt WHERE Id_processo = @Id_processo";
                _dbConnection.Execute(sqlAtualizar, new { UltimoMovimento = dateTime, ClasseTxt = classe, Id_processo = idExistente.Value });
            }
            else
            {
                string nrPro = processoCompleto.NumeroProcesso;
                DateTime datetm = processoCompleto.DataDaAbertura;
                string classe = processoCompleto.Classe;


                var sqlInserir = "INSERT INTO Processo (Numero_Processo, Ultimo_Movimento, ClasseTxt) VALUES (@NumeroProcesso, @UltimoMovimento, @ClasseTxt); SELECT LAST_INSERT_ID();";
                idExistente = _dbConnection.Query<int>(sqlInserir, new { NumeroProcesso = nrPro, UltimoMovimento = datetm, ClasseTxt = classe}).Single();
            }

            // Fechar a conexão após o uso
            _dbConnection.Close();

            return idExistente.GetValueOrDefault();

        }

        public ProcessoCompletoENT GetProcessoCompletoById(int idPJ)
        {

            var sql = @"
                SELECT * FROM processo AS p
                JOIN ato_processual AS a ON p.idPJ = a.idPJ
                WHERE p.idPJ = @Id";

            var processoCompleto = _dbConnection.Query<ProcessoCompletoENT, AtoProcessualENT, ProcessoCompletoENT>(
                sql,
                (processo, atoProcessual) =>
                {
                    processo.AtosProcessuais.Add(atoProcessual);
                    return processo;
                },
                new { Id = idPJ },
                splitOn: "idPJ"
            ).FirstOrDefault();

            return processoCompleto;
        }


        public void InserirOuAtualizarDeCSV(ProcessoCSV entidade)
        {
            // Checar se a conexão está fechada
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
            // Transação para garantir que todas as instruções SQL sejam executadas juntas
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Verifica se o processo já existe no banco de dados pelo campo numeroProcesso
                var sqlExistente = @"SELECT COUNT(*) FROM processo WHERE numero_processo = @Processo";
                bool processoExistente = _dbConnection.ExecuteScalar<int>(sqlExistente, new { Processo = entidade.Processo }, transaction) > 0;

                int idProcesso;
                if (processoExistente)
                {
                    // Atualiza o processo existente
                    var sqlAtualizaProcesso = @"UPDATE processo SET Ultimo_movimento = @DataDaAbertura 
                                        WHERE numero_Processo = @Processo";
                    _dbConnection.Execute(sqlAtualizaProcesso, entidade, transaction);

                    // Obtém o idProcesso do processo existente
                    idProcesso = _dbConnection.Query<int>("SELECT id_processo FROM processo WHERE numero_Processo = @Processo", new { Processo = entidade.Processo }, transaction).Single();
                }
                else
                {
                    // Insere um novo processo
                    var sqlInsereProcesso = @"INSERT INTO processo (numero_processo, Ultimo_movimento)
                              VALUES (@Processo, @DataDaAbertura);
                              SELECT LAST_INSERT_ID()";
                    idProcesso = _dbConnection.Query<int>(sqlInsereProcesso, entidade, transaction).Single();
                }

                // Verifica se o processo_judicial já existe
                var sqlExistenteProcessoJudicial = @"SELECT id_processo_judicial FROM processo_judicial WHERE idprocesso = @IdProcesso";
                var idProcessoJudicialList = _dbConnection.Query<int>(sqlExistenteProcessoJudicial, new { IdProcesso = idProcesso }, transaction).ToList();

                int idProcessoJudicial;

                if (idProcessoJudicialList.Count > 0)
                {
                    // O processo_judicial já existe, pegue o ID existente
                    idProcessoJudicial = idProcessoJudicialList.First();
                }
                else
                {
                    // O processo_judicial não existe, então vamos inserir e pegar o ID
                    var sqlInsereProcessoJudicial = @"INSERT INTO processo_judicial (idprocesso)
                                      VALUES (@IdProcesso);
                                      SELECT LAST_INSERT_ID()";
                    idProcessoJudicial = _dbConnection.Query<int>(sqlInsereProcessoJudicial, new { IdProcesso = idProcesso }, transaction).Single();
                }

                // Verifica se o ID já existe na tabela atoProcessual
                var sqlExistenteAtoProcessual = @"SELECT COUNT(*) FROM atoProcessual WHERE idMovimento = @ID";
                bool atoProcessualExistente = _dbConnection.ExecuteScalar<int>(sqlExistenteAtoProcessual, new { ID = entidade.ID }, transaction) > 0;

                if (!atoProcessualExistente)
                {
                    var sqlConsultaTipoAto = @"SELECT id_Tipo_Ato_processual FROM tipo_ato_Processual WHERE tipo = 'Abertura de Vista'";
                    var idTipoAtoList = _dbConnection.Query<int>(sqlConsultaTipoAto, transaction: transaction).ToList();

                    int idTipoAto;

                    if (idTipoAtoList.Count > 0)
                    {
                        // O tipo já existe, pegue o ID existente
                        idTipoAto = idTipoAtoList.First();
                    }
                    else
                    {
                        // O tipo não existe, então vamos inserir e pegar o ID
                        var sqlInsereTipoAto = @"INSERT INTO tipo_ato_Processual (tipo)
                                 VALUES ('Abertura de Vista');
                                 SELECT LAST_INSERT_ID()";
                        idTipoAto = _dbConnection.Query<int>(sqlInsereTipoAto, transaction: transaction).Single();
                    }

                    // Agora insere o novo atoProcessual com o ID do tipo e ID do processo_judicial
                    var sqlInsereAtoProcessual = @"INSERT INTO atoProcessual (IdProcesso, DataInclusao, Resumo, Tipo, idMovimento)
                               VALUES (@IdProcessoJudicial, NOW(), 'Vista', @IdTipoAto, @IdMovimento)";
                    _dbConnection.Execute(sqlInsereAtoProcessual, new { IdProcessoJudicial = idProcessoJudicial, IdTipoAto = idTipoAto, IdMovimento = entidade.ID }, transaction);
                }


                var poloAtivo = entidade.PoloAtivo.Replace("Polo ativo: ", "").Trim();

                // Verifica se a pessoa já existe no banco de dados pelo nome
                var sqlPessoaExistente = @"SELECT COUNT(*) FROM pessoa WHERE nome = @Nome";
                bool pessoaExistente = _dbConnection.ExecuteScalar<int>(sqlPessoaExistente, new { Nome = poloAtivo }, transaction) > 0;

                int idPessoa;
                if (pessoaExistente)
                {
                    // Obtém o idPessoa da pessoa existente
                    idPessoa = _dbConnection.Query<int>("SELECT id_pessoa FROM pessoa WHERE nome = @Nome", new { Nome = poloAtivo }, transaction).Single();
                }
                else
                {
                    // Insere uma nova pessoa
                    var sqlInserePessoa = @"INSERT INTO pessoa (nome)
                            VALUES (@Nome);
                            SELECT LAST_INSERT_ID()";
                    idPessoa = _dbConnection.Query<int>(sqlInserePessoa, new { Nome = poloAtivo }, transaction).Single();
                }

                var poloPassivo = entidade.PoloPassivo.Replace("Polo passivo: ", "").Trim();

                // Verifica se a pessoa do polo passivo já existe na tabela Pessoa
                var sqlExistentePessoaPassivo = @"SELECT COUNT(*) FROM pessoa WHERE nome = @PoloPassivo";
                bool pessoaPassivoExistente = _dbConnection.ExecuteScalar<int>(sqlExistentePessoaPassivo, new { PoloPassivo = poloPassivo }, transaction) > 0;

                if (!pessoaPassivoExistente)
                {
                    // Insere um novo registro na tabela Pessoa para a pessoa do polo passivo
                    var sqlInserePessoaPassivo = @"INSERT INTO pessoa (nome)
                                   VALUES (@PoloPassivo)";
                    _dbConnection.Execute(sqlInserePessoaPassivo, new { PoloPassivo = poloPassivo }, transaction);
                }

                // Verifica se membro já existe no banco de dados pelo nome
                var sqlMembroExistente = @"SELECT COUNT(*) FROM pessoa WHERE nome = @Nome";
                bool membroExistente = _dbConnection.ExecuteScalar<int>(sqlPessoaExistente, new { Nome = entidade.MembroResponsavel }, transaction) > 0;

                int idMembro;
                if (membroExistente)
                {
                    // Obtém o idPessoa da pessoa existente
                    idMembro = _dbConnection.Query<int>("SELECT id_pessoa FROM pessoa WHERE nome = @Nome", new { Nome = entidade.MembroResponsavel}, transaction).Single();
                }
                else
                {
                    // Insere uma nova pessoa
                    var sqlInserePessoa = @"INSERT INTO pessoa (nome)
                            VALUES (@Nome);
                            SELECT LAST_INSERT_ID()";
                    idMembro = _dbConnection.Query<int>(sqlInserePessoa, new { Nome = entidade.MembroResponsavel }, transaction).Single();
                }

                // Insere ou atualiza o membro responsável
                var sqlMembroResponsavelExistente = @"SELECT COUNT(*) FROM membrosmp WHERE idPessoa = @IdPessoa AND idProcesso = @IdProcesso";
                bool membroResponsavelExistente = _dbConnection.ExecuteScalar<int>(sqlMembroResponsavelExistente, new { IdPessoa = idMembro, IdProcesso = idProcesso }, transaction) > 0;

                if (membroResponsavelExistente)
                {
                    // Atualiza o membro responsável existente se necessário
                    var sqlAtualizaMembroResponsavel = @"UPDATE membrosmp SET idPessoa = @IdPessoa, idProcesso = @IdProcesso 
                                                  WHERE idPessoa = @IdPessoa AND idProcesso = @IdProcesso";
                    _dbConnection.Execute(sqlAtualizaMembroResponsavel, new { IdPessoa = idMembro, IdProcesso = idProcesso }, transaction);
                }
                else
                {
                        // Insere um novo membro responsável
                        var sqlInsereMembroResponsavel = @"INSERT INTO membrosmp (idPessoa, idProcesso)
                                                    VALUES (@IdPessoa, @IdProcesso)";
                        _dbConnection.Execute(sqlInsereMembroResponsavel, new { IdPessoa = idMembro, IdProcesso = idProcesso }, transaction);
                    }

                transaction.Commit();
            }
        }

        public void AddProcesso(ProcessoENT processo)
        {
            var sql = @"INSERT INTO processo (id_processo, numero_processo, Tipo, Ultimo_movimento, data_distribuicao, classe, assunto, vara)
                    VALUES (@id_processo, @numero_processo, @Tipo, @Ultimo_movimento, @data_distribuicao, @classe, @assunto, @vara)";

            _dbConnection.Execute(sql, processo);
        }

        public List<ProcessoENT> GetProcessos()
        {
            const string sql = "SELECT * FROM processo";
            return _dbConnection.Query<ProcessoENT>(sql).ToList();
        }

        public async Task<IEnumerable<ProcessoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM processo";
            return await _dbConnection.QueryAsync<ProcessoENT>(sql);
        }
        public void UpdateProcesso(ProcessoENT processo)
        {
            // Implemente a lógica para atualizar um processo existente no banco de dados
        }

        public ProcessoENT GetProcessoById(int id)
        {
            ProcessoENT processo = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM Processos WHERE id_processo = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            processo = new ProcessoENT
                            {
                                id_processo = reader.GetInt32("id_processo"),
                                numero_processo = reader.IsDBNull(reader.GetOrdinal("numero_processo")) ? null : reader.GetString("numero_processo"),
                                Tipo = reader.GetByte("Tipo"),
                                Ultimo_movimento = reader.GetDateTime("Ultimo_movimento"),
                                data_distribuicao = reader.GetDateTime("data_distribuicao"),
                                classe = reader.IsDBNull(reader.GetOrdinal("classe")) ? null : reader.GetString("classe"),
                                // assunto = reader.IsDBNull(reader.GetOrdinal("assunto")) ? null : reader.GetString("assunto"),
                                // vara = reader.IsDBNull(reader.GetOrdinal("vara")) ? null : reader.GetString("vara"),
                                classepublicidade = reader.IsDBNull(reader.GetOrdinal("classepublicidade")) ? null : reader.GetString("classepublicidade"),
                            };
                        }
                    }
                }
            }
            return processo;
        }

        public int GetIdProcessFromDatabase()
        {
            int processId = 0;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT MAX(id_processo) as max_id FROM processo", connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            processId = reader.IsDBNull(reader.GetOrdinal("max_id")) ? 0 : reader.GetInt32("max_id");
                        }
                    }
                }
            }
            return processId;
        }

        // Método Create - Insere um novo processo
        // Insere um novo processo a partir de um ProcessoENT
        public void InserirProcesso(ProcessoENT processo)
        {
            string nr = processo.numero_processo;
            byte tp = processo.Tipo;
            string dt = processo.Ultimo_movimento.ToString("yyyy-MM-dd HH:mm:ss");
            string dt_dist = processo.data_distribuicao.ToString("yyyy-MM-dd HH:mm:ss");
            string classe = processo.classe;
            if (classe == null) classe = "0";
            string classepublicidade = processo.classepublicidade;
            if (classepublicidade == null) classepublicidade = "SIGILOSO";

            var sql = $"INSERT INTO processo (numero_processo, Tipo, Ultimo_movimento, data_distribuicao, classe, classepublicidade) VALUES ('{nr}', {tp}, '{dt}', '{dt_dist}', '{classe}', '{classepublicidade}')";

            _dbConnection.Execute(sql);
        }


        // Insere um novo processo a partir de um ProcessoCSV
        public void InserirProcessoCSV(ProcessoCSV processoCSV)
        {
            var processoENT = new ProcessoENT
            {
                numero_processo = processoCSV.Processo,
                Tipo = byte.Parse(processoCSV.Tipo), // assume that Tipo in ProcessoCSV can be parsed to byte
                Ultimo_movimento = DateTime.Now, // set to the current time or wherever you get this data from
                data_distribuicao = processoCSV.DataDaAbertura,
                classe = processoCSV.PoloAtivo, // or whatever field matches
                classepublicidade = processoCSV.PoloPassivo // or whatever field matches
            };

            InserirProcesso(processoENT);
        }


        // Método Read - Retorna um processo com base em seu ID
        public ProcessoENT GetProcessoByNumber(string processNumber)
        {
            var sql = "SELECT * FROM processo WHERE numero_processo = @Id";
            return _dbConnection.QuerySingleOrDefault<ProcessoENT>(sql, new { Id = processNumber });
        }





        public int GetPJByProcessNumber(string processNumber)
        {
            // Primeira consulta para obter id_processo da tabela processo
            var sqlProcesso = "SELECT id_processo FROM processo WHERE numero_processo = @NumeroProcesso";
            var idProcesso = _dbConnection.QueryFirstOrDefault<int>(sqlProcesso, new { NumeroProcesso = processNumber });

            // Verificar se um id_processo foi encontrado
            if (idProcesso > 0)
            {
                // Segunda consulta para obter id_processo_judicial da tabela processo_judicial
                var sqlProcessoJudicial = "SELECT id_processo_judicial FROM processo_judicial WHERE idprocesso = @IdProcesso";
                int nrTemp = 0;
                nrTemp = _dbConnection.QueryFirstOrDefault<int>(sqlProcessoJudicial, new { IdProcesso = idProcesso });
                return _dbConnection.QueryFirstOrDefault<int>(sqlProcessoJudicial, new { IdProcesso = idProcesso });
            }

            // Retornar 0 se nenhum id_processo_judicial foi encontrado
            return 0;


        }


        public int GetProcessoByNumber_returnID(string processNumber)
        {
            var sql = "SELECT * FROM processo WHERE numero_processo = @Idn";
            return _dbConnection.ExecuteScalar<int>(sql, new { Idn = processNumber});
        }


        // Método Update - Atualiza um processo existente
        public void AtualizarProcesso(ProcessoENT processo)
        {
            var sql = "UPDATE processo SET numero_processo = @numero_processo, Tipo = @Tipo, Ultimo_movimento = @Ultimo_movimento, data_distribuicao = @Data_Distribuicao, classe = @Classe, classepublicidade = @ClassePublicidade WHERE id_processo = @Id_Processo";
            _dbConnection.Execute(sql, processo);
        }

        // Método Delete - Exclui um processo
        public void DeletarProcesso(int idProcesso)
        {
            var sql = "DELETE FROM processo WHERE id_processo = @Id";
            _dbConnection.Execute(sql, new { Id = idProcesso });
        }
    }
}

