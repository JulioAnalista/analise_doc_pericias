using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GabIA.ENT;
using System.Configuration;
using System.Diagnostics;

namespace GabIA.DAL
{
    public class ProcessoJudicialDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ProcessoJudicialDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public int InserirOuAtualizarProcessoJudicialFromNeoGab(ProcessoCompletoENT processoCompleto)
        {
            // Aqui, assumimos que processoCompleto.ProcessoJudicial contém os dados necessários
            int processoJudicial = processoCompleto.IdP;

            // Verificar se o registro já existe (assumindo que processoJudicial.IdPJ é a chave)
            var sqlExistente = "SELECT id_processo_Judicial FROM Processo_Judicial WHERE idprocesso = @IdPJ";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { IdPJ = processoJudicial }).FirstOrDefault();

            if (idExistente.HasValue)
            {
                // Atualizar o registro existente
                //var sqlAtualizar = "UPDATE Processo_Judicial SET ... WHERE IdPJ = @IdPJ";
                //_dbConnection.Execute(sqlAtualizar, processoJudicial);
                return idExistente.Value;
            }
            else
            {
                // Inserir um novo registro
                var sqlInserir = "INSERT INTO Processo_Judicial (idprocesso) VALUES (@IdP); SELECT LAST_INSERT_ID();";
                var idNovo = _dbConnection.Query<int>(sqlInserir, new { IdP = processoCompleto.IdP }).Single();
                return idNovo;

            }
        }
        public int Inserir(ProcessoJudicialENT entidade)
        {
            var sql = $@"INSERT INTO processo_judicial (Id_processo_Judicial, tribunal_distr, vara_distr, data_despacho, unidade_jurisdicional, rito, juiz, membro_MP, valor_da_causa, atua_MP, atua_curador_especial, idprocesso)
                VALUES (@Id_processo_Judicial, @tribunal_distr, @vara_distr, @data_despacho, @unidade_jurisdicional, @rito, @juiz, @membro_MP, @valor_da_causa, @atua_MP, @atua_curador_especial, @idprocesso);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddProcesso_JudicialENT(ProcessoJudicialENT entidade)
        {
            var sql = $@"INSERT INTO processo_judicial (Id_processo_Judicial, tribunal_distr, vara_distr, data_despacho, unidade_jurisdicional, rito, juiz, membro_MP, valor_da_causa, atua_MP, atua_curador_especial, idprocesso)
                        VALUES (@Id_processo_Judicial, @tribunal_distr, @vara_distr, @data_despacho, @unidade_jurisdicional, @rito, @juiz, @membro_MP, @valor_da_causa, @atua_MP, @atua_curador_especial, @idprocesso)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ProcessoJudicialENT> GetProcesso_JudicialENTs()
        {
            const string sql = "SELECT * FROM processo_judicial";
            return _dbConnection.Query<ProcessoJudicialENT>(sql).ToList();
        }

        public async Task<IEnumerable<ProcessoJudicialENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM processo_judicial";
            return await _dbConnection.QueryAsync<ProcessoJudicialENT>(sql);
        }

        public void UpdateProcesso_JudicialENT(ProcessoJudicialENT entidade)
        {
            var sql = $@"UPDATE processo_judicial SET Id_processo_Judicial = @Id_processo_Judicial, tribunal_distr = @tribunal_distr, vara_distr = @vara_distr, data_despacho = @data_despacho, unidade_jurisdicional = @unidade_jurisdicional, rito = @rito, juiz = @juiz, membro_MP = @membro_MP, valor_da_causa = @valor_da_causa, atua_MP = @atua_MP, atua_curador_especial = @atua_curador_especial, idprocesso = @idprocesso
                        WHERE Id_processo_Judicial = @Id_processo_Judicial";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteProcesso_JudicialENT(int id)
        {
            var sql = $@"DELETE FROM processo_judicial WHERE Id_processo_Judicial = @id";
            _dbConnection.Execute(sql, new { id });
        }

        // Método Create - Insere um novo processo judicial
        public void InserirProcessoJudicial(ProcessoJudicialENT processoJudicial)
        {
            Debug.WriteLine(processoJudicial.Id_processo_judicial);
            var sql = "INSERT INTO processo_judicial (tribunal_distr, vara_distr, data_despacho, unidade_jurisdicional, rito, juiz, membro_mp, valor_da_causa, atua_mp, atua_curador_especial, idProcesso, idorgaoMinisterial) VALUES (@Tribunal_Distr, @Vara_Distr, @Data_Despacho, @Unidade_Jurisdicional, @Rito, @Juiz, @Membro_MP, @Valor_Da_Causa, @Atua_MP, @Atua_Curador_Especial, @idprocesso, @idorgaoministerial)";
            _dbConnection.Execute(sql, processoJudicial);
        }

        // Método Read - Retorna um processo judicial com base em seu ID
        public ProcessoJudicialENT GetProcessoJudicialById(int idProcesso)
        {
            var sql = "SELECT * FROM processo_judicial WHERE idprocesso = @Id";
            return _dbConnection.QuerySingleOrDefault<ProcessoJudicialENT>(sql, new { Id = idProcesso });
        }

        // Método Read - Retorna o id_processo_judicial com base no idProcesso
        public int GetIdProcessoJudicialByIdProcesso(int idProcesso)
        {
            var sql = "SELECT id_processo_judicial FROM processo_judicial WHERE idprocesso = @Id";
            return _dbConnection.ExecuteScalar<int>(sql, new { Id = idProcesso });
        }

        public ProcessoJudicialENT GetProcessoJudicialByIdProcesso(int idProcesso)
        {
            return _dbConnection.QueryFirstOrDefault<ProcessoJudicialENT>("SELECT * FROM ProcessoJudicial WHERE idProcesso = @Id", new { Id = idProcesso });
        }


        // Método Update - Atualiza um processo judicial existente
        public void AtualizarProcessoJudicial(ProcessoJudicialENT processoJudicial)
        {
            var sql = "UPDATE processo_judicial SET tribunal_distr = @Tribunal_Distr, vara_distr = @Vara_Distr, data_despacho = @Data_Despacho, unidade_jurisdicional = @Unidade_Jurisdicional, rito = @Rito, juiz = @Juiz, membro_mp = @Membro_MP, valor_da_causa = @Valor_Da_Causa, atua_mp = @Atua_MP, atua_curador_especial = @Atua_Curador_Especial, idorgaoministerial = @idorgaoministerial WHERE idprocesso = @IdProcesso";
            _dbConnection.Execute(sql, processoJudicial);
        }

        // Método Delete - Exclui um processo judicial
        public void DeletarProcessoJudicial(int idProcesso)
        {
            var sql = "DELETE FROM processo_judicial WHERE idprocesso = @Id";
            _dbConnection.Execute(sql, new { Id = idProcesso });
        }

    }
}