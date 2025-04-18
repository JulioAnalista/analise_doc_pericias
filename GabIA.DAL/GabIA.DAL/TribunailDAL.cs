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
    public class TribunalDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TribunalDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TribunalENT entidade)
        {
        var sql = $@"INSERT INTO tribunal (Id_tribunal, nome)
            VALUES (@Id_tribunal, @nome);
            SELECT LAST_INSERT_ID();";

        return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddtribunailENT(TribunalENT entidade)
        {
            var sql = $@"INSERT INTO tribunail (Id_tribunal, nome)
                        VALUES (@Id_tribunal, @nome)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TribunalENT> GettribunailENTs()
        {
            const string sql = "SELECT * FROM tribunail";
            return _dbConnection.Query<TribunalENT>(sql).ToList();
        }

        public async Task<IEnumerable<TribunalENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tribunail";
            return await _dbConnection.QueryAsync<TribunalENT>(sql);
        }

        public void UpdatetribunailENT(TribunalENT entidade)
        {
            var sql = $@"UPDATE tribunail SET Id_tribunal = @Id_tribunal, nome = @nome
                        WHERE Id_tribunal = @Id_tribunal";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletetribunailENT(int id)
        {
            var sql = $@"DELETE FROM tribunail WHERE Id_tribunal = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}