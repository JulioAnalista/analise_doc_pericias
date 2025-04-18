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
    public class TipoDocumentoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public TipoDocumentoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(TipoDocumentoENT entidade)
        {
            var sql = $@"INSERT INTO tipo_documento (idtipodoc, tipo)
                VALUES (@idtipodoc, @tipo);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void Addtipo_documentoENT(TipoDocumentoENT entidade)
        {
            var sql = $@"INSERT INTO tipo_documento (idtipodoc, tipo)
                        VALUES (@idtipodoc, @tipo)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<TipoDocumentoENT> Gettipo_documentoENTs()
        {
            const string sql = "SELECT * FROM tipo_documento";
            return _dbConnection.Query<TipoDocumentoENT>(sql).ToList();
        }

        public async Task<IEnumerable<TipoDocumentoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM tipo_documento";
            return await _dbConnection.QueryAsync<TipoDocumentoENT>(sql);
        }

        public void Updatetipo_documentoENT(TipoDocumentoENT entidade)
        {
            var sql = $@"UPDATE tipo_documento SET idtipodoc = @idtipodoc, tipo = @tipo
                        WHERE idtipodoc = @idtipodoc";

            _dbConnection.Execute(sql, entidade);
        }

        public void Deletetipo_documentoENT(int id)
        {
            var sql = $@"DELETE FROM tipo_documento WHERE idtipodoc = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}