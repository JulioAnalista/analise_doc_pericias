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
    public class ProcessoAdministrativoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ProcessoAdministrativoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(ProcessoAdministrativoENT entidade)
        {
            var sql = $@"INSERT INTO processoadministrativo (Id_proc_adm, Membro_responsavel, Numero_interessados)
                VALUES (@Id_proc_adm, @Membro_responsavel, @Numero_interessados);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddprocessoadministrativoENT(ProcessoAdministrativoENT entidade)
        {
            var sql = $@"INSERT INTO processoadministrativo (Id_proc_adm, Membro_responsavel, Numero_interessados)
                        VALUES (@Id_proc_adm, @Membro_responsavel, @Numero_interessados)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ProcessoAdministrativoENT> GetprocessoadministrativoENTs()
        {
            const string sql = "SELECT * FROM processoadministrativo";
            return _dbConnection.Query<ProcessoAdministrativoENT>(sql).ToList();
        }

        public async Task<IEnumerable<ProcessoAdministrativoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM processoadministrativo";
            return await _dbConnection.QueryAsync<ProcessoAdministrativoENT>(sql);
        }

        public void UpdateprocessoadministrativoENT(ProcessoAdministrativoENT entidade)
        {
            var sql = $@"UPDATE processoadministrativo SET Id_proc_adm = @Id_proc_adm, Membro_responsavel = @Membro_responsavel, Numero_interessados = @Numero_interessados
                        WHERE Id_proc_adm = @Id_proc_adm";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteprocessoadministrativoENT(int id)
        {
            var sql = $@"DELETE FROM processoadministrativo WHERE Id_proc_adm = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}