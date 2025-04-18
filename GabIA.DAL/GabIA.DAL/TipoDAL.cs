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
    public class TipoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TipoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TipoENT entidade)
        {
            var sql = $@"INSERT INTO tipo (id_tipo, tipo, data)
                VALUES (@id_tipo, @tipo, @data);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddtipoENT(TipoENT entidade)
        {
            var sql = $@"INSERT INTO tipo (id_tipo, tipo, data)
                        VALUES (@id_tipo, @tipo, @data)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TipoENT> GettipoENTs()
        {
            const string sql = "SELECT * FROM tipo";
            return _dbConnection.Query<TipoENT>(sql).ToList();
        }

        public async Task<IEnumerable<TipoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tipo";
            return await _dbConnection.QueryAsync<TipoENT>(sql);
        }

        public void UpdatetipoENT(TipoENT entidade)
        {
            var sql = $@"UPDATE tipo SET id_tipo = @id_tipo, tipo = @tipo, data = @data
                        WHERE id_tipo = @id_tipo";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletetipoENT(int id)
        {
            var sql = $@"DELETE FROM tipo WHERE id_tipo = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}