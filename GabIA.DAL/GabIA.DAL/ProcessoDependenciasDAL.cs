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
    public class ProcessoDependenciasDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ProcessoDependenciasDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(ProcessoDependenciasENT entidade)
        {
            var sql = $@"INSERT INTO processo_dependencias (Id_dependencia, Id_processo, Id_processo_dependente, Id_tipo_dependendia)
                VALUES (@Id_dependencia, @Id_processo, @Id_processo_dependente, @Id_tipo_dependendia);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addprocesso_dependenciasENT(ProcessoDependenciasENT entidade)
        {
            var sql = $@"INSERT INTO processo_dependencias (Id_dependencia, Id_processo, Id_processo_dependente, Id_tipo_dependendia)
                        VALUES (@Id_dependencia, @Id_processo, @Id_processo_dependente, @Id_tipo_dependendia)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ProcessoDependenciasENT> Getprocesso_dependenciasENTs()
        {
            const string sql = "SELECT * FROM processo_dependencias";
            return _dbConnection.Query<ProcessoDependenciasENT>(sql).ToList();
        }

        public async Task<IEnumerable<ProcessoDependenciasENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM processo_dependencias";
            return await _dbConnection.QueryAsync<ProcessoDependenciasENT>(sql);
        }

        public void Updateprocesso_dependenciasENT(ProcessoDependenciasENT entidade)
        {
            var sql = $@"UPDATE processo_dependencias SET Id_dependencia = @Id_dependencia, Id_processo = @Id_processo, Id_processo_dependente = @Id_processo_dependente, Id_tipo_dependendia = @Id_tipo_dependendia
                        WHERE Id_dependencia = @Id_dependencia";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deleteprocesso_dependenciasENT(int id)
        {
            var sql = $@"DELETE FROM processo_dependencias WHERE Id_dependencia = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}