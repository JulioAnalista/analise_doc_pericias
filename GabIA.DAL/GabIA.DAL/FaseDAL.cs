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

namespace GabIA.DAL
{
    public class FaseDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public FaseDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(FaseENT entidade)
        {
            var sql = $@"INSERT INTO fase (IdFaseKey, IdProcesso, Data, IdFase)
                VALUES (@IdFaseKey, @IdProcesso, @Data, @IdFase);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddfaseENT(FaseENT entidade)
        {
            var sql = $@"INSERT INTO fase (IdFaseKey, IdProcesso, Data, IdFase)
                        VALUES (@IdFaseKey, @IdProcesso, @Data, @IdFase)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<FaseENT> GetfaseENTs()
        {
            const string sql = "SELECT * FROM fase";
            return _dbConnection.Query<FaseENT>(sql).ToList();
        }

        public async Task<IEnumerable<FaseENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM fase";
            return await _dbConnection.QueryAsync<FaseENT>(sql);
        }

        public void UpdatefaseENT(FaseENT entidade)
        {
            var sql = $@"UPDATE fase SET IdFaseKey = @IdFaseKey, IdProcesso = @IdProcesso, Data = @Data, IdFase = @IdFase
                        WHERE IdFaseKey = @IdFaseKey";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletefaseENT(int id)
        {
            var sql = $@"DELETE FROM fase WHERE IdFaseKey = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}