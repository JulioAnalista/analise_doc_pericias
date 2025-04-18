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
    public class ProvasDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ProvasDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(ProvasENT entidade)
        {
            var sql = $@"INSERT INTO provas (Id_prova, Id_processo, DATE_juntada, Id_juntada, Id_no_processo, Documento)
                VALUES (@Id_prova, @Id_processo, @DATE_juntada, @Id_juntada, @Id_no_processo, @Documento);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddprovasENT(ProvasENT entidade)
        {
            var sql = $@"INSERT INTO provas (Id_prova, Id_processo, DATE_juntada, Id_juntada, Id_no_processo, Documento)
                        VALUES (@Id_prova, @Id_processo, @DATE_juntada, @Id_juntada, @Id_no_processo, @Documento)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ProvasENT> GetprovasENTs()
        {
            const string sql = "SELECT * FROM provas";
            return _dbConnection.Query<ProvasENT>(sql).ToList();
        }

        public async Task<IEnumerable<ProvasENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM provas";
            return await _dbConnection.QueryAsync<ProvasENT>(sql);
        }

        public void UpdateprovasENT(ProvasENT entidade)
        {
            var sql = $@"UPDATE provas SET Id_prova = @Id_prova, Id_processo = @Id_processo, DATE_juntada = @DATE_juntada, Id_juntada = @Id_juntada, Id_no_processo = @Id_no_processo, Documento = @Documento
                        WHERE Id_prova = @Id_prova";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteprovasENT(int id)
        {
            var sql = $@"DELETE FROM provas WHERE Id_prova = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}