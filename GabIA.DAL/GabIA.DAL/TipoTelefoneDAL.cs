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
    public class TipoTelefoneDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TipoTelefoneDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TipoTelefoneENT entidade)
        {
            var sql = $@"INSERT INTO tipo_telefone (idtipotel, tipo)
                VALUES (@idtipotel, @tipo);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addtipo_telefoneENT(TipoTelefoneENT entidade)
        {
            var sql = $@"INSERT INTO tipo_telefone (idtipotel, tipo)
                        VALUES (@idtipotel, @tipo)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TipoTelefoneENT> Gettipo_telefoneENTs()
        {
            const string sql = "SELECT * FROM tipo_telefone";
            return _dbConnection.Query<TipoTelefoneENT>(sql).ToList();
        }

        public async Task<IEnumerable<TipoTelefoneENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tipo_telefone";
            return await _dbConnection.QueryAsync<TipoTelefoneENT>(sql);
        }

        public void Updatetipo_telefoneENT(TipoTelefoneENT entidade)
        {
            var sql = $@"UPDATE tipo_telefone SET idtipotel = @idtipotel, tipo = @tipo
                        WHERE idtipotel = @idtipotel";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deletetipo_telefoneENT(int id)
        {
            var sql = $@"DELETE FROM tipo_telefone WHERE idtipotel = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}