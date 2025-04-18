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
    public class TipoDependenciaDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TipoDependenciaDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TipoDependenciaENT entidade)
        {
            var sql = $@"INSERT INTO tipo_dependencia (Id_Tipo_dependencia, Tipo_dependencia)
                VALUES (@Id_Tipo_dependencia, @Tipo_dependencia);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addtipo_dependenciaENT(TipoDependenciaENT entidade)
        {
            var sql = $@"INSERT INTO tipo_dependencia (Id_Tipo_dependencia, Tipo_dependencia)
                        VALUES (@Id_Tipo_dependencia, @Tipo_dependencia)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TipoDependenciaENT> Gettipo_dependenciaENTs()
        {
            const string sql = "SELECT * FROM tipo_dependencia";
            return _dbConnection.Query<TipoDependenciaENT>(sql).ToList();
        }

        public async Task<IEnumerable<TipoDependenciaENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tipo_dependencia";
            return await _dbConnection.QueryAsync<TipoDependenciaENT>(sql);
        }

        public void Updatetipo_dependenciaENT(TipoDependenciaENT entidade)
        {
            var sql = $@"UPDATE tipo_dependencia SET Id_Tipo_dependencia = @Id_Tipo_dependencia, Tipo_dependencia = @Tipo_dependencia
                        WHERE Id_Tipo_dependencia = @Id_Tipo_dependencia";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deletetipo_dependenciaENT(int id)
        {
            var sql = $@"DELETE FROM tipo_dependencia WHERE Id_Tipo_dependencia = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}