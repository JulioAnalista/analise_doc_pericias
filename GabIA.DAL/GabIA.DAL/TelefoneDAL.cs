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
    public class TelefoneDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TelefoneDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TelefoneENT entidade)
        {
            var sql = $@"INSERT INTO telefone (Id_telefone, Id_pessoa, Numero, Id_tipo)
                VALUES (@Id_telefone, @Id_pessoa, @Numero, @Id_tipo);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddtelefoneENT(TelefoneENT entidade)
        {
            var sql = $@"INSERT INTO telefone (Id_telefone, Id_pessoa, Numero, Id_tipo)
                        VALUES (@Id_telefone, @Id_pessoa, @Numero, @Id_tipo)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TelefoneENT> GettelefoneENTs()
        {
            const string sql = "SELECT * FROM telefone";
            return _dbConnection.Query<TelefoneENT>(sql).ToList();
        }

        public async Task<IEnumerable<TelefoneENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM telefone";
            return await _dbConnection.QueryAsync<TelefoneENT>(sql);
        }

        public void UpdatetelefoneENT(TelefoneENT entidade)
        {
            var sql = $@"UPDATE telefone SET Id_telefone = @Id_telefone, Id_pessoa = @Id_pessoa, Numero = @Numero, Id_tipo = @Id_tipo
                        WHERE Id_telefone = @Id_telefone";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletetelefoneENT(int id)
        {
            var sql = $@"DELETE FROM telefone WHERE Id_telefone = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}