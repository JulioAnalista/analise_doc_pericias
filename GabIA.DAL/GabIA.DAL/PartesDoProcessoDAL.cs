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
using System.Reflection.Metadata;

namespace GabIA.DAL
{
    public class PartesDoProcessoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public PartesDoProcessoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(PartesDoProcessoENT entidade)
        {
        var sql = $@"INSERT INTO partesdoprocesso (IdPartes, IdParte, IdElementos, IdPosicao)
            VALUES (@IdPartes, @IdParte, @IdElementos, @IdPosicao);
            SELECT LAST_INSERT_ID();";

        return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public int InserirOuAtualizarParteFromNeoGab(int idElemento, ProcessoCompletoENT processoCompleto)
        {
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }

            // Verificar se existe um registro para o idElemento
            var sqlExistente = "SELECT IdPartes FROM PartesDoProcesso WHERE IdElemento = @IdElemento";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { IdElemento = idElemento }).FirstOrDefault();

            if (idExistente.HasValue)
            {
                // Atualizar o registro existente com o idElemento
                //var sqlAtualizar = "UPDATE PartesDoProcesso SET IdElemento = @IdElemento WHERE IdPartes = @IdPartes";
                //_dbConnection.Execute(sqlAtualizar, new { IdElemento = idElemento, IdPartes = idExistente.Value });
                return idExistente.Value;
            }
            else
            {
                // Inserir um novo registro com o idElemento
                var sqlInserir = "INSERT INTO PartesDoProcesso (IdElemento) VALUES (@IdElemento); SELECT LAST_INSERT_ID();";
                var idNovo = _dbConnection.Query<int>(sqlInserir, new { IdElemento = idElemento }).Single();
                return idNovo;
            }
        }
        // na classe PartesdoProcessoDAL
        public List<PartesDoProcessoENT> ObterPartesdoProcesso(int idElemento)
        {
            // Agora usamos este idprocesso para obter as partes do processo
            var sql = @"
                SELECT * FROM partesdoprocesso
                WHERE IdElementos = @IdProcesso";
            var partesDoProcesso = _dbConnection.Query<PartesDoProcessoENT>(sql, new { IdProcesso = idElemento }).ToList();

            return partesDoProcesso;
        }


        public string GetNomeParteByIdElementosAndIdPosicao(int idElementos, int idPosicao)
        {
            return _dbConnection.QueryFirstOrDefault<string>(
                "SELECT p.Nome FROM Pessoa p " +
                "JOIN PartesdoProcesso pp ON p.id_pessoa = pp.idParte " +
                "WHERE pp.IdElementos = @IdElementos AND pp.IdPosicao = @IdPosicao",
                new { IdElementos = idElementos, IdPosicao = idPosicao });
        }


        public void AddpartesdoprocessoENT(PartesDoProcessoENT entidade)
        {
            var sql = $@"INSERT INTO partesdoprocesso (IdPartes, IdParte, IdElementos, IdPosicao)
                        VALUES (@IdPartes, @IdParte, @IdElementos, @IdPosicao)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<PartesDoProcessoENT> GetPartesDoProcessoByElementoId(int idElemento)
        {
            const string sql = "SELECT * FROM partesdoprocesso WHERE IdElemento = @IdElemento";
            return _dbConnection.Query<PartesDoProcessoENT>(sql, new { IdElemento = idElemento }).ToList();
        }




        public List<PartesDoProcessoENT> GetpartesdoprocessoENTs()
        {
            const string sql = "SELECT * FROM partesdoprocesso";
            return _dbConnection.Query<PartesDoProcessoENT>(sql).ToList();
        }

        public async Task<IEnumerable<PartesDoProcessoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM partesdoprocesso";
            return await _dbConnection.QueryAsync<PartesDoProcessoENT>(sql);
        }

        public void UpdatepartesdoprocessoENT(PartesDoProcessoENT entidade)
        {
            var sql = $@"UPDATE partesdoprocesso SET IdPartes = @IdPartes, IdParte = @IdParte, IdElementos = @IdElementos, IdPosicao = @IdPosicao
                        WHERE  = @";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletepartesdoprocessoENT(int id)
        {
            var sql = $@"DELETE FROM partesdoprocesso WHERE  = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}