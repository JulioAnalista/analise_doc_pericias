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
    public class TipoAtoProcessualDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TipoAtoProcessualDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public int Inserir(TipoAtoProcessualENT entidade)
        {
            var sql = $@"INSERT INTO tipo_ato_processual (Id_Tipo_Ato_processual, Tipo)
                VALUES (@Id_Tipo_Ato_processual, @Tipo);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addtipo_ato_processualENT(TipoAtoProcessualENT entidade)
        {
            var sql = $@"INSERT INTO tipo_ato_processual (Id_Tipo_Ato_processual, Tipo)
                        VALUES (@Id_Tipo_Ato_processual, @Tipo)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TipoAtoProcessualENT> Gettipo_ato_processualENTs()
        {
            const string sql = "SELECT * FROM tipo_ato_processual";
            return _dbConnection.Query<TipoAtoProcessualENT>(sql).ToList();
        }


        public int GetId_ato_processualByTipo(string tipo)
        {
            const string sql = "SELECT id_tipo_ato_Processual FROM tipo_ato_processual Where Tipo = @_tipo";
            return _dbConnection.ExecuteScalar<int>(sql, new { _tipo = tipo});
        }


        public async Task<IEnumerable<TipoAtoProcessualENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tipo_ato_processual";
            return await _dbConnection.QueryAsync<TipoAtoProcessualENT>(sql);
        }

        public void Updatetipo_ato_processualENT(TipoAtoProcessualENT entidade)
        {
            var sql = $@"UPDATE tipo_ato_processual SET Id_Tipo_Ato_processual = @Id_Tipo_Ato_processual, Tipo = @Tipo
                        WHERE Id_Tipo_Ato_processual = @Id_Tipo_Ato_processual";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deletetipo_ato_processualENT(int id)
        {
            var sql = $@"DELETE FROM tipo_ato_processual WHERE Id_Tipo_Ato_processual = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}