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
    public class RedeSocialDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public RedeSocialDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(RedeSocialENT entidade)
        {
            var sql = $@"INSERT INTO rede_social (Id_rede, Id_pessoa, Rede, Endereco)
                VALUES (@Id_rede, @Id_pessoa, @Rede, @Endereco);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addrede_socialENT(RedeSocialENT entidade)
        {
            var sql = $@"INSERT INTO rede_social (Id_rede, Id_pessoa, Rede, Endereco)
                        VALUES (@Id_rede, @Id_pessoa, @Rede, @Endereco)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<RedeSocialENT> Getrede_socialENTs()
        {
            const string sql = "SELECT * FROM rede_social";
            return _dbConnection.Query<RedeSocialENT>(sql).ToList();
        }

        public async Task<IEnumerable<RedeSocialENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM rede_social";
            return await _dbConnection.QueryAsync<RedeSocialENT>(sql);
        }

        public void Updaterede_socialENT(RedeSocialENT entidade)
        {
            var sql = $@"UPDATE rede_social SET Id_rede = @Id_rede, Id_pessoa = @Id_pessoa, Rede = @Rede, Endereco = @Endereco
                        WHERE Id_rede = @Id_rede";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deleterede_socialENT(int id)
        {
            var sql = $@"DELETE FROM rede_social WHERE Id_rede = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}